using CommunityToolkit.Mvvm.ComponentModel;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.ViewModels;

/// <summary>Lifecycle of one queue entry.</summary>
public enum DownloadState
{
    Queued,
    Analyzing,
    Downloading,
    PostProcessing,
    Completed,
    Failed,
    Cancelled,
}

/// <summary>Severity used for colouring a log line.</summary>
public enum LogKind
{
    App,
    Output,
    Warning,
    Error,
    Debug,
}

/// <summary>One line in the shared log pane.</summary>
public sealed record LogLine(string Text, LogKind Kind)
{
    /// <summary>Timestamp shown in front of the message.</summary>
    public string Stamp { get; } = DateTime.Now.ToString("HH:mm:ss");
}

/// <summary>A single URL waiting in, or passing through, the download queue.</summary>
public sealed partial class DownloadItemViewModel : ObservableObject
{
    public DownloadItemViewModel(string url)
    {
        Url = url;
        AddedAt = DateTime.Now;
    }

    /// <summary>The link being downloaded.</summary>
    public string Url { get; }

    /// <summary>When the item entered the queue.</summary>
    public DateTime AddedAt { get; }

    /// <summary>Resolved title, filled once analysis or the download reveals it.</summary>
    [ObservableProperty] public partial string? Title { get; set; }

    /// <summary>Current lifecycle state.</summary>
    [ObservableProperty] public partial DownloadState State { get; set; }

    /// <summary>Progress in percent; null keeps the bar indeterminate.</summary>
    [ObservableProperty] public partial double? Progress { get; set; }

    /// <summary>Short human status such as "Stahuji…".</summary>
    [ObservableProperty] public partial string StatusText { get; set; } = "Ve frontě";

    /// <summary>Formatted transfer speed.</summary>
    [ObservableProperty] public partial string SpeedText { get; set; } = string.Empty;

    /// <summary>Formatted remaining time.</summary>
    [ObservableProperty] public partial string EtaText { get; set; } = string.Empty;

    /// <summary>Formatted downloaded / total size.</summary>
    [ObservableProperty] public partial string SizeText { get; set; } = string.Empty;

    /// <summary>Final file path, if yt-dlp reported one.</summary>
    [ObservableProperty] public partial string? FilePath { get; set; }

    /// <summary>Error text for failed items.</summary>
    [ObservableProperty] public partial string? ErrorMessage { get; set; }

    /// <summary>The exact command line that will run for this item.</summary>
    [ObservableProperty] public partial string? CommandPreview { get; set; }

    /// <summary>Selected preset label recorded at queue time.</summary>
    [ObservableProperty] public partial string? PresetLabel { get; set; }

    /// <summary>Headline to show in the queue list.</summary>
    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? Url : Title!;

    /// <summary>Secondary line: host plus status.</summary>
    public string DisplaySubtitle
    {
        get
        {
            var host = Uri.TryCreate(Url, UriKind.Absolute, out var uri) ? uri.Host : Url;
            return string.IsNullOrWhiteSpace(PresetLabel) ? host : $"{host} · {PresetLabel}";
        }
    }

    /// <summary>True when the item finished successfully.</summary>
    public bool IsFinished => State is DownloadState.Completed or DownloadState.Failed or DownloadState.Cancelled;

    /// <summary>True while the item is actively being worked on.</summary>
    public bool IsRunning => State is DownloadState.Downloading or DownloadState.PostProcessing or DownloadState.Analyzing;

    /// <summary>Apply a parsed progress snapshot.</summary>
    public void ApplyProgress(YtDlpProgress progress)
    {
        Progress = progress.Percent;

        if (!string.IsNullOrWhiteSpace(progress.Status))
        {
            StatusText = progress.Status switch
            {
                "downloading" => "Stahuji…",
                "finished" => "Staženo, dokončuji…",
                _ => progress.Status,
            };
        }

        SpeedText = progress.Speed is > 0 ? Formatting.Speed(progress.Speed) : string.Empty;

        EtaText = progress.EtaSeconds is > 0
            ? Formatting.Duration(progress.EtaSeconds)
            : string.Empty;

        SizeText = progress.DownloadedBytes is > 0
            ? progress.TotalBytes is > 0
                ? $"{Formatting.Bytes((long)progress.DownloadedBytes.Value)} / {Formatting.Bytes((long)progress.TotalBytes.Value)}"
                : Formatting.Bytes((long)progress.DownloadedBytes.Value)
            : string.Empty;
    }

    /// <summary>Reset volatile fields before a retry.</summary>
    public void ResetForRetry()
    {
        State = DownloadState.Queued;
        Progress = null;
        StatusText = "Ve frontě";
        SpeedText = string.Empty;
        EtaText = string.Empty;
        SizeText = string.Empty;
        ErrorMessage = null;
        FilePath = null;
    }

    partial void OnStateChanged(DownloadState value)
    {
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsFinished));
        StatusText = value switch
        {
            DownloadState.Queued => "Ve frontě",
            DownloadState.Analyzing => "Analyzuji odkaz…",
            DownloadState.Downloading => "Stahuji…",
            DownloadState.PostProcessing => "Dokončuji soubor…",
            DownloadState.Completed => "Hotovo",
            DownloadState.Failed => "Chyba",
            DownloadState.Cancelled => "Zrušeno",
            _ => StatusText,
        };
    }

    partial void OnTitleChanged(string? value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
        OnPropertyChanged(nameof(DisplaySubtitle));
    }
}
