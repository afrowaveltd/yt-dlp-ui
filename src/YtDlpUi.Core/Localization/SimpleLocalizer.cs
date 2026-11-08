using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace YtDlpUi.Core.Localization;

/// <summary>
/// Tiny dictionaryless JSON localization service. English sentences are the keys.
/// No format strings, hidden network calls, or dependencies on the UI framework.
/// </summary>
public sealed class SimpleLocalizer
{
    private readonly Dictionary<string, LocalizedText> _texts = new(StringComparer.Ordinal);
    private Dictionary<string, string> _translations = new(StringComparer.Ordinal);

    /// <summary>Shared, UI-thread-owned service.</summary>
    public static SimpleLocalizer Instance { get; } = new();

    /// <summary>Active language (currently "en" or "cs").</summary>
    public string Language { get; private set; } = "en";

    /// <summary>Cached, observable translation for a text key.</summary>
    public LocalizedText For(string english)
    {
        if (!_texts.TryGetValue(english, out var entry))
        {
            entry = new LocalizedText(english, Translate(english));
            _texts.Add(english, entry);
        }

        return entry;
    }

    /// <summary>Translate using a strict English fallback.</summary>
    public string Translate(string? english) =>
        english is null ? string.Empty :
        _translations.TryGetValue(english, out var translated) && !string.IsNullOrEmpty(translated)
            ? translated
            : english;

    /// <summary>
    /// Select a language; "system" uses the OS UI culture. Unknown languages fall back
    /// to English. Call from the UI thread so PropertyChanged handlers can update views.
    /// </summary>
    public void SetLanguage(string? language)
    {
        var requested = string.IsNullOrWhiteSpace(language) ||
                        string.Equals(language, "system", StringComparison.OrdinalIgnoreCase)
            ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            : language.Split('-', '_')[0];

        Language = string.Equals(requested, "cs", StringComparison.OrdinalIgnoreCase) ? "cs" : "en";
        _translations = LoadDictionary(Language);

        foreach (var (key, value) in _texts)
        {
            value.SetValue(Translate(key));
        }
    }

    private static Dictionary<string, string> LoadDictionary(string language)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var assembly = typeof(SimpleLocalizer).Assembly;
        var name = $"YtDlpUi.Core.Locales.{language}.json";
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream is null)
        {
            return result;
        }

        using var json = JsonDocument.Parse(stream);
        if (json.RootElement.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var item in json.RootElement.EnumerateObject())
        {
            if (item.Value.ValueKind == JsonValueKind.String)
            {
                result[item.Name] = item.Value.GetString() ?? string.Empty;
            }
        }

        return result;
    }
}

/// <summary>Observable translation string, updated when the language changes.</summary>
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
