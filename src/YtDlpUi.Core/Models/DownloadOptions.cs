using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YtDlpUi.Core.Models;

/// <summary>
/// Every configurable yt-dlp option exposed by the GUI. Each property maps directly onto one
/// command-line flag; <see cref="Services.YtDlpArgumentBuilder"/> turns a populated instance into argv.
/// Null / empty values mean "let yt-dlp use its own default", so an untouched instance produces
/// exactly the same result as plain <c>yt-dlp URL</c>.
/// </summary>
public sealed partial class DownloadOptions : ObservableObject
{
    public const string DefaultOutputTemplate = "%(title)s [%(id)s].%(ext)s";

    public DownloadOptions()
    {
        OutputTemplate = DefaultOutputTemplate;
        Format = string.Empty;
        SubLangs = "en";
        AudioQuality = "5";
        Retries = 10;
        FragmentRetries = 10;
        FileAccessRetries = 3;
        ConcurrentFragments = 1;
        SocketTimeout = 0;
        SleepInterval = 0;
        MaxSleepInterval = 0;
        SleepRequests = 0;
        SleepSubtitles = 0;
        SkipPlaylistAfterErrors = 0;
        ExtractorRetries = 3;
        TrimFilenames = 0;
        MaxDownloads = 0;
        AgeLimit = 0;
        LimitRate = string.Empty;
        Proxy = string.Empty;
        Impersonate = string.Empty;
    }

    // ───────────────────────────── General ─────────────────────────────

    /// <summary>Selected friendly preset. Mirrors the other format properties when applied.</summary>
    [ObservableProperty] public partial DownloadPreset Preset { get; set; }

    /// <summary>Ignore user/system configuration files (<c>--ignore-config</c>).</summary>
    [ObservableProperty] public partial bool IgnoreConfig { get; set; }

    /// <summary>Explicit configuration file or directory (<c>--config-locations</c>).</summary>
    [ObservableProperty] public partial string? ConfigLocations { get; set; }

    /// <summary>Extra plugin search directories (<c>--plugin-dirs</c>).</summary>
    [ObservableProperty] public partial string? PluginDirs { get; set; }

    /// <summary>Per-extractor arguments, e.g. <c>youtube:player_client=android</c> (<c>--extractor-args</c>).</summary>
    [ObservableProperty] public partial string? ExtractorArgs { get; set; }

    /// <summary>Verbose debug output (<c>--verbose</c>).</summary>
    [ObservableProperty] public partial bool Verbose { get; set; }

    /// <summary>Suppress warnings (<c>--no-warnings</c>).</summary>
    [ObservableProperty] public partial bool NoWarnings { get; set; }

    /// <summary>Write downloaded pages to files for debugging (<c>--write-pages</c>).</summary>
    [ObservableProperty] public partial bool WritePages { get; set; }

    /// <summary>Print HTTP traffic (<c>--print-traffic</c>).</summary>
    [ObservableProperty] public partial bool PrintTraffic { get; set; }

    /// <summary>Free-form extra arguments appended verbatim to the command line.</summary>
    [ObservableProperty] public partial string ExtraArguments { get; set; } = string.Empty;

    // ───────────────────────────── Format selection ─────────────────────────────

    /// <summary>Format selector (<c>-f</c>), e.g. <c>bv*+ba/b</c>.</summary>
    [ObservableProperty] public partial string Format { get; set; }

    /// <summary>Format sort order (<c>-S</c>), e.g. <c>res:1080,ext:mp4:m4a</c>.</summary>
    [ObservableProperty] public partial string? FormatSort { get; set; }

    /// <summary>Let the user sort order win over built-in precedence (<c>--format-sort-force</c>).</summary>
    [ObservableProperty] public partial bool FormatSortForce { get; set; }

    /// <summary>Allow several video streams to be merged (<c>--video-multistreams</c>).</summary>
    [ObservableProperty] public partial bool VideoMultistreams { get; set; }

    /// <summary>Allow several audio streams to be merged (<c>--audio-multistreams</c>).</summary>
    [ObservableProperty] public partial bool AudioMultistreams { get; set; }

    /// <summary>Prefer freely-usable containers (<c>--prefer-free-formats</c>).</summary>
    [ObservableProperty] public partial bool PreferFreeFormats { get; set; }

    /// <summary>Verify that selected formats are really downloadable (<c>--check-formats</c>).</summary>
    [ObservableProperty] public partial bool CheckFormats { get; set; }

