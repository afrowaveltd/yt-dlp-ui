using System.Globalization;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

/// <summary>
/// Translates <see cref="ConversionOptions"/> into an FFmpeg argument vector.
/// Kept free of process handling so the mapping can be unit-tested without running FFmpeg.
/// </summary>
public static class FfmpegArgumentBuilder
{
    /// <summary>Height used for GIF output when the user keeps the original size.</summary>
    public const int DefaultGifHeight = 480;

    /// <summary>
    /// Build the argument vector for one conversion.
    /// </summary>
    /// <param name="options">User configuration.</param>
    /// <param name="sourceOverride">Optional source path, used instead of <see cref="ConversionOptions.SourcePath"/>.</param>
    public static List<string> Build(ConversionOptions options, string? sourceOverride = null)
    {
        var source = ResolveSource(options, sourceOverride);
        var output = BuildOutputPath(options, sourceOverride);
        var isAudio = options.Target.IsAudio();
        var isGif = options.Target == ConversionTarget.Gif;
        var args = new List<string>
        {
            "-hide_banner",
            "-nostdin",
            options.OverwriteExisting ? "-y" : "-n",
            "-progress",
            "pipe:1",
            "-nostats",
        };

        // Seeking before the input is much faster; the cut is still frame-accurate
        // because FFmpeg decodes from the previous keyframe.
        if (TryParseTime(options.StartTime, out var start) && start > 0)
        {
            args.Add("-ss");
            args.Add(start.ToString("0.###", CultureInfo.InvariantCulture));
        }

        args.Add("-i");
        args.Add(source);

        if (TryParseTime(options.EndTime, out var end) && end > 0)
        {
            var hasStart = TryParseTime(options.StartTime, out var startForLength) && startForLength > 0;
            if (hasStart && end > startForLength)
            {
                // -t after -i is relative to the (already seeked) output timeline.
                args.Add("-t");
                args.Add((end - startForLength).ToString("0.###", CultureInfo.InvariantCulture));
            }
            else
            {
                args.Add("-to");
                args.Add(end.ToString("0.###", CultureInfo.InvariantCulture));
            }
        }

        if (isAudio)
        {
            AddAudioOnlyOptions(options, args);
        }
        else if (isGif)
        {
            AddGifOptions(options, args);
        }
        else
        {
            AddVideoOptions(options, args);
        }

        args.Add(output);
        return args;
    }

    /// <summary>
    /// Destination path for the converted file. A conversion that would overwrite its own source
    /// gets a <c> (converted)</c> suffix instead of destroying the original.
    /// </summary>
    public static string BuildOutputPath(ConversionOptions options, string? sourceOverride = null)
    {
        var source = ResolveSource(options, sourceOverride);
        var directory = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Path.GetDirectoryName(source)
            : options.OutputDirectory.Trim();

        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = Directory.GetCurrentDirectory();
        }

