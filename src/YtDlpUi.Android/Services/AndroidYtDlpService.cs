using Android.Content;
using Android.OS;
using Android.Provider;
using Dev.Ffmpegkit_maintained.Ytdlp;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Android.Services;

/// <summary>Completed local yt-dlp download exported to the user's public Downloads.</summary>
/// <param name="Files">Names of successfully exported files.</param>
public sealed record MobileDownloadResult(IReadOnlyList<string> Files);

/// <summary>
/// In-process yt-dlp using the pinned MIT yt-dlp-android AAR (Python runs via
/// Chaquopy, not by executing files from app-writable storage).
/// This is an active-application preview, not a foreground download service.
/// </summary>
public sealed class AndroidYtDlpService
{
    // The initial preview supports one embedded download at a time.
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private static readonly HashSet<string> MediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mkv", ".webm", ".mov", ".m4v", ".3gp", ".mp3", ".m4a",
        ".aac", ".opus", ".ogg", ".flac", ".wav"
    };

    /// <summary>Initialize the bundled Python/yt-dlp runtime without downloading anything.</summary>
    public Task CheckRuntimeAsync() =>
        Task.Run(() => YtDlp.Init(global::Android.App.Application.Context));

    /// <summary>
    /// Download a website URL with yt-dlp, then publish the resulting files using
    /// MediaStore. No broad external-storage permission is required on API 29+.
    /// </summary>
    public async Task<MobileDownloadResult> DownloadAsync(
        Uri uri,
        MobileDownloadQuality quality,
        Action<float>? onProgress = null,
        CancellationToken cancellationToken = default)
    {
        if (uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new ArgumentException("A valid HTTPS media-page URL is required.", nameof(uri));
        }

        await Gate.WaitAsync(cancellationToken);
        try
        {
            var context = global::Android.App.Application.Context;
            var workRoot = context.GetExternalFilesDir(null)?.AbsolutePath ?? context.FilesDir?.AbsolutePath
                ?? throw new InvalidOperationException("Private app storage is unavailable.");

            var folder = Path.Combine(workRoot, "yt-dlp", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);

            // Keep all yt-dlp writes inside our own app-scoped directory.
            // No output-path, extra arguments or cookies come from untrusted links.
            var output = Path.Combine(folder, "%(title).100B [%(id)s].%(ext)s");

            try
            {
                // Call native Java facade; Python and yt-dlp are packaged within AAR.
                // This MUST NOT run on the UI thread.
                var exitCode = await Task.Run(() =>
                {
                    YtDlp.Init(context);
                    using var request = new YtDlpRequest(uri.AbsoluteUri);
                    request.SetOutputTemplate(output);
                    request.AddOption("-f", MobileDownloadFormats.For(quality));
                    request.AddOption("--no-playlist");
                    using var callback = new DownloadProgressAdapter(onProgress);
                    using var result = YtDlp.Execute(request, callback);
                    return result.ExitCode;
                }, cancellationToken);

                if (exitCode != 0)
                {
                    throw new InvalidOperationException("yt-dlp could not download this media.");
                }

                var mediaFiles = Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
                    .Where(p => MediaExtensions.Contains(Path.GetExtension(p)))
                    .ToArray();
                if (mediaFiles.Length == 0)
                {
                    throw new InvalidOperationException("No playable media file was produced.");
                }

                var exported = new List<string>();
                foreach (var path in mediaFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    exported.Add(await PublishToDownloadsAsync(context, path, cancellationToken));
                }

                Directory.Delete(folder, recursive: true);
                return new MobileDownloadResult(exported);
            }
            catch
            {
                // Retain app-private files if publication fails; never delete data
                // after an incomplete media export. Future UI can offer recovery.
                throw;
            }
        }
        finally
        {
            Gate.Release();
        }
    }

    /// <summary>
    /// Android 10+ scoped storage: publish only the finished media file, without
    /// granting whole-device filesystem access.
    /// </summary>
    private static async Task<string> PublishToDownloadsAsync(
        Context context, string sourcePath, CancellationToken cancellationToken)
    {
        var name = Path.GetFileName(sourcePath);
        var mime = Path.GetExtension(name).ToLowerInvariant() switch
        {
            ".mp3" => "audio/mpeg",
            ".m4a" => "audio/mp4",
            ".flac" => "audio/flac",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".opus" => "audio/ogg",
            ".webm" => "video/webm",
            ".mkv" => "video/x-matroska",
            ".mov" => "video/quicktime",
            ".3gp" => "video/3gpp",
            _ => "video/mp4"
        };

        var resolver = context.ContentResolver
            ?? throw new InvalidOperationException("MediaStore is unavailable.");
        using var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, name);
        values.Put(MediaStore.IMediaColumns.MimeType, mime);
        values.Put(MediaStore.IMediaColumns.RelativePath, global::Android.OS.Environment.DirectoryDownloads + "/Afrowave");
        values.Put(MediaStore.IMediaColumns.IsPending, 1);

        var destination = resolver.Insert(MediaStore.Downloads.ExternalContentUri!, values)
            ?? throw new IOException("Android could not create a Downloads entry.");

        try
        {
            await using var source = File.OpenRead(sourcePath);
            using var output = resolver.OpenOutputStream(destination)
                ?? throw new IOException("Android could not open its Downloads destination.");
            await source.CopyToAsync(output, cancellationToken);
            await output.FlushAsync(cancellationToken);

            using var published = new ContentValues();
            published.Put(MediaStore.IMediaColumns.IsPending, 0);
            resolver.Update(destination, published, null, null);
            return name;
        }
        catch
        {
            resolver.Delete(destination, null, null);
            throw;
        }
    }

    /// <summary>Bridges Java's yt-dlp progress callbacks to a managed action.</summary>
    private sealed class DownloadProgressAdapter(Action<float>? report)
        : Java.Lang.Object, IDownloadProgressCallback
    {
        /// <inheritdoc />
        public void OnProgressUpdate(float progress, long etaInSeconds, string? line)
        {
            if (!float.IsNaN(progress) && !float.IsInfinity(progress))
            {
                report?.Invoke(Math.Clamp(progress, 0, 100));
            }
        }
    }
}
