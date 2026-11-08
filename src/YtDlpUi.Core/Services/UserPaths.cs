namespace YtDlpUi.Core.Services;

/// <summary>XDG-aware locations for configuration, cache and the default download folder.</summary>
public static class UserPaths
{
    private static readonly object Sync = new();
    private static string? _downloadDir;

    /// <summary>Current user's home directory.</summary>
    public static string Home =>
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    /// <summary>Configuration folder for this application (created on demand).</summary>
    public static string ConfigDirectory
    {
        get
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            var root = string.IsNullOrWhiteSpace(xdg)
                ? Path.Combine(Home, ".config")
                : xdg!;
            return Path.Combine(root, "yt-dlp-ui");
        }
    }

    /// <summary>Cache folder for this application (created on demand).</summary>
    public static string CacheDirectory
    {
        get
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
            var root = string.IsNullOrWhiteSpace(xdg)
                ? Path.Combine(Home, ".cache")
                : xdg!;
            return Path.Combine(root, "yt-dlp-ui");
        }
    }

    /// <summary>Path of the persisted settings file.</summary>
    public static string SettingsFile => Path.Combine(ConfigDirectory, "settings.json");

    /// <summary>Folder used when installing a private yt-dlp copy.</summary>
    public static string LocalBinDirectory => Path.Combine(Home, ".local", "bin");

    /// <summary>
    /// Best guess at the user's download folder: honours <c>user-dirs.dirs</c>, then
    /// <c>~/Downloads</c>, then the home folder itself.
    /// </summary>
    public static string DefaultDownloadDirectory
    {
        get
        {
            lock (Sync)
            {
                if (_downloadDir is not null)
                {
                    return _downloadDir;
                }

                var fromXdg = ReadXdgUserDir("XDG_DOWNLOAD_DIR");
                if (fromXdg is not null && Directory.Exists(fromXdg))
                {
                    return _downloadDir = fromXdg;
                }

                var downloads = Path.Combine(Home, "Downloads");
                return _downloadDir = Directory.Exists(downloads) ? downloads : Home;
            }
        }
    }

    /// <summary>Ensure a directory exists and return it.</summary>
    public static string EnsureDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception)
        {
            // Non-fatal: callers fall back to reporting the failure.
        }

        return path;
    }

    private static string? ReadXdgUserDir(string key)
    {
        try
        {
            var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            var root = string.IsNullOrWhiteSpace(configHome)
                ? Path.Combine(Home, ".config")
                : configHome!;
            var file = Path.Combine(root, "user-dirs.dirs");
            if (!File.Exists(file))
            {
                return null;
            }

            foreach (var raw in File.ReadAllLines(file))
            {
                var line = raw.Trim();
                if (!line.StartsWith(key + "=", StringComparison.Ordinal))
                {
                    continue;
                }

                var value = line[(key.Length + 1)..].Trim().Trim('"');
                value = value.Replace("$HOME", Home, StringComparison.Ordinal);
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }
        catch (Exception)
        {
            // Ignore malformed user-dirs.dirs.
        }

        return null;
    }
}
