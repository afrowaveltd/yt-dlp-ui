namespace YtDlpUi.Core.Models;

/// <summary>Container or audio format a file can be converted into.</summary>
public enum ConversionTarget
{
    /// <summary>MP4 video (H.264 + AAC), the most widely playable choice.</summary>
    Mp4 = 0,

    /// <summary>Matroska video, keeps nearly any codec.</summary>
    Mkv = 1,

    /// <summary>WebM video (VP9 + Opus).</summary>
    Webm = 2,

    /// <summary>QuickTime video (H.264 + AAC).</summary>
    Mov = 3,

    /// <summary>Legacy AVI video.</summary>
    Avi = 4,

    /// <summary>Animated GIF.</summary>
    Gif = 5,

    /// <summary>MP3 audio.</summary>
    Mp3 = 6,

    /// <summary>M4A audio (AAC in an MP4 container).</summary>
    M4a = 7,

    /// <summary>Opus audio.</summary>
    Opus = 8,

    /// <summary>FLAC lossless audio.</summary>
    Flac = 9,

    /// <summary>Uncompressed WAV audio.</summary>
    Wav = 10,
}

/// <summary>Quality tier used to pick codecs, CRF and audio bitrate.</summary>
public enum ConversionQuality
{
    /// <summary>Large file, best visual and audio fidelity.</summary>
    High = 0,

    /// <summary>Default balance of size and quality.</summary>
    Balanced = 1,

    /// <summary>Small file, visibly lower quality.</summary>
    Small = 2,
}

/// <summary>Helpers for <see cref="ConversionTarget"/> values.</summary>
public static class ConversionTargets
{
    /// <summary>True when the target holds no picture.</summary>
    public static bool IsAudio(this ConversionTarget target) =>
        target is ConversionTarget.Mp3 or ConversionTarget.M4a or ConversionTarget.Opus
            or ConversionTarget.Flac or ConversionTarget.Wav;

    /// <summary>File extension, including the leading dot.</summary>
    public static string Extension(this ConversionTarget target) => target switch
    {
        ConversionTarget.Mp4 => ".mp4",
        ConversionTarget.Mkv => ".mkv",
        ConversionTarget.Webm => ".webm",
        ConversionTarget.Mov => ".mov",
        ConversionTarget.Avi => ".avi",
        ConversionTarget.Gif => ".gif",
        ConversionTarget.Mp3 => ".mp3",
        ConversionTarget.M4a => ".m4a",
        ConversionTarget.Opus => ".opus",
        ConversionTarget.Flac => ".flac",
        ConversionTarget.Wav => ".wav",
        _ => ".mp4",
    };

    /// <summary>English label used as a localization key.</summary>
    public static string Label(this ConversionTarget target) => target switch
    {
        ConversionTarget.Mp4 => "MP4 video (H.264 + AAC)",
        ConversionTarget.Mkv => "MKV video (keeps any codec)",
        ConversionTarget.Webm => "WebM video (VP9 + Opus)",
        ConversionTarget.Mov => "MOV video (QuickTime)",
        ConversionTarget.Avi => "AVI video (legacy)",
        ConversionTarget.Gif => "GIF animation",
        ConversionTarget.Mp3 => "MP3 audio",
        ConversionTarget.M4a => "M4A audio (AAC)",
        ConversionTarget.Opus => "Opus audio",
        ConversionTarget.Flac => "FLAC audio (lossless)",
        ConversionTarget.Wav => "WAV audio (uncompressed)",
        _ => "MP4 video (H.264 + AAC)",
    };

    /// <summary>Short label shown on conversion buttons.</summary>
    public static string ShortLabel(this ConversionTarget target) => target switch
    {
        ConversionTarget.Mp4 => "MP4",
        ConversionTarget.Mkv => "MKV",
        ConversionTarget.Webm => "WebM",
        ConversionTarget.Mov => "MOV",
        ConversionTarget.Avi => "AVI",
        ConversionTarget.Gif => "GIF",
        ConversionTarget.Mp3 => "MP3",
        ConversionTarget.M4a => "M4A",
        ConversionTarget.Opus => "Opus",
        ConversionTarget.Flac => "FLAC",
        ConversionTarget.Wav => "WAV",
        _ => "MP4",
    };

    /// <summary>The default target for a source profile: keep video for video, audio for audio.</summary>
    public static ConversionTarget Suggest(this MediaProfile profile) =>
        profile.HasVideo ? ConversionTarget.Mp4 : ConversionTarget.Mp3;
}

/// <summary>Every setting the FFmpeg conversion page can apply.</summary>
public sealed partial class ConversionOptions : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    public ConversionOptions()
    {
        OutputDirectory = string.Empty;
    }

    /// <summary>File being converted.</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial string? SourcePath { get; set; }

    /// <summary>Where the converted file is written; empty means "next to the source".</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial string OutputDirectory { get; set; }

    /// <summary>Requested output format.</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial ConversionTarget Target { get; set; }

    /// <summary>Quality tier; ignored for lossless targets. Defaults to the balanced tier.</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial ConversionQuality Quality { get; set; } = ConversionQuality.Balanced;

    /// <summary>Downscale so the picture is no taller than this; 0 keeps the original size.</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial int MaxHeight { get; set; }

    /// <summary>Drop the audio track from a video target.</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial bool RemoveAudio { get; set; }

    /// <summary>Optional start of the converted range, for example <c>00:01:30</c>.</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial string? StartTime { get; set; }

    /// <summary>Optional end of the converted range, for example <c>00:02:00</c>.</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial string? EndTime { get; set; }

    /// <summary>Overwrite an existing file instead of failing.</summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    public partial bool OverwriteExisting { get; set; } = true;

    /// <summary>Independent copy used for one queue item so later edits cannot change it.</summary>
    public ConversionOptions Snapshot() => new()
    {
        SourcePath = SourcePath,
        OutputDirectory = OutputDirectory,
        Target = Target,
        Quality = Quality,
        MaxHeight = MaxHeight,
        RemoveAudio = RemoveAudio,
        StartTime = StartTime,
        EndTime = EndTime,
        OverwriteExisting = OverwriteExisting,
    };

    /// <summary>Notify every binding after a batch change.</summary>
    public void NotifyAll() => OnPropertyChanged(string.Empty);
}
