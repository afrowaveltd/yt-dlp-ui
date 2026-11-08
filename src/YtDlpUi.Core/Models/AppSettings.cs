using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YtDlpUi.Core.Models;

/// <summary>
/// Persisted user preferences (application behaviour, not per-download options).
/// Observable so the settings page can bind straight to it.
/// </summary>
public sealed partial class AppSettings : ObservableObject
{
    /// <summary>Explicit path to a yt-dlp executable, overriding PATH detection.</summary>
    [ObservableProperty] public partial string? YtDlpPath { get; set; }

    /// <summary>Explicit path to an ffmpeg binary or its folder.</summary>
    [ObservableProperty] public partial string? FfmpegPath { get; set; }

    /// <summary>Folder pre-filled in the destination box.</summary>
    [ObservableProperty] public partial string DefaultOutputDirectory { get; set; } = string.Empty;

    /// <summary>Automatically fill the link box from the clipboard.</summary>
    [ObservableProperty] public partial bool AutoFillFromClipboard { get; set; } = true;

    /// <summary>Watch the clipboard while the window is open.</summary>
    [ObservableProperty] public partial bool WatchClipboardContinuously { get; set; } = true;

    /// <summary>Check GitHub for a newer yt-dlp release at startup.</summary>
    [ObservableProperty] public partial bool CheckForYtDlpUpdates { get; set; } = true;

    /// <summary>Analyse the link automatically once it is pasted.</summary>
    [ObservableProperty] public partial bool AutoAnalyzeOnPaste { get; set; } = true;

    /// <summary>Remember every advanced option between runs.</summary>
    [ObservableProperty] public partial bool RememberOptions { get; set; } = true;

    /// <summary>Snapshot of the last used download options as JSON.</summary>
    public string? LastOptionsJson { get; set; }

    /// <summary>UI language: "system", "en" or "cs". Stored independently from OS settings.</summary>
    [ObservableProperty] public partial string Language { get; set; } = "system";

    /// <summary>Use the dark theme.</summary>
    [ObservableProperty] public partial bool DarkTheme { get; set; } = true;

    /// <summary>Sequential download limit; 1 keeps output readable.</summary>
    [ObservableProperty] public partial int MaxConcurrentDownloads { get; set; } = 1;

    /// <summary>Last window width, used to restore the layout.</summary>
    public double WindowWidth { get; set; }

    /// <summary>Last window height, used to restore the layout.</summary>
    public double WindowHeight { get; set; }

    /// <summary>True once the welcome card was dismissed.</summary>
    [ObservableProperty] public partial bool FirstRunCompleted { get; set; }

    /// <summary>Option names that must never be written to disk.</summary>
    [JsonIgnore]
    public static IReadOnlyList<string> SecretOptionNames { get; } =
    [
        nameof(DownloadOptions.Password),
        nameof(DownloadOptions.TwoFactor),
        nameof(DownloadOptions.VideoPassword),
        nameof(DownloadOptions.ClientCertificatePassword),
    ];
}