    /// <summary>Container to use when merging (<c>--merge-output-format</c>).</summary>
    [ObservableProperty] public partial MergeContainer MergeContainer { get; set; }

    // ───────────────────────────── Audio extraction ─────────────────────────────

    /// <summary>Extract audio only (<c>--extract-audio</c> / <c>-x</c>).</summary>
    [ObservableProperty] public partial bool ExtractAudio { get; set; }

    /// <summary>Target audio format for <c>-x</c> (<c>--audio-format</c>).</summary>
    [ObservableProperty] public partial string AudioFormat { get; set; } = "best";

    /// <summary>Audio quality 0-10 or a bitrate like <c>192K</c> (<c>--audio-quality</c>).</summary>
    [ObservableProperty] public partial string AudioQuality { get; set; }

    /// <summary>Remux into another container without re-encoding (<c>--remux-video</c>).</summary>
    [ObservableProperty] public partial string? RemuxVideo { get; set; }

    /// <summary>Re-encode into another container (<c>--recode-video</c>).</summary>
    [ObservableProperty] public partial string? RecodeVideo { get; set; }

    /// <summary>Keep intermediate files after post-processing (<c>--keep-video</c>).</summary>
    [ObservableProperty] public partial bool KeepVideo { get; set; }

    /// <summary>Extra ffmpeg/postprocessor arguments (<c>--postprocessor-args</c>).</summary>
    [ObservableProperty] public partial string? PostprocessorArgs { get; set; }

    /// <summary>Explicit ffmpeg binary or directory (<c>--ffmpeg-location</c>).</summary>
    [ObservableProperty] public partial string? FfmpegLocation { get; set; }

    /// <summary>Download only these chapters/time ranges (<c>--download-sections</c>).</summary>
    [ObservableProperty] public partial string? DownloadSections { get; set; }

    /// <summary>Re-encode around cuts for frame accuracy (<c>--force-keyframes-at-cuts</c>).</summary>
    [ObservableProperty] public partial bool ForceKeyframesAtCuts { get; set; }

    // ───────────────────────────── Output / filesystem ─────────────────────────────

    /// <summary>Destination folder. Mapped to <c>--paths home:…</c>.</summary>
    [ObservableProperty] public partial string OutputDirectory { get; set; } = string.Empty;

    /// <summary>Output filename template (<c>--output</c>).</summary>
    [ObservableProperty] public partial string OutputTemplate { get; set; }

    /// <summary>Temporary folder for partial files (<c>--paths temp:…</c>).</summary>
    [ObservableProperty] public partial string? TempDirectory { get; set; }

    /// <summary>ASCII-only filenames (<c>--restrict-filenames</c>).</summary>
    [ObservableProperty] public partial bool RestrictFilenames { get; set; }

    /// <summary>Force Windows-compatible filenames (<c>--windows-filenames</c>).</summary>
    [ObservableProperty] public partial bool WindowsFilenames { get; set; }

    /// <summary>Truncate filenames above this length (<c>--trim-filenames</c>). 0 = off.</summary>
    [ObservableProperty] public partial int TrimFilenames { get; set; }

    /// <summary>Existing-file behaviour.</summary>
    [ObservableProperty] public partial OverwritePolicy Overwrite { get; set; }

    /// <summary>Do not resume partial downloads (<c>--no-continue</c>).</summary>
    [ObservableProperty] public partial bool NoContinue { get; set; }

    /// <summary>Write directly to the target file instead of <c>.part</c> (<c>--no-part</c>).</summary>
    [ObservableProperty] public partial bool NoPart { get; set; }

    /// <summary>Set file modification time from the Last-modified header (<c>--mtime</c>).</summary>
    [ObservableProperty] public partial bool UseMtime { get; set; }

    /// <summary>Cache directory override (<c>--cache-dir</c>).</summary>
    [ObservableProperty] public partial string? CacheDirectory { get; set; }

    /// <summary>Remove yt-dlp's cache before downloading (<c>--rm-cache-dir</c>).</summary>
    [ObservableProperty] public partial bool RemoveCache { get; set; }

    /// <summary>Read URLs from a batch file (<c>--batch-file</c>).</summary>
    [ObservableProperty] public partial string? BatchFile { get; set; }

    // ───────────────────────────── Playlist / selection ─────────────────────────────

