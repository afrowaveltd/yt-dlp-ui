using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

/// <summary>
/// Applies a friendly <see cref="DownloadPreset"/> by configuring the technical options behind it.
/// Every preset only touches format/post-processing fields, so playlist, network and output
/// settings chosen elsewhere survive a preset change.
/// </summary>
public static class DownloadPresetFactory
{
    /// <summary>Configure <paramref name="options"/> for the given preset.</summary>
    public static void Apply(DownloadOptions options, DownloadPreset preset)
    {
        ResetFormatOptions(options);
        options.Preset = preset;

        switch (preset)
        {
            case DownloadPreset.BestVideo:
                ApplyVideo(options, null);
                break;

            case DownloadPreset.Video2160:
                ApplyVideo(options, 2160);
                break;

            case DownloadPreset.Video1440:
                ApplyVideo(options, 1440);
                break;

            case DownloadPreset.Video1080:
                ApplyVideo(options, 1080);
                break;

            case DownloadPreset.Video720:
                ApplyVideo(options, 720);
                break;

            case DownloadPreset.Video480:
                ApplyVideo(options, 480);
                break;

            case DownloadPreset.Video360:
                ApplyVideo(options, 360);
                break;

            case DownloadPreset.VideoSmallest:
                options.Format = "wv*+ba/w";
                options.FormatSort = "size";
                options.MergeContainer = MergeContainer.Mp4;
                options.ExtractAudio = false;
                break;

            case DownloadPreset.AudioMp3:
                ApplyAudio(options, "mp3");
                break;

            case DownloadPreset.AudioM4a:
                ApplyAudio(options, "m4a");
                break;

            case DownloadPreset.AudioOpus:
                ApplyAudio(options, "opus");
                break;

            case DownloadPreset.AudioFlac:
                ApplyAudio(options, "flac");
                break;

            case DownloadPreset.AudioWav:
                ApplyAudio(options, "wav");
                break;

            case DownloadPreset.AudioBest:
                options.ExtractAudio = true;
                options.Format = "ba/b";
                options.AudioFormat = "best";
                options.AudioQuality = "0";
                options.EmbedMetadata = true;
                break;

            case DownloadPreset.Custom:
            default:
                break;
        }

        options.NotifyAll();
    }

    private static void ApplyVideo(DownloadOptions options, int? maxHeight)
    {
        options.ExtractAudio = false;
        options.Format = maxHeight is null
            ? "bv*+ba/b"
            : $"bv*[height<={maxHeight}]+ba/b[height<={maxHeight}]";
        options.FormatSort = "ext:mp4:m4a";
        options.MergeContainer = MergeContainer.Mp4;
    }

    private static void ApplyAudio(DownloadOptions options, string audioFormat)
    {
        options.ExtractAudio = true;
        options.Format = "ba/b";
        options.AudioFormat = audioFormat;
        options.AudioQuality = "0";
        options.EmbedMetadata = true;
        options.EmbedThumbnail = true;
    }

    /// <summary>Clear every field a preset may set, so presets never stack up.</summary>
    private static void ResetFormatOptions(DownloadOptions options)
    {
        options.ExtractAudio = false;
        options.Format = string.Empty;
        options.FormatSort = null;
        options.FormatSortForce = false;
        options.MergeContainer = MergeContainer.Default;
        options.AudioFormat = "best";
        options.AudioQuality = "5";
        options.EmbedMetadata = false;
        options.EmbedThumbnail = false;
        options.RemuxVideo = null;
        options.RecodeVideo = null;
        options.VideoMultistreams = false;
        options.AudioMultistreams = false;
    }

    /// <summary>One selectable entry for a preset picker.</summary>
    public sealed record PresetChoice(DownloadPreset Preset, string Title, string Description, string Glyph)
    {
        /// <summary>True for audio-only presets, used for the badge in the picker.</summary>
        public bool IsAudio => Preset is >= DownloadPreset.AudioMp3 and <= DownloadPreset.AudioBest;

        /// <summary>Short category label shown on the card.</summary>
        public string CategoryLabel => IsAudio ? "Zvuk" : "Video";

        public override string ToString() => Title;
    }


    /// <summary>Presets shown on the quick-download page, in display order.</summary>
    public static IReadOnlyList<PresetChoice> VideoPresets { get; } =
    [
        new(DownloadPreset.BestVideo, "Nejlepší kvalita", "Nejvyšší dostupné rozlišení se zvukem (MP4)", "⭐"),
        new(DownloadPreset.Video2160, "4K (2160p)", "Maximálně 2160p – velké soubory", "🎬"),
        new(DownloadPreset.Video1440, "2K (1440p)", "Maximálně 1440p", "🎬"),
        new(DownloadPreset.Video1080, "Full HD (1080p)", "Nejčastější volba pro televize a telefony", "📺"),
        new(DownloadPreset.Video720, "HD (720p)", "Menší soubory, dobrá kvalita", "📺"),
        new(DownloadPreset.Video480, "480p", "Úsporné – vhodné pro mobily", "📱"),
        new(DownloadPreset.Video360, "360p", "Nejmenší běžná kvalita", "📱"),
        new(DownloadPreset.VideoSmallest, "Nejmenší soubor", "Co nejmenší velikost bez ohledu na kvalitu", "🪶"),
    ];

    /// <summary>Audio-only presets shown on the quick-download page.</summary>
    public static IReadOnlyList<PresetChoice> AudioPresets { get; } =
    [
        new(DownloadPreset.AudioMp3, "MP3", "Přehratelné všude – ideální do auta", "🎵"),
        new(DownloadPreset.AudioM4a, "M4A (AAC)", "Kvalitnější než MP3 při stejné velikosti", "🎵"),
        new(DownloadPreset.AudioOpus, "Opus", "Nejlepší poměr kvality a velikosti", "🎧"),
        new(DownloadPreset.AudioFlac, "FLAC", "Bezztrátový zvuk pro archivaci", "💎"),
        new(DownloadPreset.AudioWav, "WAV", "Nekomprimovaný zvuk pro další zpracování", "💎"),
        new(DownloadPreset.AudioBest, "Původní stopa", "Bez převodu – nejrychlejší", "⚡"),
    ];

    /// <summary>Every preset, video first then audio, in one list for the picker.</summary>
    public static IReadOnlyList<PresetChoice> AllPresets { get; } =
        VideoPresets.Concat(AudioPresets).ToList();
}
