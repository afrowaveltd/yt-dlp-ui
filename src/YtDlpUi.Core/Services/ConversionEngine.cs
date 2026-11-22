using System.Text;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

/// <summary>Result of one FFmpeg conversion.</summary>
public sealed record ConversionOutcome(
    bool Success,
    bool Cancelled,
    string? OutputPath,
    string? ErrorMessage);

/// <summary>Runs FFmpeg, forwarding progress and the final error text.</summary>
public static class ConversionEngine
{
    /// <summary>How many trailing FFmpeg log lines are kept to explain a failure.</summary>
    private const int ErrorTailLines = 12;

    /// <summary>
    /// Convert <paramref name="options"/> with FFmpeg.
    /// </summary>
    /// <param name="ffmpegPath">Resolved FFmpeg executable.</param>
    /// <param name="options">Conversion configuration.</param>
    /// <param name="sourceDurationSeconds">Duration of the input, used for the percentage.</param>
    /// <param name="onProgress">Called for every completed progress block.</param>
    /// <param name="onLog">Called for FFmpeg log lines (stderr).</param>
    public static async Task<ConversionOutcome> RunAsync(
        string ffmpegPath,
        ConversionOptions options,
        double? sourceDurationSeconds,
        Action<FfmpegProgress>? onProgress = null,
        Action<string>? onLog = null,
        CancellationToken cancellationToken = default)
    {
        var arguments = FfmpegArgumentBuilder.Build(options);
        var outputPath = FfmpegArgumentBuilder.BuildOutputPath(options);
        var accumulator = new FfmpegProgressAccumulator();
        var errorTail = new Queue<string>(ErrorTailLines);

        void HandleStdout(string line)
        {
            if (accumulator.Feed(line) is { } snapshot)
            {
                onProgress?.Invoke(snapshot);
            }
        }

        void HandleStderr(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            if (errorTail.Count == ErrorTailLines)
            {
                errorTail.Dequeue();
            }

            errorTail.Enqueue(line);
            onLog?.Invoke(line);
        }

        int exitCode;
        try
        {
            exitCode = await ProcessRunner
                .RunStreamingAsync(
                    ffmpegPath,
                    arguments,
                    HandleStdout,
                    HandleStderr,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryDeletePartial(outputPath);
            return new ConversionOutcome(false, true, null, null);
        }
        catch (Exception exception)
        {
            return new ConversionOutcome(false, false, null, exception.Message);
        }

        if (exitCode == 0 && File.Exists(outputPath))
        {
            onProgress?.Invoke(new FfmpegProgress(
                sourceDurationSeconds, null, null, new FileInfo(outputPath).Length, null, true));
            return new ConversionOutcome(true, false, outputPath, null);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            TryDeletePartial(outputPath);
            return new ConversionOutcome(false, true, null, null);
        }

        return new ConversionOutcome(false, false, null, Summarize(errorTail));
    }

    /// <summary>Turn the trailing FFmpeg log into one readable sentence.</summary>
    public static string Summarize(IEnumerable<string> lines)
    {
        var builder = new StringBuilder();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("frame=", StringComparison.Ordinal))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(trimmed);
        }

        var text = builder.ToString();
        if (text.Length == 0)
        {
            return "FFmpeg finished with an error.";
        }

        return text.Length <= 500 ? text : text[..500] + "…";
    }

    private static void TryDeletePartial(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception)
        {
            // A leftover partial file is not worth failing the cancellation path over.
        }
    }
}
