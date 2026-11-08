using System.Text;

namespace YtDlpUi.Core.Services;

/// <summary>Splitting, joining and quoting command lines in a POSIX-shell-compatible way.</summary>
public static class CommandLine
{
    /// <summary>
    /// Split a free-form argument string into tokens. Understands double quotes, single quotes and
    /// backslash escapes so users can type <c>--downloader-args "aria2c:-x 16"</c>.
    /// </summary>
    public static List<string> Split(string? input)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(input))
        {
            return tokens;
        }

        var current = new StringBuilder();
        var hasToken = false;
        var quote = '\0';

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];

            if (quote != '\0')
            {
                if (c == quote)
                {
                    quote = '\0';
                    continue;
                }

                if (c == '\\' && quote == '"' && i + 1 < input.Length && (input[i + 1] == '"' || input[i + 1] == '\\'))
                {
                    current.Append(input[++i]);
                    continue;
                }

                current.Append(c);
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (hasToken)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                    hasToken = false;
                }

                continue;
            }

            if (c is '"' or '\'')
            {
                quote = c;
                hasToken = true;
                continue;
            }

            if (c == '\\' && i + 1 < input.Length)
            {
                current.Append(input[++i]);
                hasToken = true;
                continue;
            }

            current.Append(c);
            hasToken = true;
        }

        if (hasToken)
        {
            tokens.Add(current.ToString());
        }

        return tokens;
    }

    /// <summary>Render an argument list as a copy-pasteable shell command.</summary>
    public static string Format(string executable, IEnumerable<string> arguments)
    {
        var builder = new StringBuilder(Quote(executable));
        foreach (var argument in arguments)
        {
            builder.Append(' ').Append(Quote(argument));
        }

        return builder.ToString();
    }

    /// <summary>Quote a single argument only when it needs it.</summary>
    public static string Quote(string value)
    {
        if (value.Length == 0)
        {
            return "''";
        }

        var needsQuotes = value.Any(c => char.IsWhiteSpace(c) || c is '"' or '\'' or '$' or '`' or '\\'
            or '&' or ';' or '|' or '<' or '>' or '(' or ')' or '[' or ']' or '{' or '}'
            or '*' or '?' or '!' or '#' or '~' or '\u001f' or '%');
        if (!needsQuotes)
        {
            return value;
        }

        return "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";
    }
}
