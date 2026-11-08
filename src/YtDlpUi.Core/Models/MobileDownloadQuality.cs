namespace YtDlpUi.Core.Models;

/// <summary>
/// Android preview format choices. Choose a single playable stream: this version
/// intentionally does not bundle FFmpeg or promise audio/video stream merging.
/// </summary>
public enum MobileDownloadQuality
{
    /// <summary>Best combined audio/video stream available.</summary>
    Best,
    /// <summary>Up to 720p combined audio/video where available.</summary>
    UpTo720p,
    /// <summary>Up to 480p combined audio/video where available.</summary>
    UpTo480p,
    /// <summary>Best existing audio stream, without transcoding.</summary>
    AudioOriginal
}

/// <summary>Safe format selectors understood by the embedded yt-dlp adapter.</summary>
public static class MobileDownloadFormats
{
    /// <summary>Maps a fixed UI preset to a yt-dlp -f expression.</summary>
    public static string For(MobileDownloadQuality choice) => choice switch
    {
        MobileDownloadQuality.UpTo720p => "b[height<=720]/b",
        MobileDownloadQuality.UpTo480p => "b[height<=480]/b",
        MobileDownloadQuality.AudioOriginal => "ba",
        _ => "b"
    };
}
