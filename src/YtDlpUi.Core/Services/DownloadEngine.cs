using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

/// <summary>Result of downloading one queue item.</summary>
public sealed record DownloadOutcome(int ExitCode, bool Cancelled, string? FilePath, string? ErrorMessage)
{
    /// <summary>True when the download finished without errors.</summary>
    public bool Success => !Cancelled && ExitCode == 0;
}

/// <summary>Runs one yt-dlp download and translates its console output into events.</summary>
public static class DownloadEngine
{
    /// <summary>
    /// Download <paramref name="url"/> using <paramref name="options"/>.
    /// Progress is reported through <paramref name="onEvent"/> as raw output arrives.
    /// </summary>
    public static async Task<DownloadOutcome> RunAsync(
        YtDlpCommand command,
        DownloadOptions options,
        string url,
        Action<YtDlpEvent>? onEvent,
        CancellationToken cancellationToken = default)
    {
        var arguments = YtDlpArgumentBuilder.Build(options, [url]);

        string? destination = null;
        string? lastError = null;
        var cancelled = false;

        void HandleLine(string line, bool isErrorStream)
        {
            var parsed = ProgressParser.Parse(line);
            switch (parsed)
            {
                case DestinationEvent destinationEvent:
                    destination = destinationEvent.Path;
                    break;
                case LogEvent { Level: YtDlpLogLevel.Error } errorEvent:
                    lastError = errorEvent.Text;
                    break;
                case null when isErrorStream && !string.IsNullOrWhiteSpace(line):
                    lastError = line.Trim();
                    break;
            }

            if (parsed is not null)
            {
                onEvent?.Invoke(parsed);
            }
        }

        int exitCode;
        try
        {
            exitCode = await ProcessRunner
                .RunStreamingAsync(
                    command.FileName,
                    command.With(arguments),
                    line => HandleLine(line, false),
                    line => HandleLine(line, true),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
            exitCode = -1;
        }

        return new DownloadOutcome(
            exitCode,
            cancelled,
            destination,
            lastError is null ? null : CleanupError(lastError));
    }

    /// <summary>Strip yt-dlp's <c>ERROR:</c> prefix so the UI can show plain text.</summary>
    public static string CleanupError(string raw)
    {
        var text = raw.Trim();
        return text.StartsWith("ERROR:", StringComparison.Ordinal)
            ? text["ERROR:".Length..].Trim()
            : text;
    }
}
