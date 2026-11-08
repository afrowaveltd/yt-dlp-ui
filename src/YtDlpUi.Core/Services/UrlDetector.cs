using System.Text.RegularExpressions;

namespace YtDlpUi.Core.Services;

/// <summary>Recognises URLs inside arbitrary clipboard text.</summary>
public static partial class UrlDetector
{
    [GeneratedRegex(@"https?://[^\s<>""]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex SchemeUrl();

    [GeneratedRegex(@"\bwww\.[^\s<>""]+\.[a-z]{2,}[^\s<>""]*", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex BareWwwUrl();

    private static readonly char[] TrailingJunk = ['.', ',', ';', ':', '!', '?', ')', ']', '}', '>', '"', '\'', '»', '…'];

    /// <summary>
    /// Extract the first http(s) URL from <paramref name="text"/>, or null when the text holds none.
    /// Trailing sentence punctuation is trimmed so "see https://x.test/a)." still works.
    /// </summary>
    public static string? ExtractFirstUrl(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var match = SchemeUrl().Match(text);
        if (match.Success)
        {
            return Trim(match.Value);
        }

        var bare = BareWwwUrl().Match(text);
        if (bare.Success)
        {
            return "https://" + Trim(bare.Value);
        }

        return null;
    }

    /// <summary>True when the whole string looks like a single URL.</summary>
    public static bool LooksLikeUrl(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.Trim();
        if (trimmed.Contains('\n') || trimmed.Contains(' '))
        {
            return false;
        }

        return ExtractFirstUrl(trimmed) is { } url
               && string.Equals(url, trimmed, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Split multi-line text into one URL per line, keeping order and dropping blanks.</summary>
    public static IReadOnlyList<string> ExtractAllUrls(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var results = new List<string>();
        foreach (var line in text.Split('\n'))
        {
            var url = ExtractFirstUrl(line);
            if (url is not null && !results.Contains(url, StringComparer.OrdinalIgnoreCase))
            {
                results.Add(url);
            }
        }

        return results;
    }

    private static string Trim(string value)
    {
        var trimmed = value.Trim().TrimEnd(TrailingJunk);
        // Balance parentheses: keep them only when the URL actually opened them.
        while (trimmed.EndsWith(')') && trimmed.Count(c => c == '(') < trimmed.Count(c => c == ')'))
        {
            trimmed = trimmed[..^1];
        }

        return trimmed;
    }
}
