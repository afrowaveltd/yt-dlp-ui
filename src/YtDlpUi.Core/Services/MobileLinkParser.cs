using System.Text.RegularExpressions;

namespace YtDlpUi.Core.Services;

/// <summary>
/// Pure mobile/desktop link validation shared by all frontends.
/// The initial Android downloader only accepts direct HTTPS media files;
/// video-page URLs need a separate yt-dlp-compatible Android backend.
/// </summary>
public static partial class MobileLinkParser
{
    private static readonly HashSet<string> DirectExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".m4v", ".webm", ".mkv", ".mov", ".mp3",
        ".m4a", ".aac", ".flac", ".ogg", ".opus", ".wav", ".3gp"
    };

    /// <summary>Extract the first HTTP(S) URL from a copied/share-sheet message.</summary>
    public static bool TryExtract(string? text, out string url)
    {
        url = string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (Match match in UrlPattern().Matches(text))
        {
            var value = match.Value.TrimEnd('.', ',', ';', '!', ')', ']', '}', '>');
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) &&
                !string.IsNullOrWhiteSpace(uri.Host))
            {
                url = uri.AbsoluteUri;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validate a direct HTTPS media file. Does not pretend that web pages
    /// such as /watch?v=... can be handled by Android DownloadManager.
    /// </summary>
    public static bool TryGetDirectMedia(string? text, out Uri? uri, out string fileName)
    {
        uri = null;
        fileName = string.Empty;
        if (!TryExtract(text, out var url) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var parsed) ||
            parsed.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        var name = Uri.UnescapeDataString(Path.GetFileName(parsed.AbsolutePath));
        var extension = Path.GetExtension(name);
        if (!DirectExtensions.Contains(extension))
        {
            return false;
        }

        var safe = new string(name.Where(character =>
            char.IsLetterOrDigit(character) || character is '.' or '-' or '_' or ' ').ToArray()).Trim();
        if (safe.Length == 0)
        {
            return false;
        }

        // Avoid duplicate public Downloads names and excessive path lengths.
        safe = safe.Length > 90 ? safe[^90..] : safe;
        fileName = "afrowave-" + DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + safe;
        uri = parsed;
        return true;
    }

    [GeneratedRegex(@"https?://[^\s<>""']+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UrlPattern();
}
