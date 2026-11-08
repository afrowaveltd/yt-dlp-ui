using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Converters;

/// <summary>Maps booleans to a dimmed opacity for disabled-looking text.</summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public double TrueValue { get; set; } = 1.0;

    public double FalseValue { get; set; } = 0.45;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? TrueValue : FalseValue;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>Inverts a boolean.</summary>
public sealed class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;
}

/// <summary>True when a string is present and not whitespace.</summary>
public sealed class StringNotEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && !string.IsNullOrWhiteSpace(text);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>Renders enum values with Czech, human-friendly labels.</summary>
public sealed class EnumLabelConverter : IValueConverter
{
    /// <summary>Labels for every enum surfaced in the UI.</summary>
    public static IReadOnlyDictionary<object, string> Labels { get; } = new Dictionary<object, string>
    {
        [PlaylistHandling.Default] = "Automaticky (podle yt-dlp)",
        [PlaylistHandling.VideoOnly] = "Pouze jedno video",
        [PlaylistHandling.PlaylistOnly] = "Celý playlist",

        [OverwritePolicy.Default] = "Výchozí (hotová videa nepřepisovat)",
        [OverwritePolicy.NoOverwrites] = "Nikdy nepřepisovat",
        [OverwritePolicy.ForceOverwrites] = "Vždy přepsat",

        [MergeContainer.Default] = "Automaticky",
        [MergeContainer.Mp4] = "MP4 (nejlépe přehratelné)",
        [MergeContainer.Mkv] = "MKV (nejvíce možností)",
        [MergeContainer.Webm] = "WEBM",
        [MergeContainer.Mov] = "MOV",
        [MergeContainer.Avi] = "AVI",
        [MergeContainer.Flv] = "FLV",

        [LinkFileFormat.None] = "Nevytvářet",
        [LinkFileFormat.Desktop] = "Linux (.desktop)",
        [LinkFileFormat.Url] = "Windows (.url)",
        [LinkFileFormat.Webloc] = "macOS (.webloc)",

        [DownloadPreset.BestVideo] = "Nejlepší kvalita",
        [DownloadPreset.Video2160] = "4K (2160p)",
        [DownloadPreset.Video1440] = "2K (1440p)",
        [DownloadPreset.Video1080] = "Full HD (1080p)",
        [DownloadPreset.Video720] = "HD (720p)",
        [DownloadPreset.Video480] = "480p",
        [DownloadPreset.Video360] = "360p",
        [DownloadPreset.VideoSmallest] = "Nejmenší soubor",
        [DownloadPreset.AudioMp3] = "MP3",
        [DownloadPreset.AudioM4a] = "M4A (AAC)",
        [DownloadPreset.AudioOpus] = "Opus",
        [DownloadPreset.AudioFlac] = "FLAC",
        [DownloadPreset.AudioWav] = "WAV",
        [DownloadPreset.AudioBest] = "Původní stopa",
        [DownloadPreset.Custom] = "Vlastní nastavení",
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return Labels.TryGetValue(value, out var label) ? label : value.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>Colours log lines by their kind.</summary>
public sealed class LogKindBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value switch
        {
            LogKind.Error => "ErrorBrush",
            LogKind.Warning => "WarningBrush",
            LogKind.Debug => "TextMutedBrush",
            LogKind.App => "AccentBrush",
            _ => "TextSecondaryBrush",
        };

        if (Application.Current?.TryGetResource(key, null, out var brush) == true && brush is IBrush found)
        {
            return found;
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>Colours queue items by their state.</summary>
public sealed class DownloadStateBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value switch
        {
            DownloadState.Completed => "SuccessBrush",
            DownloadState.Failed => "ErrorBrush",
            DownloadState.Cancelled => "TextMutedBrush",
            DownloadState.Downloading => "AccentBrush",
            DownloadState.Analyzing => "AccentBrush",
            DownloadState.Queued => "WarningBrush",
            _ => "TextSecondaryBrush",
        };

        if (Application.Current?.TryGetResource(key, null, out var brush) == true && brush is IBrush found)
        {
            return found;
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>True when the value is null, used for indeterminate progress bars.</summary>
public sealed class IsNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>True when the value is not null.</summary>
public sealed class NotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>True when a count is zero.</summary>
public sealed class IsZeroConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int count && count == 0;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>Green dot for available tools, red for missing ones.</summary>
public sealed class AvailableBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value is true ? "SuccessBrush" : "ErrorBrush";
        if (Application.Current?.TryGetResource(key, null, out var brush) == true && brush is IBrush found)
        {
            return found;
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>Formats byte counts.</summary>
public sealed class BytesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            long l => Formatting.Bytes(l),
            int i => Formatting.Bytes(i),
            double d => Formatting.Bytes((long)d),
            _ => "–",
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

/// <summary>Two-way converter for integer option boxes: blank or invalid input becomes 0.</summary>
public sealed class IntTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int number ? number.ToString(CultureInfo.CurrentCulture) : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsed)
            ? parsed
            : 0;
    }
}

/// <summary>Two-way converter for floating point option boxes using the current culture.</summary>
public sealed class DoubleTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            double d => d == 0 ? string.Empty : d.ToString("0.###", CultureInfo.CurrentCulture),
            _ => string.Empty,
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return 0d;
        }

        text = text.Trim().Replace(',', '.');
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0d;
    }
}

/// <summary>Formats durations as <c>h:mm:ss</c>.</summary>
public sealed class DurationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double seconds ? Formatting.Duration(seconds) : "–";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}
