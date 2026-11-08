using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YtDlpUi.Android.Services;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Android.ViewModels;

/// <summary>Simple touch-first view model with explicit, honest download capabilities.</summary>
public sealed partial class MobileViewModel : ObservableObject
{
    private readonly AndroidDirectDownloadService _downloader;

    /// <summary>Create a VM with the Android-specific download adapter.</summary>
    public MobileViewModel(AndroidDirectDownloadService downloader)
    {
        _downloader = downloader;
    }

    /// <summary>Editable link, also populated by Android ACTION_SEND.</summary>
    [ObservableProperty] public partial string Link { get; set; } = string.Empty;

    /// <summary>Status message; direct-link vs yt-dlp page distinction is deliberate.</summary>
    [ObservableProperty] public partial string Status { get; set; } =
        "Paste a direct media link or share a link from another app.";

    /// <summary>Downloads queued in this activity session (Android owns transfer progress).</summary>
    public ObservableCollection<string> RecentDownloads { get; } = [];

    /// <summary>Do not download automatically merely because another app shared a URL.</summary>
    public void ReceiveLink(string url)
    {
        Link = url;
        Status = MobileLinkParser.TryGetDirectMedia(url, out _, out _)
            ? "Direct HTTPS media file detected. Tap Download to confirm."
            : "Link received. yt-dlp website extraction is not available in this first Android prototype.";
    }

    /// <summary>Explicitly enqueue direct HTTPS files through Android's own download service.</summary>
    [RelayCommand]
    private void DownloadDirectFile()
    {
        if (!MobileLinkParser.TryGetDirectMedia(Link, out var uri, out var fileName) || uri is null)
        {
            Status = "This is not a direct HTTPS media file. yt-dlp site downloads are not available yet.";
            return;
        }

        try
        {
            var id = _downloader.Enqueue(uri, fileName);
            RecentDownloads.Insert(0, fileName);
            Status = $"Queued in Android Downloads (job {id}). Open system Downloads for progress.";
        }
        catch (Exception ex)
        {
            Status = $"Could not queue the download: {ex.Message}";
        }
    }

    /// <summary>Clear the input without affecting already queued system downloads.</summary>
    [RelayCommand]
    private void ClearLink()
    {
        Link = string.Empty;
        Status = "Paste a direct media link or share a link from another app.";
    }
}
