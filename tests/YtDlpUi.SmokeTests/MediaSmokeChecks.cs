using System.Text.Json;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

/// <summary>
/// Offline regression checks for the 0.2.0 player and FFmpeg converter additions.
/// Nothing here runs FFmpeg or touches the network, so the suite stays fast and deterministic
/// on both Linux and Windows CI agents.
/// </summary>
internal static class MediaSmokeChecks
{
    public static void Verify()
    {
        VerifyArgumentBuilder();
        VerifyOutputPath();
        VerifyTimeParsing();
        VerifyProgressParser();
        VerifyProbeParsing();
        VerifyMediaLibrary();
        VerifyTargetMapping();
        VerifyLocalizationParity();
        Console.WriteLine("0.2.0 media, player and converter smoke checks passed.");
    }

    private static void VerifyArgumentBuilder()
    {
        var mp4 = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mkv"),
            Target = ConversionTarget.Mp4,
            Quality = ConversionQuality.Balanced,
        });
        Require(IndexOfValue(mp4, "-c:v") == "libx264", "MP4 must encode video with libx264");
        Require(IndexOfValue(mp4, "-c:a") == "aac", "MP4 must encode audio with AAC");
        Require(mp4.Contains("+faststart"), "MP4 should move the index to the front");
        Require(IndexOfValue(mp4, "-pix_fmt") == "yuv420p", "MP4 needs a widely supported pixel format");
        // Path.Combine uses the platform separator, so the expectation must be built the
        // same way instead of hard-coding a POSIX path.
        Require(mp4[^1] == Path.Combine("media", "clip.mp4"),
            "output path must be the final argument, got " + mp4[^1]);
        Require(mp4.Contains("-progress") && mp4.Contains("pipe:1"), "progress must be machine readable");
        Require(mp4.Contains("-nostdin"), "FFmpeg must never wait for console input");

        var webm = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mp4"),
            Target = ConversionTarget.Webm,
        });
        Require(IndexOfValue(webm, "-c:v") == "libvpx-vp9", "WebM must use VP9");
        Require(IndexOfValue(webm, "-c:a") == "libopus", "WebM must use Opus audio");
        Require(!webm.Contains("+faststart"), "WebM has no MP4 faststart flag");

        var mp3 = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mp4"),
            Target = ConversionTarget.Mp3,
        });
        Require(mp3.Contains("-vn"), "audio targets must drop the picture");
        Require(IndexOfValue(mp3, "-c:a") == "libmp3lame", "MP3 must use libmp3lame");
        Require(IndexOfValue(mp3, "-b:a") == "192k", "balanced MP3 bitrate should be 192k");
        Require(!mp3.Contains("-c:v"), "audio targets must not configure a video codec");

        var muted = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mp4"),
            Target = ConversionTarget.Mkv,
            RemoveAudio = true,
        });
        Require(muted.Contains("-an"), "RemoveAudio must pass -an");
        Require(!muted.Contains("-c:a"), "RemoveAudio must not configure an audio codec");

        var scaled = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mp4"),
            Target = ConversionTarget.Mp4,
            MaxHeight = 720,
        });
        var scale = IndexOfValue(scaled, "-vf");
        Require(scale is not null && scale.Contains("scale=-2:min(ih\\,720)"),
            "downscaling must never upscale, and the filter comma must be escaped: " + scale);

        var gif = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mp4"),
            Target = ConversionTarget.Gif,
        });
        Require(IndexOfValue(gif, "-vf")?.Contains("palettegen") == true, "GIF needs a generated palette");
        Require(gif.Contains("-an"), "GIF must not carry audio");
        Require(IndexOfValue(gif, "-loop") == "0", "GIF should loop forever");

        var trimmed = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mp4"),
            Target = ConversionTarget.Mp4,
            StartTime = "10",
            EndTime = "25",
        });
        var inputIndex = trimmed.IndexOf("-i");
        var seekIndex = trimmed.IndexOf("-ss");
        Require(seekIndex >= 0 && seekIndex < inputIndex, "seeking before the input keeps it fast");
        Require(IndexOfValue(trimmed, "-t") == "15", "start+end must become a 15s duration");
        Require(!trimmed.Contains("-to"), "start+end uses -t, not -to");

        var endOnly = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mp4"),
            Target = ConversionTarget.Mp4,
            EndTime = "00:00:30",
        });
        Require(IndexOfValue(endOnly, "-to") == "30", "end-only trimming must use -to");

        var notOverwriting = FfmpegArgumentBuilder.Build(new ConversionOptions
        {
            SourcePath = Media("clip.mp4"),
            Target = ConversionTarget.Mp4,
            OverwriteExisting = false,
        });
        Require(notOverwriting.Contains("-n") && !notOverwriting.Contains("-y"),
            "disabling overwrite must pass -n instead of -y");
    }

    private static void VerifyOutputPath()
    {
        var same = new ConversionOptions
        {
            SourcePath = Path.Combine("media", "clip.mp4"),
            Target = ConversionTarget.Mp4,
        };
        Require(FfmpegArgumentBuilder.BuildOutputPath(same).EndsWith("clip (converted).mp4", StringComparison.Ordinal),
            "MP4 to MP4 must not overwrite the source");

        var different = new ConversionOptions
        {
            SourcePath = Path.Combine("media", "clip.mkv"),
            Target = ConversionTarget.Mp4,
        };
        Require(FfmpegArgumentBuilder.BuildOutputPath(different).EndsWith("clip.mp4", StringComparison.Ordinal),
            "a different container keeps the plain name");

        // Windows and Linux separate paths differently; the result must follow the platform.
        var separated = new ConversionOptions
        {
            SourcePath = "media" + Path.DirectorySeparatorChar + "clip.mkv",
            OutputDirectory = "out" + Path.DirectorySeparatorChar + "audio",
            Target = ConversionTarget.Mp3,
        };
        var separatedOutput = FfmpegArgumentBuilder.BuildOutputPath(separated);
        Require(Path.GetFileName(separatedOutput) == "clip.mp3",
            "output file name must survive any directory separator, got " + separatedOutput);
        Require(separatedOutput == Path.Combine("out", "audio", "clip.mp3"),
            "output directory must use the platform separator, got " + separatedOutput);

        var directed = new ConversionOptions
        {
            SourcePath = Path.Combine("media", "clip.mkv"),
            Target = ConversionTarget.Mp3,
            OutputDirectory = Path.Combine("out", "audio"),
        };
        Require(FfmpegArgumentBuilder.BuildOutputPath(directed) ==
                Path.Combine("out", "audio", "clip.mp3"),
            "the configured output folder must be honoured");
    }

    private static void VerifyTimeParsing()
    {
        Require(FfmpegArgumentBuilder.TryParseTime("90", out var seconds) && seconds == 90, "plain seconds");
        Require(FfmpegArgumentBuilder.TryParseTime("1:30", out seconds) && seconds == 90, "mm:ss");
        Require(FfmpegArgumentBuilder.TryParseTime("00:01:30", out seconds) && seconds == 90, "hh:mm:ss");
        Require(FfmpegArgumentBuilder.TryParseTime("0:00:02.5", out seconds) && Math.Abs(seconds - 2.5) < 0.001,
            "fractional seconds");
        Require(!FfmpegArgumentBuilder.TryParseTime("abc", out _), "text is rejected");
        Require(!FfmpegArgumentBuilder.TryParseTime("-5", out _), "negative time is rejected");
        Require(!FfmpegArgumentBuilder.TryParseTime("", out _), "blank is rejected");
        Require(!FfmpegArgumentBuilder.TryParseTime(null, out _), "null is rejected");
    }

    private static void VerifyProgressParser()
    {
        var accumulator = new FfmpegProgressAccumulator();
        FfmpegProgress? snapshot = null;

        foreach (var line in new[]
                 {
                     "frame=73", "fps=25.0", "bitrate=1234.5kbits/s", "total_size=123456",
                     "out_time_us=1200000", "out_time=00:00:01.200000", "speed=2.05x", "progress=continue",
                 })
        {
            snapshot = accumulator.Feed(line) ?? snapshot;
        }

        Require(snapshot is not null, "a progress block must produce a snapshot");
        Require(Math.Abs(snapshot!.OutTimeSeconds!.Value - 1.2) < 0.001, "out_time is parsed");
        Require(snapshot.SpeedFactor == 2.05, "speed factor is parsed");
        Require(Math.Abs(snapshot.BitrateKbps!.Value - 1234.5) < 0.01, "bitrate is parsed");
        Require(snapshot.TotalSizeBytes == 123456, "total size is parsed");
        Require(snapshot.Frame == 73, "frame counter is parsed");
        Require(!snapshot.IsComplete, "continue is not the end");
        Require(Math.Abs(snapshot.Percent(3)!.Value - 40) < 0.5, "percent uses the known duration");

        var end = accumulator.Feed("progress=end");
        Require(end is { IsComplete: true }, "end block is flagged as complete");
        Require(end!.Percent(3) == 100, "a finished run reports 100 percent");

        Require(accumulator.Feed("garbage") is null, "unknown lines are ignored");
        Require(accumulator.Feed("out_time=") is null, "an empty value does not complete a block");
        Require(FfmpegProgressAccumulator.TryParseBitrate("512bits/s", out var bitsPerSecond) &&
                Math.Abs(bitsPerSecond - 0.512) < 0.001, "plain bits are converted to kbit/s");
    }

    private static void VerifyProbeParsing()
    {
        const string json = """
        {
          "streams": [
            { "codec_type": "video", "codec_name": "h264", "width": 1920, "height": 1080,
              "avg_frame_rate": "30000/1001", "bit_rate": "5000000",
              "disposition": { "attached_pic": 0 } },
            { "codec_type": "video", "codec_name": "mjpeg", "width": 500, "height": 500,
              "disposition": { "attached_pic": 1 } },
            { "codec_type": "audio", "codec_name": "aac", "channels": 2,
              "sample_rate": "48000", "bit_rate": "128000" },
            { "codec_type": "subtitle", "codec_name": "subrip" }
          ],
          "format": { "duration": "12.5", "format_name": "mov,mp4,m4a", "size": "1234567" }
        }
        """;

        var profile = MediaProbe.ParseFfprobeJson(json, Media("clip.mp4"));
        Require(profile.HasVideo && profile.HasAudio, "video and audio streams are detected");
        Require(profile.VideoCodec == "h264" && profile.AudioCodec == "aac", "codecs are read");
        Require(profile.Width == 1920 && profile.Height == 1080, "resolution is read");
        Require(Math.Abs(profile.FrameRate!.Value - 29.97) < 0.01, "fractional frame rate is converted");
        Require(profile.AudioChannels == 2 && profile.SampleRate == 48000, "audio details are read");
        Require(profile.SubtitleStreamCount == 1, "subtitle streams are counted");
        Require(profile.DurationSeconds == 12.5, "duration is read from the container");
        Require(profile.SizeBytes == 1234567, "size is read from the container");
        Require(profile.ResolutionText == "1920×1080", "resolution text is formatted");
        Require(profile.Summary.Contains("h264", StringComparison.Ordinal), "summary mentions the codec");

        // Cover art must not be mistaken for real video.
        const string audioOnly = """
        {
          "streams": [
            { "codec_type": "video", "codec_name": "mjpeg", "width": 600, "height": 600,
              "disposition": { "attached_pic": 1 } },
            { "codec_type": "audio", "codec_name": "flac" }
          ],
          "format": { "duration": "61", "format_name": "flac", "size": "9876543" }
        }
        """;
        var audioProfile = MediaProbe.ParseFfprobeJson(audioOnly, Media("song.flac"));
        Require(!audioProfile.HasVideo, "attached cover art is not treated as a video stream");
        Require(audioProfile.HasAudio, "the audio stream is still detected");
        Require(audioProfile.Suggest() == ConversionTarget.Mp3, "audio sources suggest an audio target");
        Require(profile.Suggest() == ConversionTarget.Mp4, "video sources suggest MP4");

        var empty = MediaProbe.ParseFfprobeJson("{}", Media("unknown.bin"));
        Require(!empty.IsUsable && empty.DurationSeconds is null, "an empty document is tolerated");

        Require(MediaProbe.ParseFrameRate("25/1") == 25, "plain frame rate");
        Require(MediaProbe.ParseFrameRate("0/0") is null, "a zero denominator is rejected");
        Require(MediaProbe.ParseFrameRate("nonsense") is null, "garbage frame rate is rejected");
        Require(MediaProbe.ParseFrameRate(null) is null, "missing frame rate is rejected");
    }

    private static void VerifyMediaLibrary()
    {
        Require(MediaLibrary.IsSupportedMedia("clip.MP4"), "extension matching is case insensitive");
        Require(MediaLibrary.IsSupportedMedia("song.flac"), "audio files are supported");
        Require(!MediaLibrary.IsSupportedMedia("notes.txt"), "text files are rejected");
        Require(!MediaLibrary.IsSupportedMedia(""), "empty path is rejected");
        Require(!MediaLibrary.IsSupportedMedia(null), "null path is rejected");
        Require(MediaLibrary.IsVideoFile("clip.mkv") && !MediaLibrary.IsVideoFile("song.mp3"),
            "video and audio files are distinguished");

        var folder = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-media-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var older = Path.Combine(folder, "older.mp4");
            var newer = Path.Combine(folder, "newer.webm");
            File.WriteAllText(older, "x");
            File.WriteAllText(newer, "x");
            File.WriteAllText(Path.Combine(folder, "notes.txt"), "x");
            File.SetLastWriteTime(older, DateTime.Now.AddHours(-2));

            var found = MediaLibrary.Scan(folder);
            Require(found.Count == 2, "only supported media is listed, got " + found.Count);
            Require(Path.GetFileName(found[0]) == "newer.webm", "newest file comes first");
            Require(MediaLibrary.Scan(Path.Combine(folder, "missing")).Count == 0,
                "a missing folder yields an empty list");
            Require(MediaLibrary.Scan(null).Count == 0, "a null folder yields an empty list");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static void VerifyTargetMapping()
    {
        Require(ConversionTarget.Mp4.Extension() == ".mp4", "MP4 extension");
        Require(ConversionTarget.Mkv.Extension() == ".mkv", "MKV extension");
        Require(ConversionTarget.Webm.Extension() == ".webm", "WebM extension");
        Require(ConversionTarget.Gif.Extension() == ".gif", "GIF extension");
        Require(ConversionTarget.Mp3.Extension() == ".mp3", "MP3 extension");
        Require(ConversionTarget.M4a.Extension() == ".m4a", "M4A extension");
        Require(ConversionTarget.Opus.Extension() == ".opus", "Opus extension");
        Require(ConversionTarget.Flac.Extension() == ".flac", "FLAC extension");
        Require(ConversionTarget.Wav.Extension() == ".wav", "WAV extension");

        Require(ConversionTarget.Mp3.IsAudio() && ConversionTarget.Flac.IsAudio(), "audio targets are audio");
        Require(!ConversionTarget.Mp4.IsAudio() && !ConversionTarget.Gif.IsAudio(), "video targets are not audio");

        foreach (var target in Enum.GetValues<ConversionTarget>())
        {
            Require(!string.IsNullOrWhiteSpace(target.Label()), "every target has a label: " + target);
            Require(!string.IsNullOrWhiteSpace(target.ShortLabel()), "every target has a short label: " + target);
        }

        // A queued conversion must not follow later UI edits.
        var live = new ConversionOptions { SourcePath = "/a.mp4", Target = ConversionTarget.Mp4 };
        var snapshot = live.Snapshot();
        live.Target = ConversionTarget.Mp3;
        live.SourcePath = "/b.mp4";
        Require(snapshot.Target == ConversionTarget.Mp4 && snapshot.SourcePath == "/a.mp4",
            "conversion snapshots are independent of later edits");
    }

    /// <summary>
    /// Every English key must exist in the Czech dictionary as well; new UI strings are
    /// therefore either translated or explicitly identical.
    /// </summary>
    private static void VerifyLocalizationParity()
    {
        var root = FindRepositoryRoot();
        if (root is null)
        {
            return;
        }

        var locales = Path.Combine(root, "src", "YtDlpUi.Core", "Locales");
        var englishPath = Path.Combine(locales, "en.json");
        var czechPath = Path.Combine(locales, "cs.json");
        Require(File.Exists(englishPath) && File.Exists(czechPath), "both locale dictionaries must exist");

        using var english = JsonDocument.Parse(File.ReadAllText(englishPath));
        using var czech = JsonDocument.Parse(File.ReadAllText(czechPath));
        var englishKeys = english.RootElement.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        var czechKeys = czech.RootElement.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);

        var missingInCzech = englishKeys.Except(czechKeys, StringComparer.Ordinal).ToList();
        var missingInEnglish = czechKeys.Except(englishKeys, StringComparer.Ordinal).ToList();
        Require(missingInCzech.Count == 0, "missing Czech translations: " + string.Join(" | ", missingInCzech));
        Require(missingInEnglish.Count == 0, "keys absent from en.json: " + string.Join(" | ", missingInEnglish));
        Require(englishKeys.Count >= 100, "locale dictionaries should cover the whole interface");
    }

    /// <summary>Build a sample media path with the current platform's separator.</summary>
    private static string Media(string fileName) => Path.Combine("media", fileName);

    private static string? IndexOfValue(List<string> arguments, string flag)
    {
        var index = arguments.LastIndexOf(flag);
        return index >= 0 && index + 1 < arguments.Count ? arguments[index + 1] : null;
    }

    private static string? FindRepositoryRoot()
    {
        for (var path = new DirectoryInfo(Environment.CurrentDirectory); path is not null; path = path.Parent)
        {
            if (File.Exists(Path.Combine(path.FullName, "YtDlpUi.slnx")))
            {
                return path.FullName;
            }
        }

        return null;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException("MEDIA SMOKE TEST FAILED: " + message);
        }
    }
}
