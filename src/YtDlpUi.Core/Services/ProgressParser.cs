using System.Globalization;
using System.Text.RegularExpressions;

namespace YtDlpUi.Core.Services;

/// <summary>Severity of a yt-dlp log line.</summary>
public enum YtDlpLogLevel
{
    Info,
    Warning,
    Error,
    Debug,
}

/// <summary>A snapshot of download progress for one item.</summary>
public sealed record YtDlpProgress(
    string Status,
    double? DownloadedBytes,
    double? TotalBytes,
    double? Speed,
    double? EtaSeconds,
    int? FragmentIndex,
    int? FragmentCount)
{
    /// <summary>Completion percentage in the range 0-100, when it can be derived.</summary>
    public double? Percent
    {
        get
        {
            if (Status is "finished")
            {
                return 100;
            }

            if (DownloadedBytes is not > 0)
            {
                return null;
            }

            var total = TotalBytes;
            if (total is > 0)
            {
                return Math.Clamp(DownloadedBytes.Value / total.Value * 100, 0, 100);
            }

            if (FragmentIndex is > 0 && FragmentCount is > 0)
            {
                return Math.Clamp((double)FragmentIndex.Value / FragmentCount.Value * 100, 0, 100);
            }

            return null;
        }
    }

    /// <summary>True when the byte counters are known well enough to show a bar.</summary>
    public bool HasDeterminateSize => TotalBytes is > 0;
}

/// <summary>Something interesting that happened in the yt-dlp output stream.</summary>
public abstract record YtDlpEvent;

/// <summary>Parsed download progress.</summary>
public sealed record ProgressEvent(YtDlpProgress Progress) : YtDlpEvent;

/// <summary>Post-processing step started or finished.</summary>
public sealed record PostProcessEvent(string Status, string PostProcessor) : YtDlpEvent;

/// <summary>Identified output file, filled as soon as yt-dlp knows it.</summary>
public sealed record DestinationEvent(string Path) : YtDlpEvent;

/// <summary>Plain log text.</summary>
public sealed record LogEvent(string Text, YtDlpLogLevel Level) : YtDlpEvent;

/// <summary>Turns yt-dlp console output into typed events for the GUI.</summary>
public static partial class ProgressParser
{
    private static readonly string[] NotAvailable = ["NA", "None", "N/A", "null", "-", ""];

    [GeneratedRegex(@"^\[[A-Za-z]+\] Destination: (.+)$", RegexOptions.Compiled)]
    private static partial Regex DestinationPattern();

    /// <summary>Parse a single output line. Returns null for lines that carry no useful signal.</summary>
    public static YtDlpEvent? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        if (line.StartsWith(YtDlpArgumentBuilder.ProgressMarker, StringComparison.Ordinal))
        {
            return ParseDownloadProgress(line[YtDlpArgumentBuilder.ProgressMarker.Length..]);
        }

        if (line.StartsWith(YtDlpArgumentBuilder.PostProcessMarker, StringComparison.Ordinal))
        {
            return ParsePostProcess(line[YtDlpArgumentBuilder.PostProcessMarker.Length..]);
        }

        var destination = DestinationPattern().Match(line);
        if (destination.Success)
        {
            return new DestinationEvent(destination.Groups[1].Value.Trim());
        }

        if (line.StartsWith("[Merger] Merging formats into \"", StringComparison.Ordinal))
        {
            var path = line["[Merger] Merging formats into \"".Length..].TrimEnd('"');
            return new DestinationEvent(path);
        }

        if (line.StartsWith("ERROR:", StringComparison.Ordinal))
        {
            return new LogEvent(line, YtDlpLogLevel.Error);
        }

        if (line.StartsWith("WARNING:", StringComparison.Ordinal))
        {
            return new LogEvent(line, YtDlpLogLevel.Warning);
        }

        if (line.Contains("ERROR:", StringComparison.Ordinal) && line.StartsWith('['))
        {
            return new LogEvent(line, YtDlpLogLevel.Error);
        }

        if (line.StartsWith("[debug]", StringComparison.Ordinal))
        {
            return new LogEvent(line, YtDlpLogLevel.Debug);
        }

        return new LogEvent(line, YtDlpLogLevel.Info);
    }

    private static YtDlpEvent? ParseDownloadProgress(string payload)
    {
        var fields = payload.Split(YtDlpArgumentBuilder.FieldSeparator);
        if (fields.Length < 8)
        {
            return null;
        }

        var status = fields[0].Trim();
        var progress = new YtDlpProgress(
            Status: status.Length == 0 ? "downloading" : status,
            DownloadedBytes: ToDouble(fields[1]),
            TotalBytes: ToDouble(fields[2]) ?? ToDouble(fields[3]),
            Speed: ToDouble(fields[4]),
            EtaSeconds: ToDouble(fields[5]),
            FragmentIndex: ToInt(fields[6]),
            FragmentCount: ToInt(fields[7]));

        return new ProgressEvent(progress);
    }

    private static YtDlpEvent ParsePostProcess(string payload)
    {
        var fields = payload.Split(YtDlpArgumentBuilder.FieldSeparator);
        var status = fields.Length > 0 ? fields[0].Trim() : string.Empty;
        var processor = fields.Length > 1 ? fields[1].Trim() : string.Empty;
        return new PostProcessEvent(status, processor);
    }

    private static double? ToDouble(string raw)
    {
        var value = raw.Trim();
        if (NotAvailable.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static int? ToInt(string raw)
    {
        var value = ToDouble(raw);
        return value is null ? null : (int)value.Value;
    }
}