    /// <summary>Video vs playlist handling.</summary>
    [ObservableProperty] public partial PlaylistHandling PlaylistHandling { get; set; }

    /// <summary>Playlist item selector (<c>--playlist-items</c>), e.g. <c>1:5,10</c>.</summary>
    [ObservableProperty] public partial string? PlaylistItems { get; set; }

    /// <summary>Download playlist entries in reverse order (<c>--playlist-reverse</c>).</summary>
    [ObservableProperty] public partial bool PlaylistReverse { get; set; }

    /// <summary>Download playlist entries in random order (<c>--playlist-random</c>).</summary>
    [ObservableProperty] public partial bool PlaylistRandom { get; set; }

    /// <summary>Process entries as they arrive (<c>--lazy-playlist</c>).</summary>
    [ObservableProperty] public partial bool LazyPlaylist { get; set; }

    /// <summary>Stop after N files (<c>--max-downloads</c>). 0 = unlimited.</summary>
    [ObservableProperty] public partial int MaxDownloads { get; set; }

    /// <summary>Skip already-downloaded IDs recorded in this file (<c>--download-archive</c>).</summary>
    [ObservableProperty] public partial string? DownloadArchive { get; set; }

    /// <summary>Stop when an archived entry is hit (<c>--break-on-existing</c>).</summary>
    [ObservableProperty] public partial bool BreakOnExisting { get; set; }

    /// <summary>Reset the N-limit counters for every input URL (<c>--break-per-input</c>).</summary>
    [ObservableProperty] public partial bool BreakPerInput { get; set; }

    /// <summary>Skip the rest of a playlist after N failures (<c>--skip-playlist-after-errors</c>). 0 = off.</summary>
    [ObservableProperty] public partial int SkipPlaylistAfterErrors { get; set; }

    /// <summary>Continue with the next item after a failure (<c>--ignore-errors</c>).</summary>
    [ObservableProperty] public partial bool IgnoreErrors { get; set; }

    /// <summary>Abort the whole run on the first error (<c>--abort-on-error</c>).</summary>
    [ObservableProperty] public partial bool AbortOnError { get; set; }

    /// <summary>Minimum file size filter (<c>--min-filesize</c>), e.g. <c>50k</c>.</summary>
    [ObservableProperty] public partial string? MinFilesize { get; set; }

    /// <summary>Maximum file size filter (<c>--max-filesize</c>), e.g. <c>4.2G</c>.</summary>
    [ObservableProperty] public partial string? MaxFilesize { get; set; }

    /// <summary>Only videos uploaded on this date (<c>--date</c>).</summary>
    [ObservableProperty] public partial string? Date { get; set; }

    /// <summary>Only videos uploaded before this date (<c>--datebefore</c>).</summary>
    [ObservableProperty] public partial string? DateBefore { get; set; }

    /// <summary>Only videos uploaded after this date (<c>--dateafter</c>).</summary>
    [ObservableProperty] public partial string? DateAfter { get; set; }

    /// <summary>Age limit filter (<c>--age-limit</c>). 0 = off.</summary>
    [ObservableProperty] public partial int AgeLimit { get; set; }

    /// <summary>Generic field filters (<c>--match-filters</c>), one per line.</summary>
    [ObservableProperty] public partial string? MatchFilters { get; set; }

    /// <summary>Stop the run when a filter rejects an item (<c>--break-match-filters</c>), one per line.</summary>
    [ObservableProperty] public partial string? BreakMatchFilters { get; set; }

    /// <summary>Download livestreams from the beginning (<c>--live-from-start</c>).</summary>
    [ObservableProperty] public partial bool LiveFromStart { get; set; }

    /// <summary>Wait for scheduled streams (<c>--wait-for-video</c>), e.g. <c>60</c> or <c>30-120</c>.</summary>
    [ObservableProperty] public partial string? WaitForVideo { get; set; }

    /// <summary>Do not resolve playlist entries (<c>--flat-playlist</c>).</summary>
    [ObservableProperty] public partial bool FlatPlaylist { get; set; }

    /// <summary>Mark videos as watched on the site (<c>--mark-watched</c>).</summary>
    [ObservableProperty] public partial bool MarkWatched { get; set; }

    // ───────────────────────────── Subtitles ─────────────────────────────

    /// <summary>Write subtitle files (<c>--write-subs</c>).</summary>
    [ObservableProperty] public partial bool WriteSubs { get; set; }

