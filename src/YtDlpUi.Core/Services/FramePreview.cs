namespace YtDlpUi.Core.Services;

/// <summary>
/// Grabs a single still frame from a media file so the player window can show what a file
/// looks like at any position without decoding the whole stream.
/// </summary>
public static class FramePreview
{
    /// <summary>Default preview height; large enough to be readable, small enough to stay fast.</summary>
    public const int DefaultHeight = 360;

    /// <summary>
    /// Render one PNG frame at <paramref name="positionSeconds"/>.
    /// Returns null for audio-only files, unreadable files or a failed FFmpeg run.
    /// </summary>
    public static async Task<byte[]?> GrabPngAsync(
        string? ffmpegPath,
        string? filePath,
        double positionSeconds,
        int maxHeight = DefaultHeight,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ffmpegPath) ||
            string.IsNullOrWhiteSpace(filePath) ||
            !File.Exists(filePath))
        {
            return null;
        }

        var seconds = Math.Max(0, positionSeconds);
        var height = Math.Clamp(maxHeight, 90, 1080);

        var arguments = new List<string>
        {
            "-hide_banner",
            "-loglevel", "error",
            "-nostdin",
            // Seeking before -i keeps frame grabs near-instant on long files.
            "-ss", seconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            "-i", filePath!,
            "-frames:v", "1",
            "-vf", $"scale=-2:min(ih\\,{height})",
            "-f", "image2pipe",
            "-vcodec", "png",
            "-",
        };

        try
        {
            var (exitCode, output, _) = await ProcessRunner
                .RunForBytesAsync(ffmpegPath!, arguments, cancellationToken)
                .ConfigureAwait(false);

            return exitCode == 0 && output.Length > 0 ? output : null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
