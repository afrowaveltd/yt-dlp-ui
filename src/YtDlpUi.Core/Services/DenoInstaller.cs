using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace YtDlpUi.Core.Services;

/// <summary>
/// User-scoped, SHA-256-verified Deno installer for the yt-dlp YouTube EJS solver.
/// Downloads only when the user explicitly requests it; never needs elevation.
/// </summary>
public sealed class DenoInstaller : IDisposable
{
    private static readonly Version MinimumVersion = new(2, 3, 0);
    private const string ReleaseRoot = "https://github.com/denoland/deno/releases/latest/download/";
    private const long MaximumArchiveBytes = 250L * 1024 * 1024;
    private const long MaximumExecutableBytes = 350L * 1024 * 1024;
    private readonly HttpClient _http;
    private readonly bool _ownsHttp;

    /// <summary>Create a verified Deno installer. Inject HTTP client for offline tests.</summary>
    public DenoInstaller(HttpClient? http = null)
    {
        _ownsHttp = http is null;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromMinutes(20) };
        if (!_http.DefaultRequestHeaders.UserAgent.Any())
        {
            _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("yt-dlp-ui", "0.1"));
        }
    }

    /// <summary>Available for x64 and arm64 desktop Linux/Windows.</summary>
    public static bool IsSupported =>
        (OperatingSystem.IsLinux() || OperatingSystem.IsWindows()) &&
        (RuntimeInformation.ProcessArchitecture is Architecture.X64 or Architecture.Arm64);

    /// <summary>Official release artifact for the current OS and architecture.</summary>
    public static string? AssetName()
    {
        var cpu = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x86_64",
            Architecture.Arm64 => "aarch64",
            _ => null
        };
        if (cpu is null) return null;
        if (OperatingSystem.IsLinux()) return $"deno-{cpu}-unknown-linux-gnu.zip";
        if (OperatingSystem.IsWindows()) return $"deno-{cpu}-pc-windows-msvc.zip";
        return null;
    }

    /// <summary>Personal install folder, discoverable without PATH changes.</summary>
    public static string DefaultInstallPath
    {
        get
        {
            if (OperatingSystem.IsWindows())
                return Path.Combine(UserPaths.LocalBinDirectory, "deno.exe");

            var prefix = Environment.GetEnvironmentVariable("DENO_INSTALL");
            var root = string.IsNullOrWhiteSpace(prefix)
                ? Path.Combine(UserPaths.Home, ".deno")
                : ToolLocator.ExpandHome(prefix.Trim());
            return Path.Combine(root, "bin", "deno");
        }
    }

    /// <summary>Parse publisher SHA256 sidecar (bare hash or matching filename).</summary>
    public static string PublishedSha256(string checksum, string asset)
    {
        ArgumentNullException.ThrowIfNull(checksum);
        var matches = new List<string>();
        foreach (var line in checksum.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var value = line.Trim();
            var match = Regex.Match(value, @"^([0-9a-fA-F]{64})(?:\s+\*?(.+))?$");
            if (!match.Success) continue;
            var filename = match.Groups[2].Value.Trim();
            if (filename.Length == 0 || filename.Equals(asset, StringComparison.Ordinal))
            {
                matches.Add(match.Groups[1].Value);
            }
        }

        if (matches.Count != 1) throw new InvalidDataException("Missing or ambiguous Deno publisher SHA-256 checksum.");
        return matches[0];
    }

    /// <summary>Return a compatible Deno version only when it really runs.</summary>
    public static async Task<string?> CheckVersionAsync(string? executable, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executable) || !File.Exists(executable))
        {
            return null;
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            var result = await ProcessRunner.RunAsync(executable, ["--version"], timeout.Token).ConfigureAwait(false);
            if (result.ExitCode != 0) return null;

            var match = Regex.Match(result.StandardOutput, @"(?m)^deno\s+(\d+\.\d+\.\d+)(?:\S*)");
            if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var version) ||
                version < MinimumVersion) return null;

            return version.ToString();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or InvalidOperationException or OperationCanceledException or System.ComponentModel.Win32Exception)
        {
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            return null;
        }
    }

    /// <summary>Probe actual Deno command, without downloading or changing files.</summary>
    public static async Task<ToolStatus> LocateAsync(CancellationToken cancellationToken = default)
    {
        var path = JavaScriptRuntimeLocator.FindDenoExecutable();
        var version = await CheckVersionAsync(path, cancellationToken).ConfigureAwait(false);
        return version is null
            ? ToolStatus.Missing("Deno", path is null
                ? "Deno was not found (2.3.0+ recommended for YouTube)."
                : $"Deno at {path} could not run or is older than 2.3.0.")
            : ToolStatus.Ok("Deno", path!, version);
    }

    /// <summary>Install a validated private copy, preserving the previous copy on failure.</summary>
    public async Task<string> InstallAsync(IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var asset = AssetName() ?? throw new PlatformNotSupportedException("Unsupported Deno platform.");
        var archivePath = Path.Combine(Path.GetTempPath(), "afrowave-deno-" + Guid.NewGuid().ToString("N") + ".zip");
        var directory = Path.GetDirectoryName(DefaultInstallPath)!;
        Directory.CreateDirectory(directory);
        var target = DefaultInstallPath;
        // Windows requires .exe extension to execute and validate the staged binary.
        var staging = Path.Combine(directory, ".deno-stage-" + Guid.NewGuid().ToString("N")
            + (OperatingSystem.IsWindows() ? ".exe" : string.Empty));
        var backup = Path.Combine(directory, ".deno-backup-" + Guid.NewGuid().ToString("N"));
        bool oldMoved = false;
        bool installedSuccessfully = false;
        try
        {
            var checksumUrl = new Uri(ReleaseRoot + asset + ".sha256sum");
            using var checksumResponse = await _http.GetAsync(checksumUrl, cancellationToken).ConfigureAwait(false);
            checksumResponse.EnsureSuccessStatusCode();
            var checksumText = await checksumResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var expected = PublishedSha256(checksumText, asset);

            using (var response = await _http.GetAsync(new Uri(ReleaseRoot + asset),
                HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength > MaximumArchiveBytes)
                    throw new InvalidDataException("Deno archive is too large.");

                await using var input = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await using var output = new FileStream(archivePath, FileMode.CreateNew, FileAccess.Write,
                    FileShare.None, 81920, useAsync: true);
                var buffer = new byte[81920];
                long totalRead = 0;
                int read;
                while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    totalRead += read;
                    if (totalRead > MaximumArchiveBytes)
                        throw new InvalidDataException("Deno archive exceeds the allowed size.");
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                    var total = response.Content.Headers.ContentLength;
                    progress?.Report(new DownloadProgress(totalRead, total,
                        total is > 0 ? 100.0 * totalRead / total : null));
                }
            }

            await using (var archiveStream = File.OpenRead(archivePath))
            {
                var actual = Convert.ToHexString(await SHA256.HashDataAsync(archiveStream, cancellationToken).ConfigureAwait(false));
                if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Deno SHA-256 mismatch. Existing installation was preserved.");
            }

            using (var zip = ZipFile.OpenRead(archivePath))
            {
                var name = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
                var entries = zip.Entries.Where(entry => entry.FullName == name).ToArray();
                if (entries.Length != 1 || entries[0].Length <= 0 || entries[0].Length > MaximumExecutableBytes)
                    throw new InvalidDataException("Deno archive does not contain one valid executable.");

                await using var fromZip = entries[0].Open();
                await using var extracted = new FileStream(staging, FileMode.CreateNew, FileAccess.Write,
                    FileShare.None, 81920, useAsync: true);
                await fromZip.CopyToAsync(extracted, cancellationToken).ConfigureAwait(false);
            }

            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(staging, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                    | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            }

            if (await CheckVersionAsync(staging, cancellationToken).ConfigureAwait(false) is null)
                throw new InvalidDataException("Deno did not start or does not meet the minimum version 2.3.0.");

            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(target))
            {
                File.Move(target, backup);
                oldMoved = true;
            }

            try
            {
                File.Move(staging, target);
                if (await CheckVersionAsync(target, cancellationToken).ConfigureAwait(false) is null)
                    throw new InvalidDataException("New Deno failed post-install verification.");
            }
            catch
            {
                if (oldMoved && File.Exists(backup))
                {
                    if (File.Exists(target)) File.Delete(target);
                    File.Move(backup, target);
                    oldMoved = false;
                }
                else if (File.Exists(target)) File.Delete(target);
                throw;
            }

            installedSuccessfully = true;
            return target;
        }
        finally
        {
            if (File.Exists(archivePath)) File.Delete(archivePath);
            if (File.Exists(staging)) File.Delete(staging);
            // Never delete the old binary if rollback failed. Preserve it on disk
            // for manual recovery; remove backups only after a verified install.
            if (installedSuccessfully && oldMoved && File.Exists(backup))
            {
                try { File.Delete(backup); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_ownsHttp) _http.Dispose();
    }
}