    /// <summary>Write auto-generated subtitles (<c>--write-auto-subs</c>).</summary>
    [ObservableProperty] public partial bool WriteAutoSubs { get; set; }

    /// <summary>Subtitle languages (<c>--sub-langs</c>), e.g. <c>cs,en.*</c>.</summary>
    [ObservableProperty] public partial string SubLangs { get; set; }

    /// <summary>Preferred subtitle format (<c>--sub-format</c>).</summary>
    [ObservableProperty] public partial string SubFormat { get; set; } = "best";

    /// <summary>Embed subtitles into the video (<c>--embed-subs</c>).</summary>
    [ObservableProperty] public partial bool EmbedSubs { get; set; }

    /// <summary>Convert subtitles to another format (<c>--convert-subs</c>).</summary>
    [ObservableProperty] public partial string? ConvertSubs { get; set; }

    // ───────────────────────────── Thumbnail ─────────────────────────────

    /// <summary>Write the thumbnail image (<c>--write-thumbnail</c>).</summary>
    [ObservableProperty] public partial bool WriteThumbnail { get; set; }

    /// <summary>Write every thumbnail size (<c>--write-all-thumbnails</c>).</summary>
    [ObservableProperty] public partial bool WriteAllThumbnails { get; set; }

    /// <summary>Embed the thumbnail as cover art (<c>--embed-thumbnail</c>).</summary>
    [ObservableProperty] public partial bool EmbedThumbnail { get; set; }

    /// <summary>Convert thumbnails to another image format (<c>--convert-thumbnails</c>).</summary>
    [ObservableProperty] public partial string? ConvertThumbnails { get; set; }

    // ───────────────────────────── Metadata ─────────────────────────────

    /// <summary>Embed metadata into the media file (<c>--embed-metadata</c>).</summary>
    [ObservableProperty] public partial bool EmbedMetadata { get; set; }

    /// <summary>Write a <c>.description</c> file (<c>--write-description</c>).</summary>
    [ObservableProperty] public partial bool WriteDescription { get; set; }

    /// <summary>Write a <c>.info.json</c> file (<c>--write-info-json</c>).</summary>
    [ObservableProperty] public partial bool WriteInfoJson { get; set; }

    /// <summary>Also write playlist-level metadata files (<c>--write-playlist-metafiles</c>).</summary>
    [ObservableProperty] public partial bool WritePlaylistMetafiles { get; set; }

    /// <summary>Fetch and store comments in the infojson (<c>--write-comments</c>).</summary>
    [ObservableProperty] public partial bool WriteComments { get; set; }

    /// <summary>Embed chapter markers (<c>--embed-chapters</c>).</summary>
    [ObservableProperty] public partial bool EmbedChapters { get; set; }

    /// <summary>Split the video into one file per chapter (<c>--split-chapters</c>).</summary>
    [ObservableProperty] public partial bool SplitChapters { get; set; }

    /// <summary>Remove chapters whose title matches this regex (<c>--remove-chapters</c>).</summary>
    [ObservableProperty] public partial string? RemoveChapters { get; set; }

    /// <summary>Rewrite metadata fields (<c>--parse-metadata</c>).</summary>
    [ObservableProperty] public partial string? ParseMetadata { get; set; }

    /// <summary>Write metadata to file xattrs (<c>--xattrs</c>).</summary>
    [ObservableProperty] public partial bool Xattrs { get; set; }

    /// <summary>Write an internet shortcut next to the media.</summary>
    [ObservableProperty] public partial LinkFileFormat LinkFile { get; set; }

    /// <summary>Run a command after downloading (<c>--exec</c>).</summary>
    [ObservableProperty] public partial string? ExecCommand { get; set; }

    // ───────────────────────────── SponsorBlock ─────────────────────────────

    /// <summary>Categories to create chapters for (<c>--sponsorblock-mark</c>).</summary>
    [ObservableProperty] public partial string? SponsorBlockMark { get; set; }

    /// <summary>Categories to cut out of the video (<c>--sponsorblock-remove</c>).</summary>
    [ObservableProperty] public partial string? SponsorBlockRemove { get; set; }

    /// <summary>Custom SponsorBlock API endpoint (<c>--sponsorblock-api</c>).</summary>
    [ObservableProperty] public partial string? SponsorBlockApi { get; set; }

    // ───────────────────────────── Network ─────────────────────────────

