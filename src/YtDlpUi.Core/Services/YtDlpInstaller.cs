using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace YtDlpUi.Core.Services;

/// <summary>Progress of a binary download.</summary>
public readonly record struct DownloadProgress(long ReceivedBytes, long? TotalBytes, double? Percent);

/// <summary>
/// Downloads, installs and updates yt-dlp for machines that do not have it yet.
/// Only friendly, already-trusted sources are used: the official GitHub releases and pip.
/// </summary>
public sealed class YtDlpInstaller : IDisposable
{
    private const string ReleasesBase = "https://github.com/yt-dlp/yt-dlp/releases";
    private static readonly Uri LatestReleaseApi =
        new("https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest");

    private readonly HttpClient _http;
    private readonly bool _ownsHttp;

    public YtDlpInstaller(HttpClient? http = null)
    {
        _ownsHttp = http is null;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        if (!_http.DefaultRequestHeaders.UserAgent.Any())
        {
            _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("yt-dlp-ui", "1.0"));
        }

        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    /// <summary>Name of the official standalone asset for this OS/architecture.</summary>
    public static string StandaloneAssetName()
    {
        if (OperatingSystem.IsWindows())
        {
            return Architecture.Arm64 == RuntimeInformation.ProcessArchitecture
                ? "yt-dlp_arm64.exe"
                : "yt-dlp.exe";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "yt-dlp_macos";
        }

        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm64 => "yt-dlp_linux_aarch64",
            Architecture.Arm => "yt-dlp_linux_armv7l",
            Architecture.X86 => "yt-dlp_linux_x86",
            _ => "yt-dlp_linux",
        };
    }

    /// <summary>Where a downloaded yt-dlp binary should live.</summary>
    public static string DefaultInstallPath()
    {
        var name = OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp";
        return Path.Combine(UserPaths.LocalBinDirectory, name);
    }

    /// <summary>
    /// Find the publisher's SHA-256 value for exactly the selected binary.
    /// Reject missing, ambiguous or malformed manifests instead of installing
    /// an unchecked executable.
    /// </summary>
    public static string ReadPublishedChecksum(string manifest, string assetName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetName);
        ArgumentNullException.ThrowIfNull(manifest);

        var pattern = @"(?im)^[ \t]*([a-f0-9]{64})[ \t]+\*?" +
                      Regex.Escape(assetName) + @"[ \t]*\r?$";
        var matches = Regex.Matches(manifest, pattern);
        if (matches.Count != 1)
        {
            throw new InvalidDataException($"Expected exactly one SHA-256 entry for {assetName}.");
        }

        return matches[0].Groups[1].Value;
    }

    /// <summary>
    /// Download the official standalone binary, verify its published checksum and
    /// replace the previous version only after the entire download is verified.
    /// An interruption leaves the already-installed executable untouched.
    /// </summary>
    public async Task<string> DownloadStandaloneAsync(
        string? destinationPath = null,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var asset = StandaloneAssetName();
        var downloadUrl = new Uri($"{ReleasesBase}/latest/download/{asset}");
        var checksumUrl = new Uri($"{ReleasesBase}/latest/download/SHA2-256SUMS");
        var destination = destinationPath ?? DefaultInstallPath();

        var directory = Path.GetDirectoryName(Path.GetFullPath(destination));
        Directory.CreateDirectory(directory!);

        // The temporary file must live on the same filesystem for an atomic rename.
        // Unique filenames also keep concurrent installation attempts isolated.
        var tempFile = Path.Combine(directory!, "." + Path.GetFileName(destination) +
            "." + Guid.NewGuid().ToString("N") + ".download");

        try
        {
            using var checksumResponse = await _http.GetAsync(checksumUrl, cancellationToken).ConfigureAwait(false);
            checksumResponse.EnsureSuccessStatusCode();
            var manifest = await checksumResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var expected = ReadPublishedChecksum(manifest, asset);

            using (var response = await _http.GetAsync(
                       downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var total = response.Content.Headers.ContentLength;
                await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await using var target = new FileStream(tempFile, FileMode.CreateNew, FileAccess.Write,
                    FileShare.None, 81920, useAsync: true);

                var buffer = new byte[81920];
                long received = 0;
                int read;
                while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    received += read;
                    progress?.Report(new DownloadProgress(
                        received, total, total is > 0 ? received * 100d / total.Value : null));
                }
            }

            await using (var file = File.OpenRead(tempFile))
            {
                var actual = Convert.ToHexString(
                    await SHA256.HashDataAsync(file, cancellationToken).ConfigureAwait(false));
                if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("yt-dlp SHA-256 verification failed. Previous version was kept.");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(tempFile, File.GetUnixFileMode(tempFile) | ExecutableBits);
            }

            // Same-volume overwrite: never delete the old executable before verifying the new one.
            File.Move(tempFile, destination, overwrite: true);
            progress?.Report(new DownloadProgress(new FileInfo(destination).Length, null, 100));
            return destination;
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    /// <summary>Ask GitHub for the newest stable release tag (e.g. <c>2026.08.19</c>).</summary>
    public async Task<string?> GetLatestReleaseVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.GetAsync(LatestReleaseApi, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            return document.RootElement.TryGetProperty("tag_name", out var tag) ? tag.GetString() : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Run <c>yt-dlp --update</c> and stream its output to <paramref name="log"/>.</summary>
    public async Task<int> UpdateAsync(
        YtDlpCommand command,
        Action<string>? log,
        CancellationToken cancellationToken = default)
    {
        return await ProcessRunner
            .RunStreamingAsync(
                command.FileName,
                command.With(["--update", "--color", "never"]),
                log,
                log,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Install through pip/pipx when a standalone binary is not wanted.</summary>
    public async Task<int> InstallViaPipAsync(Action<string>? log, CancellationToken cancellationToken = default)
    {
        var pipx = ToolLocator.FindOnPath("pipx");
        if (pipx is not null)
        {
            return await ProcessRunner
                .RunStreamingAsync(pipx, ["install", "yt-dlp"], log, log, cancellationToken)
                .ConfigureAwait(false);
        }

        var python = ToolLocator.FindOnPath("python3") ?? ToolLocator.FindOnPath("python");
        if (python is null)
        {
            log?.Invoke("Python nebyl nalezen, pip instalace není možná.");
            return -1;
        }

        var args = OperatingSystem.IsWindows()
            ? new List<string> { "-m", "pip", "install", "--upgrade", "yt-dlp" }
            : new List<string> { "-m", "pip", "install", "--user", "--upgrade", "yt-dlp" };

        return await ProcessRunner.RunStreamingAsync(python, args, log, log, cancellationToken).ConfigureAwait(false);
    }

    private const UnixFileMode ExecutableBits =
        UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttp)
        {
            _http.Dispose();
        }
    }
}
