using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace YtDlpUi.Core.Services;

/// <summary>Verified, per-user FFmpeg/ffprobe installer for Windows x64.</summary>
public sealed class FfmpegInstaller : IDisposable
{
    private static readonly Uri ArchiveUrl = new("https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip");
    private static readonly Uri ChecksumUrl = new("https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip.sha256");
    private readonly HttpClient _client = new() { Timeout = TimeSpan.FromMinutes(30) };

    /// <summary>Only the published Windows x64 ZIP is installed automatically.</summary>
    public static bool IsSupported => OperatingSystem.IsWindows() &&
        RuntimeInformation.OSArchitecture == Architecture.X64;

    /// <summary>Download, validate SHA-256, then install a private copy of FFmpeg and FFprobe.</summary>
    public async Task<string> InstallAsync(
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsSupported)
        {
            throw new PlatformNotSupportedException("Automatic FFmpeg installation currently supports Windows x64.");
        }

        var root = UserPaths.LocalBinDirectory;
        Directory.CreateDirectory(root);
        var archive = Path.Combine(root, "ffmpeg-" + Guid.NewGuid().ToString("N") + ".zip");
        var stage = Path.Combine(root, "ffmpeg-" + Guid.NewGuid().ToString("N") + ".stage");
        var destination = UserPaths.ManagedFfmpegDirectory;
        try
        {
            // Stop rather than execute unverified code when the checksum is missing.
            var checksumText = await _client.GetStringAsync(ChecksumUrl, cancellationToken).ConfigureAwait(false);
            var match = Regex.Match(checksumText, @"(?i)\b[0-9a-f]{64}\b");
            if (!match.Success)
            {
                throw new InvalidDataException("FFmpeg SHA-256 checksum was not found.");
            }

            using (var response = await _client.GetAsync(
                       ArchiveUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var total = response.Content.Headers.ContentLength;
                await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                await using var output = new FileStream(archive, FileMode.CreateNew, FileAccess.Write,
                    FileShare.None, 81920, useAsync: true);
                var buffer = new byte[81920];
                long received = 0;
                int size;
                while ((size = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, size), cancellationToken).ConfigureAwait(false);
                    received += size;
                    progress?.Report(new DownloadProgress(received, total,
                        total > 0 ? received * 100d / total.Value : null));
                }
            }

            await using (var stream = File.OpenRead(archive))
            {
                var digest = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false));
                if (!digest.Equals(match.Value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("FFmpeg ZIP SHA-256 verification failed.");
                }
            }

            Directory.CreateDirectory(stage);
            var copied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var zip = ZipFile.OpenRead(archive))
            {
                foreach (var entry in zip.Entries)
                {
                    var normalizedPath = entry.FullName.Replace('\\', '/');
                    if (!normalizedPath.Contains("/bin/", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var filename = Path.GetFileName(normalizedPath);
                    if (filename is not ("ffmpeg.exe" or "ffprobe.exe"))
                    {
                        continue;
                    }

                    if (entry.Length > 350_000_000 || !copied.Add(filename))
                    {
                        throw new InvalidDataException("Unexpected or duplicate FFmpeg executable in the archive.");
                    }

                    // Extract only the two expected file names: never arbitrary ZIP paths.
                    entry.ExtractToFile(Path.Combine(stage, filename));
                }
            }

            if (copied.Count != 2)
            {
                throw new InvalidDataException("FFmpeg archive did not contain both expected executables.");
            }

            // Keep the previous working FFmpeg installation until the new copy
            // is ready, and restore it if moving the staged files fails.
            FileDeployment.ReplaceDirectory(stage, destination);
            return destination;
        }
        finally
        {
            if (File.Exists(archive))
            {
                File.Delete(archive);
            }

            if (Directory.Exists(stage))
            {
                Directory.Delete(stage, recursive: true);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() => _client.Dispose();
}
