namespace YtDlpUi.Core.Models;

/// <summary>One selectable stream reported by yt-dlp for a video.</summary>
public sealed class MediaFormat
{
    public string? FormatId { get; init; }
    public string? Extension { get; init; }
    public string? Resolution { get; init; }
    public double? Fps { get; init; }
    public string? VideoCodec { get; init; }
    public string? AudioCodec { get; init; }
    public long? FileSize { get; init; }
    public long? FileSizeApprox { get; init; }
    public double? TotalBitrate { get; init; }
    public double? AudioBitrate { get; init; }
    public string? Note { get; init; }
    public string? Format { get; init; }
    public string? Protocol { get; init; }
    public int? AudioChannels { get; init; }

    /// <summary>True when the stream carries a picture.</summary>
    public bool HasVideo => !string.IsNullOrEmpty(VideoCodec) && VideoCodec != "none";

    /// <summary>True when the stream carries sound.</summary>
    public bool HasAudio => !string.IsNullOrEmpty(AudioCodec) && AudioCodec != "none";

    /// <summary>Compact description used in the format list.</summary>
    public string Summary
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(FormatId))
            {
                parts.Add(FormatId!);
            }

            if (!string.IsNullOrWhiteSpace(Extension))
            {
                parts.Add(Extension!);
            }

            if (HasVideo && !string.IsNullOrWhiteSpace(Resolution))
            {
                parts.Add(Fps is > 0 ? $"{Resolution}p{Fps:0}" : Resolution!);
            }

            if (HasVideo && !string.IsNullOrWhiteSpace(VideoCodec))
            {
                parts.Add(VideoCodec!);
            }

            if (HasAudio && !string.IsNullOrWhiteSpace(AudioCodec))
            {
                var bitrate = Formatting.Bitrate(AudioBitrate);
                parts.Add(bitrate.Length == 0 ? AudioCodec! : $"{AudioCodec} {bitrate}");
            }

            var size = FileSize ?? FileSizeApprox;
            if (size is > 0)
            {
                parts.Add(Formatting.Bytes(size));
            }

            if (!string.IsNullOrWhiteSpace(Note))
            {
                parts.Add(Note!);
            }

            return string.Join(" · ", parts);
        }
    }
}

/// <summary>A single entry of a playlist, used for the analysis preview.</summary>
public sealed class MediaEntry
{
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Url { get; init; }
    public double? Duration { get; init; }
    public string? Uploader { get; init; }
    public int Index { get; init; }

    /// <summary>Position plus title, for list display.</summary>
    public string Display => $"{Index}. {Title ?? "(bez názvu)"}";
}

/// <summary>Everything the GUI shows after analysing a link.</summary>
public sealed class MediaInfo
{
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? FullTitle { get; init; }
    public string? Uploader { get; init; }
    public string? Channel { get; init; }
    public string? Extractor { get; init; }
    public string? WebPageUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public string? Description { get; init; }
    public string? UploadDate { get; init; }
    public string? License { get; init; }
    public double? Duration { get; init; }
    public long? ViewCount { get; init; }
    public long? LikeCount { get; init; }
    public long? CommentCount { get; init; }
    public double? AverageRating { get; init; }
    public bool? IsLive { get; init; }
    public string? LiveStatus { get; init; }
    public bool IsPlaylist { get; init; }
    public int? PlaylistCount { get; init; }
    public IReadOnlyList<MediaFormat> Formats { get; init; } = Array.Empty<MediaFormat>();
    public IReadOnlyList<MediaEntry> Entries { get; init; } = Array.Empty<MediaEntry>();
    public IReadOnlyList<string> SubtitleLanguages { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> AutomaticSubtitleLanguages { get; init; } = Array.Empty<string>();

    /// <summary>Total duration of every analysed entry.</summary>
    public double? TotalDuration { get; init; }

    /// <summary>Headline shown above the analyser result.</summary>
    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? "(bez názvu)" : Title!;
}

/// <summary>Formatting helpers shared by the GUI and the core assembly.</summary>
public static class Formatting
{
    /// <summary>Format a byte count using binary units.</summary>
    public static string Bytes(long? value)
    {
        if (value is null or <= 0)
        {
            return "–";
        }

        string[] units = ["B", "KiB", "MiB", "GiB", "TiB"];
        double size = value.Value;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return unit == 0 ? $"{size:0} B" : $"{size:0.0} {units[unit]}";
    }

    /// <summary>Format a bitrate in kbit/s.</summary>
    public static string Bitrate(double? kbps)
    {
        if (kbps is null or <= 0)
        {
            return string.Empty;
        }

        return kbps >= 1000 ? $"{kbps / 1000:0.0} Mbit/s" : $"{kbps:0} kbit/s";
    }

    /// <summary>Format a duration as <c>h:mm:ss</c> or <c>m:ss</c>.</summary>
    public static string Duration(double? seconds)
    {
        if (seconds is null or <= 0 || double.IsNaN(seconds.Value))
        {
            return "–";
        }

        var span = TimeSpan.FromSeconds(seconds.Value);
        return span.TotalHours >= 1
            ? $"{(int)span.TotalHours}:{span.Minutes:00}:{span.Seconds:00}"
            : $"{span.Minutes}:{span.Seconds:00}";
    }

    /// <summary>Format a rate in bytes per second.</summary>
    public static string Speed(double? bytesPerSecond)
    {
        if (bytesPerSecond is null or <= 0)
        {
            return "–";
        }

        return Bytes((long)bytesPerSecond.Value) + "/s";
    }

    /// <summary>Format yt-dlp's <c>YYYYMMDD</c> upload date for display.</summary>
    public static string UploadDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw!.Length != 8)
        {
            return raw ?? string.Empty;
        }

        return $"{raw[..4]}-{raw[4..6]}-{raw[6..]}";
    }

    /// <summary>Compact human count such as <c>1,2 mil.</c>.</summary>
    public static string Count(long? value)
    {
        if (value is null or < 0)
        {
            return "–";
        }

        return value.Value switch
        {
            >= 1_000_000_000 => $"{value.Value / 1_000_000_000d:0.#} mld.",
            >= 1_000_000 => $"{value.Value / 1_000_000d:0.#} mil.",
            >= 1_000 => $"{value.Value / 1_000d:0.#} tis.",
            _ => value.Value.ToString("N0"),
        };
    }
}
