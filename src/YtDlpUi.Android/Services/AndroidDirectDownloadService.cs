using Android.App;
using Android.Content;
using Android.OS;

namespace YtDlpUi.Android.Services;

/// <summary>
/// First mobile adapter: download *direct* HTTPS media files with the system DownloadManager.
/// yt-dlp site extraction is intentionally NOT implemented by this adapter.
/// No storage permission or background execution permission is requested.
 /// </summary>
public sealed class AndroidDirectDownloadService
{
    /// <summary>
    /// Queue one verified direct-media URL in Android Downloads.
    /// The OS manages notification, retries and background transfer.
    /// </summary>
    public long Enqueue(Uri directUrl, string fileName)
    {
        if (directUrl.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("A direct HTTPS media file is required.");
        }

        var context = global::Android.App.Application.Context;
        var manager = context.GetSystemService(Context.DownloadService) as DownloadManager
            ?? throw new InvalidOperationException("Android DownloadManager is unavailable.");

        using var request = new DownloadManager.Request(global::Android.Net.Uri.Parse(directUrl.AbsoluteUri));
        request.SetTitle(fileName);
        request.SetDescription("Afrowave download");
        request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
        request.SetDestinationInExternalPublicDir(global::Android.OS.Environment.DirectoryDownloads, fileName);
        return manager.Enqueue(request);
    }
}
