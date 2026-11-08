namespace YtDlpUi.Core.Models;

/// <summary>How yt-dlp should treat a URL that is both a video and a playlist.</summary>
public enum PlaylistHandling
{
    /// <summary>Let yt-dlp decide (its built-in default).</summary>
    Default = 0,

    /// <summary>Download only the single video (<c>--no-playlist</c>).</summary>
    VideoOnly = 1,

    /// <summary>Download the whole playlist (<c>--yes-playlist</c>).</summary>
    PlaylistOnly = 2,
}

/// <summary>Existing-file handling for downloaded media.</summary>
public enum OverwritePolicy
{
    /// <summary>yt-dlp default: keep the video, refresh sidecar files.</summary>
    Default = 0,

    /// <summary>Never overwrite anything (<c>--no-overwrites</c>).</summary>
    NoOverwrites = 1,

    /// <summary>Overwrite everything (<c>--force-overwrites</c>).</summary>
    ForceOverwrites = 2,
}

/// <summary>Friendly download presets offered on the "Quick download" page.</summary>
public enum DownloadPreset
{
    /// <summary>Highest quality video + audio, merged container.</summary>
    BestVideo = 0,

    /// <summary>Video limited to 2160p (4K).</summary>
    Video2160 = 1,

    /// <summary>Video limited to 1440p (2K).</summary>
    Video1440 = 2,

    /// <summary>Video limited to 1080p (Full HD).</summary>
    Video1080 = 3,

    /// <summary>Video limited to 720p (HD).</summary>
    Video720 = 4,

    /// <summary>Video limited to 480p.</summary>
    Video480 = 5,

    /// <summary>Video limited to 360p (small files).</summary>
    Video360 = 6,

    /// <summary>Smallest possible video file.</summary>
    VideoSmallest = 7,

    /// <summary>Audio only, converted to MP3.</summary>
    AudioMp3 = 8,

    /// <summary>Audio only, converted to M4A/AAC.</summary>
    AudioM4a = 9,

    /// <summary>Audio only, converted to Opus.</summary>
    AudioOpus = 10,

    /// <summary>Audio only, converted to FLAC (lossless).</summary>
    AudioFlac = 11,

    /// <summary>Audio only, converted to WAV (uncompressed).</summary>
    AudioWav = 12,

    /// <summary>Audio only, keeping the original best stream.</summary>
    AudioBest = 13,

    /// <summary>Use whatever is configured on the manual format page.</summary>
    Custom = 99,
}

/// <summary>Where yt-dlp writes sidecar metadata.</summary>
public enum LinkFileFormat
{
    None = 0,
    Desktop = 1,
    Url = 2,
    Webloc = 3,
}

/// <summary>yt-dlp container choice when merging separate video/audio streams.</summary>
public enum MergeContainer
{
    Default = 0,
    Mp4 = 1,
    Mkv = 2,
    Webm = 3,
    Mov = 4,
    Avi = 5,
    Flv = 6,
}
