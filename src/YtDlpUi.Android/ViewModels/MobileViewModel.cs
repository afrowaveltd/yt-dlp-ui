using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YtDlpUi.Android.Services;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Android.ViewModels;

/// <summary>A fixed selection of stream presets; no FFmpeg transcode is implied.</summary>
public sealed record MobileQualityOption(string Label, MobileDownloadQuality Quality);

/// <summary>
/// Android-specific UI. Uses the system DownloadManager for direct files
/// and embedded yt-dlp for website links. Neither starts automatically on share.
/// </summary>
public sealed partial class MobileViewModel : ObservableObject
{
    private readonly AndroidDirectDownloadService _directDownloader;
    private readonly AndroidYtDlpService _siteDownloader;

    /// <summary>Mobile view model with separate platform download engines.</summary>
    public MobileViewModel(AndroidDirectDownloadService directDownloader, AndroidYtDlpService siteDownloader)
    {
        _directDownloader = directDownloader;
        _siteDownloader = siteDownloader;
    }

    /// <summary>URL entered or shared from another Android application.</summary>
    [ObservableProperty] public partial string Link { get; set; } = string.Empty;

    /// <summary>Short user-facing progress/status information.</summary>
    [ObservableProperty] public partial string Status { get; set; } =
        "Paste a video link or use the Android Share menu.";

    /// <summary>Current percentage reported by embedded yt-dlp.</summary>
    [ObservableProperty] public partial double ProgressPercent { get; set; }

    /// <summary>Prevent duplicate submissions while embedded Python is running.</summary>
    [ObservableProperty] public partial bool IsBusy { get; set; }

    /// <summary>Only formats supported without bundling FFmpeg.</summary>
    public IReadOnlyList<MobileQualityOption> QualityOptions { get; } =
    [
        new("Best single stream", MobileDownloadQuality.Best),
        new("Up to 720p", MobileDownloadQuality.UpTo720p),
        new("Up to 480p", MobileDownloadQuality.UpTo480p),
        new("Original audio stream", MobileDownloadQuality.AudioOriginal)
    ];

    /// <summary>Selected quality preset for website extraction.</summary>
    [ObservableProperty] public partial MobileQualityOption? SelectedQuality { get; set; } =
        new("Best single stream", MobileDownloadQuality.Best);

    /// <summary>Files submitted in this activity session (not a persistent queue).</summary>
    public ObservableCollection<string> RecentDownloads { get; } = [];

    /// <summary>
    /// Receive a Share intent. User must still tap Download; shares never
    /// trigger network access on their own.
    /// </summary>
    public void ReceiveLink(string url)
    {
        Link = url;
        Status = MobileLinkParser.TryGetDirectMedia(url, out _, out _)
            ? "Direct HTTPS media file detected. Tap Download to confirm."
            : "Video-page link received. Tap Download to try the embedded yt-dlp engine.";
    }

    /// <summary>Download a direct file with Android or use embedded yt-dlp for a website link.</summary>
    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!MobileLinkParser.TryExtract(Link, out var url) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var parsed) ||
            parsed.Scheme != Uri.UriSchemeHttps)
        {
            Status = "A valid HTTPS link is required.";
            return;
        }

        if (MobileLinkParser.TryGetDirectMedia(url, out var directUri, out var directName) &&
            directUri is not null)
        {
            try
            {
                var id = _directDownloader.Enqueue(directUri, directName);
                RecentDownloads.Insert(0, directName);
                Status = $"Queued with Android DownloadManager (job {id}).";
            }
            catch (Exception)
            {
                Status = "Android could not queue the direct file. Check network and Downloads.";
            }

            return;
        }

        IsBusy = true;
        ProgressPercent = 0;
        Status = "Initializing embedded yt-dlp (first launch may take longer)…";

        try
        {
            var quality = SelectedQuality?.Quality ?? MobileDownloadQuality.Best;
            var result = await _siteDownloader.DownloadAsync(parsed, quality,
                progress => Dispatcher.UIThread.Post(() =>
                {
                    ProgressPercent = progress;
                    Status = $"Downloading with yt-dlp: {progress:0.0}%";
                }));

            foreach (var name in result.Files)
            {
                RecentDownloads.Insert(0, name);
            }

            ProgressPercent = 100;
            Status = $"Completed: {result.Files.Count} file(s) saved to Downloads/Afrowave.";
        }
        catch (Exception)
        {
            // Upstream errors can embed media URLs, cookies, or tokens.
            // Do not echo full exceptions into the mobile UI.
            Status = "yt-dlp download failed. This site may require cookies, a newer engine, or FFmpeg.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Open Android's standard Downloads screen.</summary>
    [RelayCommand]
    private void OpenDownloads() => _directDownloader.OpenDownloads();

    /// <summary>Clear input without affecting active transfers.</summary>
    [RelayCommand]
    private void ClearLink()
    {
        if (IsBusy)
        {
            return;
        }

        Link = string.Empty;
        Status = "Paste a video link or use the Android Share menu.";
        ProgressPercent = 0;
    }
}
