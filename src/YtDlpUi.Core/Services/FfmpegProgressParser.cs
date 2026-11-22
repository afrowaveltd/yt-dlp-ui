using System.Globalization;

namespace YtDlpUi.Core.Services;

/// <summary>Snapshot of FFmpeg's machine-readable progress block.</summary>
public sealed record FfmpegProgress(
    double? OutTimeSeconds,
    double? SpeedFactor,
    double? BitrateKbps,
    long? TotalSizeBytes,
    long? Frame,
    bool IsComplete)
{
    /// <summary>Completion percentage against the known input duration, when both are available.</summary>
    public double? Percent(double? durationSeconds)
    {
        if (IsComplete)
        {
            return 100;
        }

        if (durationSeconds is not > 0 || OutTimeSeconds is not > 0)
        {
            return null;
        }

        return Math.Clamp(OutTimeSeconds.Value / durationSeconds!.Value * 100, 0, 100);
    }
}

/// <summary>
/// Turns FFmpeg's <c>-progress pipe:1</c> output into <see cref="FfmpegProgress"/> snapshots.
/// FFmpeg prints key=value pairs and finishes a block with <c>progress=continue|end</c>.
/// </summary>
public sealed class FfmpegProgressAccumulator
{
    private double? _outTime;
    private double? _speed;
    private double? _bitrate;
    private long? _size;
    private long? _frame;

    /// <summary>Apply one output line. Returns a snapshot when a progress block completes.</summary>
    public FfmpegProgress? Feed(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        var separator = line.IndexOf('=');
        if (separator <= 0)
        {
            return null;
        }

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim();

        switch (key)
        {
            case "out_time_us":
                // Microseconds; authoritative when present.
                if (TryParseDouble(value, out var micro)) _outTime = micro / 1_000_000d;
                break;
            case "out_time":
                if (TryParseTime(value, out var seconds)) _outTime = seconds;
                break;
            case "speed":
                if (TryParseSpeed(value, out var speed)) _speed = speed;
                break;
            case "bitrate":
                if (TryParseBitrate(value, out var bitrate)) _bitrate = bitrate;
                break;
            case "total_size":
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var size)) _size = size;
                break;
            case "frame":
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var frame)) _frame = frame;
                break;
            case "progress":
                var complete = string.Equals(value, "end", StringComparison.OrdinalIgnoreCase);
                var snapshot = new FfmpegProgress(_outTime, _speed, _bitrate, _size, _frame, complete);
                if (complete)
                {
                    Reset();
                }

                return snapshot;
        }

        return null;
    }

    /// <summary>Clear the accumulated values, for example before a new run.</summary>
    public void Reset()
    {
        _outTime = null;
        _speed = null;
        _bitrate = null;
        _size = null;
        _frame = null;
    }

    /// <summary>Read the <c>out_time</c> form <c>HH:MM:SS.microseconds</c>.</summary>
    public static bool TryParseTime(string value, out double seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var negative = value.StartsWith('-');
        if (negative)
        {
            value = value[1..];
        }

        var parts = value.Split(':');
        if (parts.Length is < 1 or > 3)
        {
            return false;
        }

        var total = 0d;
        foreach (var part in parts)
        {
            if (!TryParseDouble(part, out var component) || component < 0)
            {
                return false;
            }

            total = (total * 60) + component;
        }

        seconds = negative ? 0 : total;
        return true;
    }

    /// <summary>Read the <c>speed=1.23x</c> form.</summary>
    public static bool TryParseSpeed(string value, out double speed)
    {
        speed = 0;
        var trimmed = value.Trim().TrimEnd('x', 'X');
        return TryParseDouble(trimmed, out speed) && speed >= 0;
    }

    /// <summary>Read <c>bitrate=1234.5kbits/s</c>, returning kbit/s.</summary>
    public static bool TryParseBitrate(string value, out double kbps)
    {
        kbps = 0;
        var trimmed = value.Trim();
        if (trimmed.EndsWith("kbits/s", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[..^"kbits/s".Length];
        }
        else if (trimmed.EndsWith("bits/s", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryParseDouble(trimmed[..^"bits/s".Length], out var bits))
            {
                return false;
            }

            kbps = bits / 1000d;
            return true;
        }

        trimmed = trimmed.TrimEnd('k', 'K');

        return TryParseDouble(trimmed, out kbps);
    }

    private static bool TryParseDouble(string value, out double parsed) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
}