    /// <summary>HTTP/HTTPS/SOCKS proxy (<c>--proxy</c>).</summary>
    [ObservableProperty] public partial string Proxy { get; set; }

    /// <summary>Socket timeout in seconds (<c>--socket-timeout</c>). 0 = default.</summary>
    [ObservableProperty] public partial int SocketTimeout { get; set; }

    /// <summary>Client-side IP address to bind to (<c>--source-address</c>).</summary>
    [ObservableProperty] public partial string? SourceAddress { get; set; }

    /// <summary>Client to impersonate (<c>--impersonate</c>), e.g. <c>chrome</c>.</summary>
    [ObservableProperty] public partial string Impersonate { get; set; }

    /// <summary>Force IPv4 (<c>--force-ipv4</c>).</summary>
    [ObservableProperty] public partial bool ForceIpv4 { get; set; }

    /// <summary>Force IPv6 (<c>--force-ipv6</c>).</summary>
    [ObservableProperty] public partial bool ForceIpv6 { get; set; }

    /// <summary>Download rate limit (<c>--limit-rate</c>), e.g. <c>2M</c>.</summary>
    [ObservableProperty] public partial string LimitRate { get; set; }

    /// <summary>Throttled-rate detection threshold (<c>--throttled-rate</c>).</summary>
    [ObservableProperty] public partial string? ThrottledRate { get; set; }

    /// <summary>General retry count (<c>--retries</c>).</summary>
    [ObservableProperty] public partial int Retries { get; set; }

    /// <summary>Per-fragment retry count (<c>--fragment-retries</c>).</summary>
    [ObservableProperty] public partial int FragmentRetries { get; set; }

    /// <summary>File-access retry count (<c>--file-access-retries</c>).</summary>
    [ObservableProperty] public partial int FileAccessRetries { get; set; }

    /// <summary>Extractor retry count (<c>--extractor-retries</c>).</summary>
    [ObservableProperty] public partial int ExtractorRetries { get; set; }

    /// <summary>Parallel fragment downloads (<c>--concurrent-fragments</c>).</summary>
    [ObservableProperty] public partial int ConcurrentFragments { get; set; }

    /// <summary>Download buffer size (<c>--buffer-size</c>).</summary>
    [ObservableProperty] public partial string? BufferSize { get; set; }

    /// <summary>HTTP chunk size (<c>--http-chunk-size</c>).</summary>
    [ObservableProperty] public partial string? HttpChunkSize { get; set; }

    /// <summary>Seconds to sleep between extraction requests (<c>--sleep-requests</c>).</summary>
    [ObservableProperty] public partial double SleepRequests { get; set; }

    /// <summary>Minimum seconds to sleep before each download (<c>--sleep-interval</c>).</summary>
    [ObservableProperty] public partial double SleepInterval { get; set; }

    /// <summary>Maximum random sleep before each download (<c>--max-sleep-interval</c>).</summary>
    [ObservableProperty] public partial double MaxSleepInterval { get; set; }

    /// <summary>Seconds to sleep before each subtitle download (<c>--sleep-subtitles</c>).</summary>
    [ObservableProperty] public partial double SleepSubtitles { get; set; }

    /// <summary>Certificate validation bypass (<c>--no-check-certificates</c>).</summary>
    [ObservableProperty] public partial bool NoCheckCertificates { get; set; }

    /// <summary>Allow legacy TLS renegotiation (<c>--legacy-server-connect</c>).</summary>
    [ObservableProperty] public partial bool LegacyServerConnect { get; set; }

    /// <summary>Extra HTTP headers, one <c>Field:Value</c> per line (<c>--add-headers</c>).</summary>
    [ObservableProperty] public partial string? AddHeaders { get; set; }

    /// <summary>Geo-bypass verification proxy (<c>--geo-verification-proxy</c>).</summary>
    [ObservableProperty] public partial string? GeoVerificationProxy { get; set; }

    /// <summary>Fake <c>X-Forwarded-For</c> value (<c>--xff</c>).</summary>
    [ObservableProperty] public partial string? XForwardedFor { get; set; }

    /// <summary>Extra JavaScript runtime, e.g. <c>node</c> (<c>--js-runtimes</c>). Empty = yt-dlp default.</summary>
    [ObservableProperty] public partial string? JsRuntimes { get; set; }

