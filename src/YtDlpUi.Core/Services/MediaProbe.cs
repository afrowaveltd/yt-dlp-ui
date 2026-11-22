using System.Globalization;
using System.Text.Json;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

/// <summary>Reads technical metadata for a local file using <c>ffprobe</c>.</summary>
public static class MediaProbe
{
    /// <summary>Probe one file. Returns null when ffprobe cannot read it.</summary>
    public static async Task<MediaProfile?> ProbeAsync(
        string ffprobePath,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ProcessRunner.RunAsync(
                    ffprobePath,
                    ["-v", "error", "-print_format", "json", "-show_format", "-show_streams", filePath],
                    cancellationToken)
                .ConfigureAwait(false);

            if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.StandardOutput))
            {
                return null;
            }

            return ParseFfprobeJson(result.StandardOutput, filePath);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Map ffprobe's JSON document onto a <see cref="MediaProfile"/>.
    /// Missing or malformed fields are treated as unknown instead of failing the whole probe.
    /// </summary>
    public static MediaProfile ParseFfprobeJson(string json, string filePath)
    {
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        });

        var root = document.RootElement;
        JsonElement format = default;
        var hasFormat = root.ValueKind == JsonValueKind.Object &&
                        root.TryGetProperty("format", out format) &&
                        format.ValueKind == JsonValueKind.Object;

        long size = 0;
        double? duration = null;
        string? container = null;
        double? formatBitrate = null;

        if (hasFormat)
        {
            size = GetLong(format, "size") ?? 0;
            duration = GetDouble(format, "duration");
            container = GetString(format, "format_name");
            formatBitrate = GetDouble(format, "bit_rate") is { } bps ? bps / 1000d : null;
        }

        if (size <= 0)
        {
            try
            {
                size = new FileInfo(filePath).Length;
            }
            catch (Exception)
            {
                size = 0;
            }
        }

        string? videoCodec = null;
        int? width = null;
        int? height = null;
        double? frameRate = null;
        double? videoBitrate = null;
        string? audioCodec = null;
        int? channels = null;
        int? sampleRate = null;
        double? audioBitrate = null;
        var subtitleStreams = 0;
        var hasVideo = false;
        var hasAudio = false;

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("streams", out var streams) &&
            streams.ValueKind == JsonValueKind.Array)
        {
            foreach (var stream in streams.EnumerateArray())
            {
                if (stream.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var type = GetString(stream, "codec_type");

                if (string.Equals(type, "video", StringComparison.OrdinalIgnoreCase))
                {
                    // Cover art is exposed as a video stream; ignore it.
                    if (IsAttachedPicture(stream))
                    {
                        continue;
                    }

                    if (!hasVideo)
                    {
                        hasVideo = true;
                        videoCodec = GetString(stream, "codec_name");
                        width = (int?)GetLong(stream, "width");
                        height = (int?)GetLong(stream, "height");
                        frameRate = ParseFrameRate(GetString(stream, "avg_frame_rate") ?? GetString(stream, "r_frame_rate"));
                        videoBitrate = GetDouble(stream, "bit_rate") is { } videoBps ? videoBps / 1000d : null;
                    }
                }
                else if (string.Equals(type, "audio", StringComparison.OrdinalIgnoreCase))
                {
                    if (!hasAudio)
                    {
                        hasAudio = true;
                        audioCodec = GetString(stream, "codec_name");
                        channels = (int?)GetLong(stream, "channels");
                        sampleRate = (int?)GetLong(stream, "sample_rate");
                        audioBitrate = GetDouble(stream, "bit_rate") is { } audioBps ? audioBps / 1000d : null;
                    }
                }
                else if (string.Equals(type, "subtitle", StringComparison.OrdinalIgnoreCase))
                {
                    subtitleStreams++;
                }
            }
        }

        return new MediaProfile
        {
            FilePath = filePath,
            SizeBytes = size,
            DurationSeconds = duration,
            ContainerFormat = container,
            HasVideo = hasVideo,
            HasAudio = hasAudio,
            VideoCodec = videoCodec,
            Width = width,
            Height = height,
            FrameRate = frameRate,
            VideoBitrateKbps = videoBitrate ?? formatBitrate,
            AudioCodec = audioCodec,
            AudioChannels = channels,
            SampleRate = sampleRate,
            AudioBitrateKbps = audioBitrate,
            SubtitleStreamCount = subtitleStreams,
        };
    }

    /// <summary>Parse ffprobe's <c>30000/1001</c> frame-rate notation.</summary>
    public static double? ParseFrameRate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Split('/');
        if (parts.Length == 2 &&
            double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var numerator) &&
            double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var denominator) &&
            denominator > 0)
        {
            var rate = numerator / denominator;
            return rate is > 0 and < 1000 ? rate : null;
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var simple) && simple is > 0 and < 1000
            ? simple
            : null;
    }

    /// <summary>True when the stream is embedded cover art rather than real video.</summary>
    private static bool IsAttachedPicture(JsonElement stream) =>
        stream.TryGetProperty("disposition", out var disposition) &&
        disposition.ValueKind == JsonValueKind.Object &&
        disposition.TryGetProperty("attached_pic", out var picture) &&
        picture.ValueKind == JsonValueKind.Number &&
        picture.TryGetInt32(out var flag) &&
        flag == 1;

    private static string? GetString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            _ => null,
        };
    }

    private static double? GetDouble(JsonElement element, string property)
    {
        var text = GetString(element, property);
        return text is not null &&
               double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static long? GetLong(JsonElement element, string property)
    {
        var value = GetDouble(element, property);
        return value is null ? null : (long)value.Value;
    }
}
