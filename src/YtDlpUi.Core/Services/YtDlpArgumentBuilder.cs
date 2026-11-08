using System.Globalization;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

/// <summary>
/// Translates a <see cref="DownloadOptions"/> instance into a yt-dlp argument vector.
/// This is the single place that knows about command-line flags, so the GUI can expose
/// every option the terminal tool supports without duplicating knowledge in the view layer.
/// </summary>
public static class YtDlpArgumentBuilder
{
    /// <summary>Marker prefixed to machine-readable download progress lines.</summary>
    public const string ProgressMarker = "@@YTDLP_PROGRESS@@";

    /// <summary>Marker prefixed to machine-readable post-processing lines.</summary>
    public const string PostProcessMarker = "@@YTDLP_PP@@";

    /// <summary>Unit separator used between progress template fields.</summary>
    public const char FieldSeparator = '\u001f';

    /// <summary>yt-dlp defaults, used to decide whether a flag is worth emitting.</summary>
    private const int DefaultRetries = 10;

    private const int DefaultFragmentRetries = 10;
    private const int DefaultFileAccessRetries = 3;
    private const int DefaultExtractorRetries = 3;
    private const int DefaultConcurrentFragments = 1;

    /// <summary>
    /// Build the argument vector for a download run.
    /// </summary>
    /// <param name="options">User configuration.</param>
    /// <param name="urls">Target URLs, appended last.</param>
    /// <param name="machineReadableProgress">
    /// When true, adds a progress template that the GUI parses instead of a text progress bar.
    /// </param>
    public static List<string> Build(
        DownloadOptions options,
        IEnumerable<string> urls,
        bool machineReadableProgress = true)
    {
        var args = new List<string>();

        AddGeneral(options, args);
        AddNetwork(options, args);
        AddSelection(options, args);
        AddFormat(options, args);
        AddPostProcessing(options, args);
        AddFilesystem(options, args);
        AddSubtitles(options, args);
        AddMetadata(options, args);
        AddAuthentication(options, args);

        AddFlag(args, options.Verbose, "--verbose");
        AddFlag(args, options.NoWarnings, "--no-warnings");
        AddFlag(args, options.WritePages, "--write-pages");
        AddFlag(args, options.PrintTraffic, "--print-traffic");

        // Deterministic, parseable output.
        AddOption(args, "--color", "never");

        if (machineReadableProgress)
        {
            AddFlag(args, true, "--newline");
            AddOption(args, "--progress-delta", "1");
            AddOption(args, "--progress-template", BuildDownloadProgressTemplate());
            AddOption(args, "--progress-template", BuildPostProcessTemplate());
        }

        foreach (var extra in CommandLine.Split(options.ExtraArguments))
        {
            args.Add(extra);
        }

        foreach (var url in urls)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                args.Add(url);
            }
        }

        return args;
    }

    /// <summary>
    /// Build the arguments used to analyse a link (<c>--dump-single-json</c>).
    /// Only network/authentication options matter here, but they matter a lot for
    /// restricted videos, so they are included verbatim.
    /// </summary>
    public static List<string> BuildAnalysisArguments(DownloadOptions options, string url, bool flatPlaylist)
    {
        var args = new List<string>();

        AddFlag(args, options.IgnoreConfig, "--ignore-config");
        AddOption(args, "--config-locations", options.ConfigLocations);
        AddOption(args, "--extractor-args", options.ExtractorArgs);

        AddNetwork(options, args);
        AddAuthentication(options, args);

        AddOption(args, "--color", "never");
        AddFlag(args, true, "--no-warnings");

        if (flatPlaylist)
        {
            AddFlag(args, true, "--flat-playlist");
        }

        AddOption(args, "--socket-timeout", options.SocketTimeout > 0
            ? options.SocketTimeout.ToString(CultureInfo.InvariantCulture)
            : null);

        // -J already implies simulation; adding --no-simulate here would make yt-dlp
        // fetch the media as well, which is exactly what "Analyzovat" must not do.
        args.Add("--dump-single-json");

        foreach (var extra in CommandLine.Split(options.ExtraArguments))
        {
            if (extra is "-J" or "--dump-single-json" or "-j" or "--dump-json")
            {
                continue;
            }

            args.Add(extra);
        }

        args.Add(url);
        return args;
    }

    /// <summary>The <c>--progress-template</c> value that yields parseable download progress.</summary>
    public static string BuildDownloadProgressTemplate() =>
        "download:" + ProgressMarker +
        "%(progress.status)s" + FieldSeparator +
        "%(progress.downloaded_bytes)s" + FieldSeparator +
        "%(progress.total_bytes)s" + FieldSeparator +
        "%(progress.total_bytes_estimate)s" + FieldSeparator +
        "%(progress.speed)s" + FieldSeparator +
        "%(progress.eta)s" + FieldSeparator +
        "%(progress.fragment_index)s" + FieldSeparator +
        "%(progress.fragment_count)s" + FieldSeparator +
        "%(info.title)s";

    /// <summary>The <c>--progress-template</c> value that yields parseable post-processing progress.</summary>
    public static string BuildPostProcessTemplate() =>
        "postprocess:" + PostProcessMarker +
        "%(progress.status)s" + FieldSeparator +
        "%(progress.postprocessor)s" + FieldSeparator +
        "%(info.title)s";

    // ───────────────────────────── groups ─────────────────────────────

    private static void AddGeneral(DownloadOptions o, List<string> args)
    {
        AddFlag(args, o.IgnoreConfig, "--ignore-config");
        AddOption(args, "--config-locations", o.ConfigLocations);
        AddOption(args, "--plugin-dirs", o.PluginDirs);
        AddOption(args, "--extractor-args", o.ExtractorArgs);
    }

    private static void AddNetwork(DownloadOptions o, List<string> args)
    {
        AddOption(args, "--proxy", o.Proxy);
        AddOption(args, "--impersonate", o.Impersonate);
        AddOption(args, "--source-address", o.SourceAddress);

        if (o.SocketTimeout > 0)
        {
            AddOption(args, "--socket-timeout", o.SocketTimeout.ToString(CultureInfo.InvariantCulture));
        }

        AddFlag(args, o.ForceIpv4, "--force-ipv4");
        AddFlag(args, o.ForceIpv6, "--force-ipv6");

        AddOption(args, "--limit-rate", o.LimitRate);
        AddOption(args, "--throttled-rate", o.ThrottledRate);

        if (o.Retries != DefaultRetries)
        {
            AddOption(args, "--retries", o.Retries.ToString(CultureInfo.InvariantCulture));
        }

        if (o.FragmentRetries != DefaultFragmentRetries)
        {
            AddOption(args, "--fragment-retries", o.FragmentRetries.ToString(CultureInfo.InvariantCulture));
        }

        if (o.FileAccessRetries != DefaultFileAccessRetries)
        {
            AddOption(args, "--file-access-retries", o.FileAccessRetries.ToString(CultureInfo.InvariantCulture));
        }

        if (o.ExtractorRetries != DefaultExtractorRetries)
        {
            AddOption(args, "--extractor-retries", o.ExtractorRetries.ToString(CultureInfo.InvariantCulture));
        }

        if (o.ConcurrentFragments != DefaultConcurrentFragments)
        {
            AddOption(args, "--concurrent-fragments", o.ConcurrentFragments.ToString(CultureInfo.InvariantCulture));
        }

        AddOption(args, "--buffer-size", o.BufferSize);
        AddOption(args, "--http-chunk-size", o.HttpChunkSize);

        AddSeconds(args, "--sleep-requests", o.SleepRequests);
        AddSeconds(args, "--sleep-interval", o.SleepInterval);
        AddSeconds(args, "--max-sleep-interval", o.MaxSleepInterval);
        AddSeconds(args, "--sleep-subtitles", o.SleepSubtitles);

        AddFlag(args, o.NoCheckCertificates, "--no-check-certificates");
        AddFlag(args, o.LegacyServerConnect, "--legacy-server-connect");

        AddLines(args, o.AddHeaders, "--add-headers");
        AddOption(args, "--geo-verification-proxy", o.GeoVerificationProxy);
        AddOption(args, "--xff", o.XForwardedFor);
        AddOption(args, "--js-runtimes", o.JsRuntimes);
        AddOption(args, "--remote-components", o.RemoteComponents);
        AddOption(args, "--downloader", o.ExternalDownloader);
        AddOption(args, "--downloader-args", o.ExternalDownloaderArgs);
    }

    private static void AddSelection(DownloadOptions o, List<string> args)
    {
        switch (o.PlaylistHandling)
        {
            case PlaylistHandling.VideoOnly:
                AddFlag(args, true, "--no-playlist");
                break;
            case PlaylistHandling.PlaylistOnly:
                AddFlag(args, true, "--yes-playlist");
                break;
        }

        AddOption(args, "--playlist-items", o.PlaylistItems);
        AddFlag(args, o.PlaylistReverse, "--playlist-reverse");
        AddFlag(args, o.PlaylistRandom, "--playlist-random");
        AddFlag(args, o.LazyPlaylist, "--lazy-playlist");

        if (o.MaxDownloads > 0)
        {
            AddOption(args, "--max-downloads", o.MaxDownloads.ToString(CultureInfo.InvariantCulture));
        }

        AddOption(args, "--download-archive", o.DownloadArchive);
        AddFlag(args, o.BreakOnExisting, "--break-on-existing");
        AddFlag(args, o.BreakPerInput, "--break-per-input");

        if (o.SkipPlaylistAfterErrors > 0)
        {
            AddOption(args, "--skip-playlist-after-errors", o.SkipPlaylistAfterErrors.ToString(CultureInfo.InvariantCulture));
        }

        if (o.AbortOnError)
        {
            AddFlag(args, true, "--abort-on-error");
        }
        else if (o.IgnoreErrors)
        {
            AddFlag(args, true, "--ignore-errors");
        }

        AddOption(args, "--min-filesize", o.MinFilesize);
        AddOption(args, "--max-filesize", o.MaxFilesize);
        AddOption(args, "--date", o.Date);
        AddOption(args, "--datebefore", o.DateBefore);
        AddOption(args, "--dateafter", o.DateAfter);

        if (o.AgeLimit > 0)
        {
            AddOption(args, "--age-limit", o.AgeLimit.ToString(CultureInfo.InvariantCulture));
        }

        AddLines(args, o.MatchFilters, "--match-filters");
        AddLines(args, o.BreakMatchFilters, "--break-match-filters");

        AddFlag(args, o.LiveFromStart, "--live-from-start");
        AddOption(args, "--wait-for-video", o.WaitForVideo);
        AddFlag(args, o.FlatPlaylist, "--flat-playlist");
        AddFlag(args, o.MarkWatched, "--mark-watched");
    }

    private static void AddFormat(DownloadOptions o, List<string> args)
    {
        var format = o.Format;
        if (o.ExtractAudio && string.IsNullOrWhiteSpace(format))
        {
            format = "ba/b";
        }

        AddOption(args, "--format", format);
        AddOption(args, "--format-sort", o.FormatSort);
        AddFlag(args, o.FormatSortForce, "--format-sort-force");
        AddFlag(args, o.VideoMultistreams, "--video-multistreams");
        AddFlag(args, o.AudioMultistreams, "--audio-multistreams");
        AddFlag(args, o.PreferFreeFormats, "--prefer-free-formats");
        AddFlag(args, o.CheckFormats, "--check-formats");
        AddOption(args, "--merge-output-format", MergeContainerName(o.MergeContainer));
    }

    private static void AddPostProcessing(DownloadOptions o, List<string> args)
    {
        AddFlag(args, o.ExtractAudio, "--extract-audio");

        if (o.ExtractAudio)
        {
            AddOption(args, "--audio-format", string.IsNullOrWhiteSpace(o.AudioFormat) ? "best" : o.AudioFormat);
            AddOption(args, "--audio-quality", o.AudioQuality);
        }

        AddOption(args, "--remux-video", o.RemuxVideo);
        AddOption(args, "--recode-video", o.RecodeVideo);
        AddFlag(args, o.KeepVideo, "--keep-video");
        AddOption(args, "--postprocessor-args", o.PostprocessorArgs);
        AddOption(args, "--ffmpeg-location", o.FfmpegLocation);
        AddOption(args, "--download-sections", o.DownloadSections);
        AddFlag(args, o.ForceKeyframesAtCuts, "--force-keyframes-at-cuts");
        AddOption(args, "--exec", o.ExecCommand);
    }

    private static void AddFilesystem(DownloadOptions o, List<string> args)
    {
        var directory = o.OutputDirectory?.Trim();
        if (!string.IsNullOrWhiteSpace(directory))
        {
            AddOption(args, "--paths", "home:" + directory);
        }

        AddOption(args, "--output", string.IsNullOrWhiteSpace(o.OutputTemplate) ? null : o.OutputTemplate);

        if (!string.IsNullOrWhiteSpace(o.TempDirectory))
        {
            AddOption(args, "--paths", "temp:" + o.TempDirectory!.Trim());
        }

        AddFlag(args, o.RestrictFilenames, "--restrict-filenames");
        AddFlag(args, o.WindowsFilenames, "--windows-filenames");

        if (o.TrimFilenames > 0)
        {
            AddOption(args, "--trim-filenames", o.TrimFilenames.ToString(CultureInfo.InvariantCulture));
        }

        switch (o.Overwrite)
        {
            case OverwritePolicy.NoOverwrites:
                AddFlag(args, true, "--no-overwrites");
                break;
            case OverwritePolicy.ForceOverwrites:
                AddFlag(args, true, "--force-overwrites");
                break;
        }

        AddFlag(args, o.NoContinue, "--no-continue");
        AddFlag(args, o.NoPart, "--no-part");
        AddFlag(args, o.UseMtime, "--mtime");
        AddOption(args, "--cache-dir", o.CacheDirectory);
        AddFlag(args, o.RemoveCache, "--rm-cache-dir");
        AddOption(args, "--batch-file", o.BatchFile);
    }

    private static void AddSubtitles(DownloadOptions o, List<string> args)
    {
        AddFlag(args, o.WriteSubs, "--write-subs");
        AddFlag(args, o.WriteAutoSubs, "--write-auto-subs");

        if (o.WriteSubs || o.WriteAutoSubs)
        {
            AddOption(args, "--sub-langs", o.SubLangs);
            AddOption(args, "--sub-format", o.SubFormat);
        }

        AddFlag(args, o.EmbedSubs, "--embed-subs");
        AddOption(args, "--convert-subs", o.ConvertSubs);
    }

    private static void AddMetadata(DownloadOptions o, List<string> args)
    {
        AddFlag(args, o.EmbedMetadata, "--embed-metadata");
        AddFlag(args, o.WriteDescription, "--write-description");
        AddFlag(args, o.WriteInfoJson, "--write-info-json");
        AddFlag(args, o.WritePlaylistMetafiles, "--write-playlist-metafiles");
        AddFlag(args, o.WriteComments, "--write-comments");
        AddFlag(args, o.EmbedChapters, "--embed-chapters");
        AddFlag(args, o.SplitChapters, "--split-chapters");
        AddOption(args, "--remove-chapters", o.RemoveChapters);
        AddOption(args, "--parse-metadata", o.ParseMetadata);
        AddFlag(args, o.Xattrs, "--xattrs");

        switch (o.LinkFile)
        {
            case LinkFileFormat.Desktop:
                AddFlag(args, true, "--write-desktop-link");
                break;
            case LinkFileFormat.Url:
                AddFlag(args, true, "--write-url-link");
                break;
            case LinkFileFormat.Webloc:
                AddFlag(args, true, "--write-webloc-link");
                break;
        }

        AddFlag(args, o.WriteThumbnail, "--write-thumbnail");
        AddFlag(args, o.WriteAllThumbnails, "--write-all-thumbnails");
        AddFlag(args, o.EmbedThumbnail, "--embed-thumbnail");
        AddOption(args, "--convert-thumbnails", o.ConvertThumbnails);

        AddOption(args, "--sponsorblock-mark", o.SponsorBlockMark);
        AddOption(args, "--sponsorblock-remove", o.SponsorBlockRemove);
        AddOption(args, "--sponsorblock-api", o.SponsorBlockApi);
    }

    private static void AddAuthentication(DownloadOptions o, List<string> args)
    {
        AddOption(args, "--cookies", o.CookiesFile);
        AddOption(args, "--cookies-from-browser", o.CookiesFromBrowser);
        AddOption(args, "--username", o.Username);
        AddOption(args, "--password", o.Password);
        AddOption(args, "--twofactor", o.TwoFactor);
        AddOption(args, "--video-password", o.VideoPassword);
        AddFlag(args, o.Netrc, "--netrc");
        AddOption(args, "--netrc-location", o.NetrcLocation);
        AddOption(args, "--client-certificate", o.ClientCertificate);
        AddOption(args, "--client-certificate-key", o.ClientCertificateKey);
        AddOption(args, "--client-certificate-password", o.ClientCertificatePassword);
    }

    // ───────────────────────────── helpers ─────────────────────────────

    private static void AddFlag(List<string> args, bool condition, string flag)
    {
        if (condition)
        {
            args.Add(flag);
        }
    }

    private static void AddOption(List<string> args, string flag, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        args.Add(flag);
        args.Add(value!);
    }

    private static void AddSeconds(List<string> args, string flag, double seconds)
    {
        if (seconds > 0)
        {
            args.Add(flag);
            args.Add(seconds.ToString("0.###", CultureInfo.InvariantCulture));
        }
    }

    private static void AddLines(List<string> args, string? multiline, string flag)
    {
        foreach (var line in SplitLines(multiline))
        {
            args.Add(flag);
            args.Add(line);
        }
    }

    /// <summary>Split a multi-line option into trimmed, non-empty entries.</summary>
    public static IEnumerable<string> SplitLines(string? multiline)
    {
        if (string.IsNullOrWhiteSpace(multiline))
        {
            yield break;
        }

        foreach (var raw in multiline!.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length > 0)
            {
                yield return line;
            }
        }
    }

    /// <summary>Map the container enum to the value yt-dlp expects.</summary>
    public static string? MergeContainerName(MergeContainer container) => container switch
    {
        MergeContainer.Mp4 => "mp4",
        MergeContainer.Mkv => "mkv",
        MergeContainer.Webm => "webm",
        MergeContainer.Mov => "mov",
        MergeContainer.Avi => "avi",
        MergeContainer.Flv => "flv",
        _ => null,
    };
}