        var baseName = Path.GetFileNameWithoutExtension(source);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "converted";
        }

        var candidate = Path.Combine(directory!, baseName + options.Target.Extension());

        if (PathsEqual(candidate, source))
        {
            candidate = Path.Combine(directory!, baseName + " (converted)" + options.Target.Extension());
        }

        return candidate;
    }

    /// <summary>Resolve the source path, falling back to an override or the option value.</summary>
    public static string ResolveSource(ConversionOptions options, string? sourceOverride = null)
    {
        var source = !string.IsNullOrWhiteSpace(sourceOverride)
            ? sourceOverride!
            : options.SourcePath ?? string.Empty;

        return string.IsNullOrWhiteSpace(source) ? string.Empty : source.Trim();
    }

    /// <summary>
    /// Parse a user-entered time into seconds. Accepts <c>90</c>, <c>1:30</c>,
    /// <c>00:01:30</c> and <c>1:00:00.5</c>. Returns false for blank or invalid input.
    /// </summary>
    public static bool TryParseTime(string? text, out double seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.Trim().Replace(',', '.');
        if (trimmed.Contains(':'))
        {
            var parts = trimmed.Split(':');
            if (parts.Length is < 2 or > 3)
            {
                return false;
            }

            var total = 0d;
            foreach (var part in parts)
            {
                if (!double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || value < 0)
                {
                    return false;
                }

                total = (total * 60) + value;
            }

            seconds = total;
            return true;
        }

        if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var simple) || simple < 0)
        {
            return false;
        }

        seconds = simple;
        return true;
    }

    /// <summary>Video bitrate/CRF tier for the requested quality.</summary>
    public static int CrfFor(ConversionQuality quality) => quality switch
    {
        ConversionQuality.High => 18,
        ConversionQuality.Balanced => 23,
        _ => 28,
    };

    /// <summary>Encoder speed preset for the requested quality.</summary>
    public static string PresetFor(ConversionQuality quality) => quality switch
    {
        ConversionQuality.High => "slow",
        ConversionQuality.Balanced => "medium",
        _ => "veryfast",
    };

    /// <summary>Audio bitrate for the requested quality and codec family.</summary>
    public static string AudioBitrateFor(ConversionTarget target, ConversionQuality quality)
    {
        var opus = target is ConversionTarget.Opus or ConversionTarget.Webm;
        return (opus, quality) switch
        {
            (true, ConversionQuality.High) => "160k",
            (true, ConversionQuality.Balanced) => "128k",
            (true, _) => "96k",
            (false, ConversionQuality.High) => "320k",
            (false, ConversionQuality.Balanced) => "192k",
            (false, _) => "128k",
        };
    }

    private static void AddVideoOptions(ConversionOptions options, List<string> args)
    {
        var webm = options.Target == ConversionTarget.Webm;

        if (options.MaxHeight > 0)
        {
            // min(ih,H) prevents upscaling a smaller source; the inner comma must be escaped.
            args.Add("-vf");
            args.Add($"scale=-2:min(ih\\,{options.MaxHeight})");
        }

        args.Add("-c:v");
        args.Add(webm ? "libvpx-vp9" : "libx264");

        if (webm)
        {
            args.Add("-crf");
            args.Add(CrfFor(options.Quality).ToString(CultureInfo.InvariantCulture));
            args.Add("-b:v");
            args.Add("0");
        }
        else
        {
            args.Add("-preset");
            args.Add(PresetFor(options.Quality));
            args.Add("-crf");
            args.Add(CrfFor(options.Quality).ToString(CultureInfo.InvariantCulture));
            args.Add("-pix_fmt");
            args.Add("yuv420p");
        }

        if (options.Target is ConversionTarget.Mp4 or ConversionTarget.Mov)
        {
            // Put the index at the front so browsers and TVs can start playing immediately.
            args.Add("-movflags");
            args.Add("+faststart");
        }

        if (options.RemoveAudio)
        {
            args.Add("-an");
        }
        else
        {
            args.Add("-c:a");
            args.Add(webm ? "libopus" : "aac");
            args.Add("-b:a");
            args.Add(AudioBitrateFor(options.Target, options.Quality));
        }
    }

    private static void AddAudioOnlyOptions(ConversionOptions options, List<string> args)
    {
        args.Add("-vn");

        switch (options.Target)
        {
            case ConversionTarget.Mp3:
                args.Add("-c:a");
                args.Add("libmp3lame");
                break;
            case ConversionTarget.M4a:
                args.Add("-c:a");
                args.Add("aac");
                break;
            case ConversionTarget.Opus:
                args.Add("-c:a");
                args.Add("libopus");
                break;
            case ConversionTarget.Flac:
                args.Add("-c:a");
                args.Add("flac");
                break;
            case ConversionTarget.Wav:
                args.Add("-c:a");
                args.Add("pcm_s16le");
                break;
        }

        if (options.Target is ConversionTarget.Mp3 or ConversionTarget.M4a or ConversionTarget.Opus)
        {
            args.Add("-b:a");
            args.Add(AudioBitrateFor(options.Target, options.Quality));
        }
    }

    private static void AddGifOptions(ConversionOptions options, List<string> args)
    {
        var height = options.MaxHeight > 0
            ? Math.Clamp(options.MaxHeight, 64, 2160)
            : DefaultGifHeight;

        // Single-pass palette generation keeps colours sane and needs no temporary files.
        args.Add("-vf");
        args.Add($"fps=12,scale=-2:min(ih\\,{height}):flags=lanczos,split[a][b];[a]palettegen[p];[b][p]paletteuse");
        args.Add("-loop");
        args.Add("0");
        args.Add("-an");
    }

    private static bool PathsEqual(string first, string second)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(first),
                Path.GetFullPath(second),
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
