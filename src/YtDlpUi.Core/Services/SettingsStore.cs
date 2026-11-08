using System.Text.Json;
using System.Text.Json.Serialization;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

/// <summary>Loads and saves <see cref="AppSettings"/> plus the last-used download options.</summary>
public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _path;

    public SettingsStore(string? path = null) => _path = path ?? UserPaths.SettingsFile;

    /// <summary>Full path of the settings file in use.</summary>
    public string Path => _path;

    /// <summary>Read settings from disk; never throws.</summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return CreateDefault();
            }

            var json = File.ReadAllText(_path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? CreateDefault();
            if (string.IsNullOrWhiteSpace(settings.DefaultOutputDirectory))
            {
                settings.DefaultOutputDirectory = UserPaths.DefaultDownloadDirectory;
            }

            return settings;
        }
        catch (Exception)
        {
            return CreateDefault();
        }
    }

    /// <summary>Write settings to disk; never throws.</summary>
    public bool Save(AppSettings settings)
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(settings, JsonOptions);
            // Write a complete new copy first, then atomically rename it on the
            // same volume. A crash or failed write must not truncate settings.json.
            var tempPath = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, _path, overwrite: true);
                return true;
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Deserialize the remembered download options, if any.</summary>
    public static DownloadOptions? RestoreOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DownloadOptions>(json!, JsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Serialize download options, stripping anything that looks like a secret.</summary>
    public static string StoreOptions(DownloadOptions options)
    {
        try
        {
            var scrub = JsonSerializer.SerializeToNode(options, JsonOptions)?.AsObject();
            if (scrub is not null)
            {
                foreach (var secret in AppSettings.SecretOptionNames)
                {
                    if (scrub.ContainsKey(secret))
                    {
                        scrub[secret] = null;
                    }
                }

                return scrub.ToJsonString(JsonOptions);
            }
        }
        catch (Exception)
        {
            // Ignore: remembering options is best effort.
        }

        return string.Empty;
    }

    private static AppSettings CreateDefault() => new()
    {
        DefaultOutputDirectory = UserPaths.DefaultDownloadDirectory,
    };
}