    /// <summary>Allow remote components, e.g. <c>ejs:github</c> (<c>--remote-components</c>).</summary>
    [ObservableProperty] public partial string? RemoteComponents { get; set; }

    /// <summary>External downloader, e.g. <c>aria2c</c> (<c>--downloader</c>).</summary>
    [ObservableProperty] public partial string? ExternalDownloader { get; set; }

    /// <summary>Arguments for the external downloader (<c>--downloader-args</c>).</summary>
    [ObservableProperty] public partial string? ExternalDownloaderArgs { get; set; }

    // ───────────────────────────── Authentication ─────────────────────────────

    /// <summary>Cookies file in Netscape format (<c>--cookies</c>).</summary>
    [ObservableProperty] public partial string? CookiesFile { get; set; }

    /// <summary>Load cookies straight from a browser (<c>--cookies-from-browser</c>).</summary>
    [ObservableProperty] public partial string? CookiesFromBrowser { get; set; }

    /// <summary>Site username (<c>--username</c>).</summary>
    [ObservableProperty] public partial string? Username { get; set; }

    /// <summary>Site password (<c>--password</c>).</summary>
    [ObservableProperty] public partial string? Password { get; set; }

    /// <summary>Two-factor code (<c>--twofactor</c>).</summary>
    [ObservableProperty] public partial string? TwoFactor { get; set; }

    /// <summary>Video-specific password (<c>--video-password</c>).</summary>
    [ObservableProperty] public partial string? VideoPassword { get; set; }

    /// <summary>Use <c>~/.netrc</c> credentials (<c>--netrc</c>).</summary>
    [ObservableProperty] public partial bool Netrc { get; set; }

    /// <summary>Alternative .netrc location (<c>--netrc-location</c>).</summary>
    [ObservableProperty] public partial string? NetrcLocation { get; set; }

    /// <summary>Client certificate file (<c>--client-certificate</c>).</summary>
    [ObservableProperty] public partial string? ClientCertificate { get; set; }

    /// <summary>Client certificate private key (<c>--client-certificate-key</c>).</summary>
    [ObservableProperty] public partial string? ClientCertificateKey { get; set; }

    /// <summary>Client certificate key password (<c>--client-certificate-password</c>).</summary>
    [ObservableProperty] public partial string? ClientCertificatePassword { get; set; }

    // ───────────────────────────── Derived helpers ─────────────────────────────

    /// <summary>True when the preset intends an audio-only download.</summary>
    [JsonIgnore]
    public bool IsAudioOnlyPreset => Preset is DownloadPreset.AudioMp3 or DownloadPreset.AudioM4a
        or DownloadPreset.AudioOpus or DownloadPreset.AudioFlac or DownloadPreset.AudioWav
        or DownloadPreset.AudioBest;

    /// <summary>Human readable label for the currently selected preset.</summary>
    [JsonIgnore]
    public string PresetLabel => PresetLabelFor(Preset);

    /// <summary>Human readable label for any preset value.</summary>
    public static string PresetLabelFor(DownloadPreset preset) => preset switch
    {
        DownloadPreset.BestVideo => "Nejlepší kvalita (video + zvuk)",
        DownloadPreset.Video2160 => "Video do 2160p (4K)",
        DownloadPreset.Video1440 => "Video do 1440p (2K)",
        DownloadPreset.Video1080 => "Video do 1080p (Full HD)",
        DownloadPreset.Video720 => "Video do 720p (HD)",
        DownloadPreset.Video480 => "Video do 480p",
        DownloadPreset.Video360 => "Video do 360p (malé soubory)",
        DownloadPreset.VideoSmallest => "Nejmenší možné video",
        DownloadPreset.AudioMp3 => "Jen zvuk – MP3",
        DownloadPreset.AudioM4a => "Jen zvuk – M4A (AAC)",
        DownloadPreset.AudioOpus => "Jen zvuk – Opus",
        DownloadPreset.AudioFlac => "Jen zvuk – FLAC (bezztrátově)",
        DownloadPreset.AudioWav => "Jen zvuk – WAV",
        DownloadPreset.AudioBest => "Jen zvuk – původní stopa",
        DownloadPreset.Custom => "Vlastní nastavení",
        _ => "Nejlepší kvalita (video + zvuk)",
    };

    /// <summary>Raised when several properties change at once so the UI can refresh once.</summary>
    public void NotifyAll()
    {
        OnPropertyChanged(string.Empty);
    }
}
