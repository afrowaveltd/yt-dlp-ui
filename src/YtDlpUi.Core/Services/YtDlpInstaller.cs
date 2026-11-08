using System.Net.Http.Headers;
using System.Runtime.InteropServices;
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

    public YtDlpInstaller(HttpClient? http = null)
    {
        _http = http ?? new HttpClient();
        _http.Timeout = TimeSpan.FromMinutes(30);
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

    /// <summary>Download the official standalone binary and make it executable.</summary>
    public async Task<string> DownloadStandaloneAsync(
        string? destinationPath = null,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var asset = StandaloneAssetName();
        var url = new Uri($"{ReleasesBase}/latest/download/{asset}");
        var destination = destinationPath ?? DefaultInstallPath();

        var directory = Path.GetDirectoryName(destination);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempFile = destination + ".download";
        using (var response = await _http
                         .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                         .ConfigureAwait(false))
        {
            response.EnsureSuccessStatusCode();
            var total = response.Content.Headers.ContentLength;

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var target = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

            var buffer = new byte[81920];
            long received = 0;
            int read;

            while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                received += read;
                progress?.Report(new DownloadProgress(
                    received,
                    total,
                    total is > 0 ? received * 100d / total.Value : null));
            }
        }

        if (File.Exists(destination))
        {
            File.Delete(destination);
        }

        File.Move(tempFile, destination);

        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(destination, File.GetUnixFileMode(destination) | ExecutableBits);
        }

        progress?.Report(new DownloadProgress(new FileInfo(destination).Length, null, 100));
        return destination;
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
    public void Dispose() => _http.Dispose();
}
