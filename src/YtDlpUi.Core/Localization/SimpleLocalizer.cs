using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Localization;

/// <summary>Language discovered from a JSON dictionary (for example cs or pt-BR).</summary>
public sealed record LanguageOption(string Code, string DisplayName);

/// <summary>
/// Minimal dictionaryless localizer: English sentences are the keys and the fallback.
/// Discovers embedded and on-disk Locales/*.json without hard-coded language choices.
/// </summary>
public sealed class SimpleLocalizer
{
    private const string Prefix = "YtDlpUi.Core.Locales.";
    private const string Suffix = ".json";
    private readonly Dictionary<string, LocalizedText> _texts = new(StringComparer.Ordinal);
    private readonly string _userLocaleDirectory;
    private Dictionary<string, Dictionary<string, string>> _catalog = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _translations = new(StringComparer.Ordinal);
    private IReadOnlyList<LanguageOption> _availableLanguages = Array.Empty<LanguageOption>();

    /// <summary>Shared localizer; use from the UI thread.</summary>
    public static SimpleLocalizer Instance { get; } = new();

    /// <summary>
    /// The optional directory overrides the standard user configuration Locales folder.
    /// Useful for tests and user-defined languages without rebuilding the application.
    /// </summary>
    public SimpleLocalizer(string? userLocaleDirectory = null)
    {
        _userLocaleDirectory = userLocaleDirectory ?? Path.Combine(UserPaths.ConfigDirectory, "Locales");
        RefreshLanguages();
    }

    /// <summary>Currently selected language code.</summary>
    public string Language { get; private set; } = "en";

    /// <summary>Valid languages available in embedded and filesystem dictionaries.</summary>
    public IReadOnlyList<LanguageOption> AvailableLanguages => _availableLanguages;

    /// <summary>Cached, observable translation for an English key.</summary>
    public LocalizedText For(string english)
    {
        if (!_texts.TryGetValue(english, out var entry))
        {
            entry = new LocalizedText(english, Translate(english));
            _texts.Add(english, entry);
        }

        return entry;
    }

    /// <summary>Missing or empty translations fall back to the English key.</summary>
    public string Translate(string? english) =>
        english is null ? string.Empty :
        _translations.TryGetValue(english, out var translated) && !string.IsNullOrEmpty(translated)
            ? translated
            : english;

    /// <summary>
    /// Scan bundled resources, application-local Locales, and the user's Locales folder.
    /// Later valid dictionaries override earlier ones; malformed files are ignored.
    /// Call from the UI thread, including when reopening the language selector.
    /// </summary>
    public void RefreshLanguages()
    {
        var catalog = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(SimpleLocalizer).Assembly;
        foreach (var resource in assembly.GetManifestResourceNames())
        {
            if (!resource.StartsWith(Prefix, StringComparison.Ordinal) ||
                !resource.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase) ||
                !TryCulture(resource[Prefix.Length..^Suffix.Length], out var code))
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(resource);
            if (stream is not null && TryRead(stream, out var values))
            {
                catalog[code] = values;
            }
        }

        AddFiles(catalog, Path.Combine(AppContext.BaseDirectory, "Locales"));
        AddFiles(catalog, _userLocaleDirectory);
        catalog.TryAdd("en", new Dictionary<string, string>(StringComparer.Ordinal));
        _catalog = catalog;
        _availableLanguages = catalog.Keys.Select(code =>
        {
            var culture = CultureInfo.GetCultureInfo(code);
            var name = culture.NativeName;
            var nativeName = string.IsNullOrEmpty(name) ? code : char.ToUpper(name[0], culture) + name[1..];
            return new LanguageOption(code, $"{nativeName} ({code})");
        }).OrderBy(item => item.Code == "en" ? 0 : 1)
          .ThenBy(item => item.DisplayName, StringComparer.CurrentCultureIgnoreCase)
          .ToArray();

        SetLanguage(Language);
    }

    /// <summary>
    /// Resolve full culture first (pt-BR), then parent (pt); use OS language when
    /// the setting is "system". Unknown languages fall back to English.
    /// </summary>
    public void SetLanguage(string? language)
    {
        var requested = string.IsNullOrWhiteSpace(language) ||
                        string.Equals(language, "system", StringComparison.OrdinalIgnoreCase)
            ? CultureInfo.CurrentUICulture.Name
            : language.Trim();

        while (!string.IsNullOrEmpty(requested))
        {
            if (_catalog.TryGetValue(requested, out var dictionary))
            {
                Language = _catalog.Keys.First(code =>
                    string.Equals(code, requested, StringComparison.OrdinalIgnoreCase));
                _translations = Language.Equals("en", StringComparison.OrdinalIgnoreCase)
                    ? new Dictionary<string, string>(StringComparer.Ordinal)
                    : dictionary;
                RefreshBindings();
                return;
            }

            var separator = requested.LastIndexOf('-');
            if (separator < 0)
            {
                break;
            }

            requested = requested[..separator];
        }

        Language = "en";
        _translations = new Dictionary<string, string>(StringComparer.Ordinal);
        RefreshBindings();
    }

    private void RefreshBindings()
    {
        foreach (var (key, text) in _texts)
        {
            text.SetValue(Translate(key));
        }
    }

    private static void AddFiles(
        Dictionary<string, Dictionary<string, string>> catalog, string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly))
            {
                if (!TryCulture(Path.GetFileNameWithoutExtension(file), out var code))
                {
                    continue;
                }

                try
                {
                    if (new FileInfo(file).Length > 4 * 1024 * 1024)
                    {
                        continue;
                    }

                    using var stream = File.OpenRead(file);
                    if (TryRead(stream, out var values))
                    {
                        catalog[code] = values;
                    }
                }
                catch (Exception)
                {
                    // Optional external file may be malformed, missing or inaccessible.
                }
            }
        }
        catch (Exception)
        {
            // An inaccessible external directory must not prevent startup.
        }
    }

    private static bool TryCulture(string name, out string code)
    {
        code = string.Empty;
        if (string.IsNullOrEmpty(name) || name.Length > 32 ||
            name.Any(character => !(char.IsAsciiLetterOrDigit(character) || character == '-')))
        {
            return false;
        }

        try
        {
            code = CultureInfo.GetCultureInfo(name).Name;
            return !string.IsNullOrEmpty(code);
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    private static bool TryRead(Stream stream, out Dictionary<string, string> values)
    {
        values = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    values[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

/// <summary>Observable translation, changed in-place when language switches.</summary>
public sealed class LocalizedText : INotifyPropertyChanged
{
    internal LocalizedText(string key, string value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>English fallback key.</summary>
    public string Key { get; }

    /// <summary>Current translation.</summary>
    public string Value { get; private set; }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    internal void SetValue(string value)
    {
        if (Value == value)
        {
            return;
        }

        Value = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
