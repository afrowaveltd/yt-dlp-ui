using System.Text.Json;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

/// <summary>Raised when a link cannot be analysed; carries a friendly, user-facing message.</summary>
public sealed class AnalysisException : Exception
{
    public AnalysisException(string message, string? details = null)
        : base(message) => Details = details;

    /// <summary>Raw yt-dlp output, useful for the log pane.</summary>
    public string? Details { get; }
}

/// <summary>Runs <c>yt-dlp --dump-single-json</c> and maps the result onto <see cref="MediaInfo"/>.</summary>
public sealed class MetadataService
{
    /// <summary>Analyse a link and return everything worth showing before downloading.</summary>
    public async Task<MediaInfo> AnalyzeAsync(
        YtDlpCommand command,
        DownloadOptions options,
        string url,
        bool flatPlaylist = true,
        CancellationToken cancellationToken = default)
    {
        var arguments = YtDlpArgumentBuilder.BuildAnalysisArguments(options, url, flatPlaylist);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));

        ProcessResult result;
        try
        {
            result = await ProcessRunner
                .RunAsync(command.FileName, command.With(arguments), timeout.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AnalysisException("Analýza odkazu trvala příliš dlouho a byla přerušena.");
        }
        catch (Exception ex)
        {
            throw new AnalysisException($"yt-dlp se nepodařilo spustit: {ex.Message}");
        }

        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            throw new AnalysisException(BuildFriendlyError(result.StandardError), result.StandardError);
        }

        try
        {
            return Parse(result.StandardOutput);
        }
        catch (JsonException ex)
        {
            throw new AnalysisException($"Odpověď yt-dlp nebyla čitelná: {ex.Message}", result.StandardOutput);
        }
    }

    /// <summary>Map yt-dlp's JSON payload onto <see cref="MediaInfo"/>.</summary>
    public static MediaInfo Parse(string json)
    {
        using var document = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        });

        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            // Older behaviour occasionally returns one object per entry.
            var entries = root.EnumerateArray().Select((element, index) => ToEntry(element, index + 1)).ToList();
            return new MediaInfo
            {
                Title = $"Seznam ({entries.Count} položek)",
                IsPlaylist = true,
                PlaylistCount = entries.Count,
                Entries = entries,
                TotalDuration = entries.Sum(e => e.Duration ?? 0),
            };
        }

        var type = GetString(root, "_type");
        if (string.Equals(type, "playlist", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(type, "multi_video", StringComparison.OrdinalIgnoreCase))
        {
            return ToPlaylist(root);
        }

        return ToVideo(root);
    }

    private static MediaInfo ToVideo(JsonElement root)
    {
        var formats = new List<MediaFormat>();
        if (root.TryGetProperty("formats", out var formatsElement) && formatsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var format in formatsElement.EnumerateArray())
            {
                if (format.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                formats.Add(new MediaFormat
                {
                    FormatId = GetString(format, "format_id"),
                    Extension = GetString(format, "ext"),
                    Resolution = GetString(format, "resolution"),
                    Fps = GetDouble(format, "fps"),
                    VideoCodec = GetString(format, "vcodec"),
                    AudioCodec = GetString(format, "acodec"),
                    FileSize = GetLong(format, "filesize"),
                    FileSizeApprox = GetLong(format, "filesize_approx"),
                    TotalBitrate = GetDouble(format, "tbr"),
                    AudioBitrate = GetDouble(format, "abr"),
                    Note = GetString(format, "format_note"),
                    Format = GetString(format, "format"),
                    Protocol = GetString(format, "protocol"),
                    AudioChannels = (int?)GetLong(format, "audio_channels"),
                });
            }
        }

        var (subtitles, automatic) = ReadSubtitleLanguages(root);

        return new MediaInfo
        {
            Id = GetString(root, "id"),
            Title = GetString(root, "title"),
            FullTitle = GetString(root, "fulltitle"),
            Uploader = GetString(root, "uploader") ?? GetString(root, "channel"),
            Channel = GetString(root, "channel"),
            Extractor = GetString(root, "extractor_key") ?? GetString(root, "extractor"),
            WebPageUrl = GetString(root, "webpage_url") ?? GetString(root, "original_url"),
            ThumbnailUrl = GetString(root, "thumbnail"),
            Description = GetString(root, "description"),
            UploadDate = GetString(root, "upload_date"),
            License = GetString(root, "license"),
            Duration = GetDouble(root, "duration"),
            ViewCount = GetLong(root, "view_count"),
            LikeCount = GetLong(root, "like_count"),
            CommentCount = GetLong(root, "comment_count"),
            AverageRating = GetDouble(root, "average_rating"),
            IsLive = GetBool(root, "is_live"),
            LiveStatus = GetString(root, "live_status"),
            Formats = formats,
            SubtitleLanguages = subtitles,
            AutomaticSubtitleLanguages = automatic,
        };
    }

    private static MediaInfo ToPlaylist(JsonElement root)
    {
        var entries = new List<MediaEntry>();
        var totalDuration = 0d;

        if (root.TryGetProperty("entries", out var entriesElement) && entriesElement.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var entry in entriesElement.EnumerateArray())
            {
                index++;
                if (entry.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                // --flat-playlist can yield a string URL instead of an object.
                if (entry.ValueKind == JsonValueKind.String)
                {
                    entries.Add(new MediaEntry { Index = index, Url = entry.GetString(), Title = entry.GetString() });
                    continue;
                }

                var mapped = ToEntry(entry, index);
                entries.Add(mapped);
                totalDuration += mapped.Duration ?? 0;
            }
        }

        var (subtitles, automatic) = ReadSubtitleLanguages(root);

        return new MediaInfo
        {
            Id = GetString(root, "id"),
            Title = GetString(root, "title"),
            FullTitle = GetString(root, "title"),
            Uploader = GetString(root, "uploader") ?? GetString(root, "channel"),
            Channel = GetString(root, "channel"),
            Extractor = GetString(root, "extractor_key"),
            WebPageUrl = GetString(root, "webpage_url") ?? GetString(root, "original_url"),
            ThumbnailUrl = GetString(root, "thumbnail"),
            Description = GetString(root, "description"),
            IsPlaylist = true,
            PlaylistCount = (int?)(GetLong(root, "playlist_count") ?? entries.Count),
            Entries = entries,
            TotalDuration = totalDuration > 0 ? totalDuration : GetDouble(root, "duration"),
            SubtitleLanguages = subtitles,
            AutomaticSubtitleLanguages = automatic,
        };
    }

    private static MediaEntry ToEntry(JsonElement element, int index) => new()
    {
        Index = index,
        Id = GetString(element, "id"),
        Title = GetString(element, "title"),
        Url = GetString(element, "url") ?? GetString(element, "webpage_url"),
        Duration = GetDouble(element, "duration"),
        Uploader = GetString(element, "uploader") ?? GetString(element, "channel"),
    };

    private static (IReadOnlyList<string> Subtitle, IReadOnlyList<string> Automatic) ReadSubtitleLanguages(JsonElement root)
    {
        var subtitles = ReadKeys(root, "subtitles");
        var automatic = ReadKeys(root, "automatic_captions");
        return (subtitles, automatic);
    }

    private static IReadOnlyList<string> ReadKeys(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<string>();
        }

        return element.EnumerateObject().Select(p => p.Name).OrderBy(name => name, StringComparer.Ordinal).ToList();
    }

    private static string BuildFriendlyError(string stderr)
    {
        if (string.IsNullOrWhiteSpace(stderr))
        {
            return "Odkaz se nepodařilo analyzovat.";
        }

        var errorLine = stderr
            .Split('\n')
            .Select(line => line.Trim())
            .LastOrDefault(line => line.StartsWith("ERROR:", StringComparison.Ordinal));

        if (errorLine is null)
        {
            return "Odkaz se nepodařilo analyzovat. Zkontrolujte prosím připojení k internetu.";
        }

        var message = errorLine["ERROR:".Length..].Trim();

        if (message.Contains("Unsupported URL", StringComparison.OrdinalIgnoreCase))
        {
            return "Tento odkaz yt-dlp nepodporuje. Zkontrolujte, zda je adresa úplná.";
        }

        if (message.Contains("Video unavailable", StringComparison.OrdinalIgnoreCase))
        {
            return "Video není dostupné (mohlo být odstraněno nebo je omezené pro vaši zemi).";
        }

        if (message.Contains("Sign in", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("login", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("cookies", StringComparison.OrdinalIgnoreCase))
        {
            return "Video vyžaduje přihlášení. Zkuste v nastavení povolit načtení cookies z prohlížeče.";
        }

        if (message.Contains("getaddrinfo", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Failed to resolve", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Connection", StringComparison.OrdinalIgnoreCase))
        {
            return "Nepodařilo se připojit k internetu nebo k danému serveru.";
        }

        return message;
    }

    private static string? GetString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null,
        };
    }

    private static double? GetDouble(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDouble(out var number) => number,
            JsonValueKind.String when double.TryParse(
                value.GetString(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed) => parsed,
            _ => null,
        };
    }

    private static long? GetLong(JsonElement element, string property)
    {
        var asDouble = GetDouble(element, property);
        return asDouble is null ? null : (long)asDouble.Value;
    }

    private static bool? GetBool(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        };
    }
}
