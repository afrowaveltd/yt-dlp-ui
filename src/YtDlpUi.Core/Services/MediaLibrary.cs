namespace YtDlpUi.Core.Services;

/// <summary>Finds media files that the player and converter can work with.</summary>
public static class MediaLibrary
{
    /// <summary>Video containers recognised by the built-in browsers.</summary>
    public static IReadOnlyCollection<string> VideoExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mkv", ".webm", ".mov", ".avi", ".m4v", ".mpg", ".mpeg", ".wmv", ".flv", ".ts", ".3gp", ".ogv",
    };

    /// <summary>Audio containers recognised by the built-in browsers.</summary>
    public static IReadOnlyCollection<string> AudioExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".m4a", ".aac", ".opus", ".ogg", ".oga", ".flac", ".wav", ".wma", ".aiff", ".aif", ".alac",
    };

    /// <summary>Every extension the media browser lists.</summary>
    public static bool IsSupportedMedia(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var extension = Path.GetExtension(path);
        return VideoExtensions.Contains(extension) || AudioExtensions.Contains(extension);
    }

    /// <summary>True when the file is a video container (has a picture in practice).</summary>
    public static bool IsVideoFile(string? path) =>
        !string.IsNullOrWhiteSpace(path) && VideoExtensions.Contains(Path.GetExtension(path!));

    /// <summary>
    /// List supported media files in a folder, newest first.
    /// Enumeration never throws: an unreadable folder simply yields an empty list.
    /// </summary>
    public static IReadOnlyList<string> Scan(string? directory, int maxFiles = 500)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return Array.Empty<string>();
        }

        var expanded = ToolLocator.ExpandHome(directory!.Trim());
        if (!Directory.Exists(expanded))
        {
            return Array.Empty<string>();
        }

        try
        {
            return Directory
                .EnumerateFiles(expanded)
                .Where(IsSupportedMedia)
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Take(Math.Max(1, maxFiles))
                .Select(file => file.FullName)
                .ToList();
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }
}
