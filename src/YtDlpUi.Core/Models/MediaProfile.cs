namespace YtDlpUi.Core.Models;

/// <summary>
/// Technical description of one local media file, as reported by ffprobe.
/// Only leaf values are stored so the object can safely cross into the UI layer.
/// </summary>
public sealed class MediaProfile
{
    /// <summary>Absolute path of the analysed file.</summary>
    public required string FilePath { get; init; }

    /// <summary>File name without the directory part.</summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>Size on disk in bytes, or 0 when unknown.</summary>
    public long SizeBytes { get; init; }

    /// <summary>Duration in seconds, or null when the container does not expose one.</summary>
    public double? DurationSeconds { get; init; }

    /// <summary>Container short name such as <c>matroska,webm</c> or <c>mp4</c>.</summary>
    public string? ContainerFormat { get; init; }

    /// <summary>True when at least one video stream exists.</summary>
    public bool HasVideo { get; init; }

    /// <summary>True when at least one audio stream exists.</summary>
    public bool HasAudio { get; init; }

    /// <summary>Video codec name, for example <c>h264</c> or <c>vp9</c>.</summary>
    public string? VideoCodec { get; init; }

    /// <summary>Video width in pixels.</summary>
    public int? Width { get; init; }

    /// <summary>Video height in pixels.</summary>
    public int? Height { get; init; }

    /// <summary>Average frame rate.</summary>
    public double? FrameRate { get; init; }

    /// <summary>Overall (or video) bitrate in kbit/s.</summary>
    public double? VideoBitrateKbps { get; init; }

    /// <summary>Audio codec name, for example <c>aac</c> or <c>opus</c>.</summary>
    public string? AudioCodec { get; init; }

    /// <summary>Number of audio channels.</summary>
    public int? AudioChannels { get; init; }

    /// <summary>Audio sample rate in Hz.</summary>
    public int? SampleRate { get; init; }

    /// <summary>Audio bitrate in kbit/s.</summary>
    public double? AudioBitrateKbps { get; init; }

    /// <summary>Number of embedded subtitle streams.</summary>
    public int SubtitleStreamCount { get; init; }

    /// <summary>True when the file is usable as a conversion or playback source.</summary>
    public bool IsUsable => HasVideo || HasAudio;

    /// <summary>Resolution written as <c>1920×1080</c>, or a dash when there is no picture.</summary>
    public string ResolutionText => HasVideo && Width is > 0 && Height is > 0 ? $"{Width}×{Height}" : "–";

    /// <summary>One-line technical summary used in the player and converter.</summary>
    public string Summary
    {
        get
        {
            var parts = new List<string>();
            if (HasVideo)
            {
                parts.Add(string.IsNullOrWhiteSpace(VideoCodec) ? "video" : VideoCodec!);
                if (Width is > 0 && Height is > 0)
                {
                    parts.Add($"{Width}×{Height}");
                }

                if (FrameRate is > 0)
                {
                    parts.Add($"{FrameRate:0.##} fps");
                }
            }

            if (HasAudio)
            {
                parts.Add(string.IsNullOrWhiteSpace(AudioCodec) ? "audio" : AudioCodec!);
                if (AudioChannels is > 0)
                {
                    parts.Add(AudioChannels == 1 ? "mono" : AudioChannels == 2 ? "stereo" : $"{AudioChannels} ch");
                }
            }

            parts.Add(Formatting.Duration(DurationSeconds));
            parts.Add(Formatting.Bytes(SizeBytes));

            return string.Join(" · ", parts);
        }
    }

    /// <summary>Name shown in pickers; falls back to the file name.</summary>
    public string DisplayName => FileName;
}
