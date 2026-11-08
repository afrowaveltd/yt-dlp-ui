using System.Diagnostics;
using System.Text;

namespace YtDlpUi.Core.Services;

/// <summary>Captured result of a finished process.</summary>
public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Success => ExitCode == 0;
}

/// <summary>Thin, cancellation-aware wrapper around <see cref="Process"/> for console tools.</summary>
public static class ProcessRunner
{
    /// <summary>Environment variables applied to every child process.</summary>
    private static readonly Dictionary<string, string> ExtraEnvironment = new()
    {
        ["PYTHONUNBUFFERED"] = "1",
        ["PYTHONIOENCODING"] = "utf-8",
    };

    /// <summary>
    /// Run a process and surface its output line by line as it arrives.
    /// Returns the exit code; throws <see cref="OperationCanceledException"/> when cancelled.
    /// </summary>
    public static async Task<int> RunStreamingAsync(
        string executable,
        IReadOnlyList<string> arguments,
        Action<string>? onOutput,
        Action<string>? onError,
        CancellationToken cancellationToken,
        string? workingDirectory = null)
    {
        using var process = StartProcess(executable, arguments, workingDirectory);

        var stdout = PumpAsync(process.StandardOutput, onOutput, cancellationToken);
        var stderr = PumpAsync(process.StandardError, onError, cancellationToken);

        await using (cancellationToken.Register(() => TryKill(process)))
        {
            try
            {
                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                throw;
            }
        }

        // Drain readers so no output is lost when the process exits quickly.
        await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
        return process.ExitCode;
    }

    /// <summary>Run a process and capture its complete output.</summary>
    public static async Task<ProcessResult> RunAsync(
        string executable,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default,
        string? workingDirectory = null)
    {
        using var process = StartProcess(executable, arguments, workingDirectory);

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await using (cancellationToken.Register(() => TryKill(process)))
        {
            try
            {
                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                throw;
            }
        }

        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);
        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    /// <summary>Terminate a process and its children, ignoring races.</summary>
    public static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception)
        {
            // Already gone, or not permitted: nothing useful to do.
        }
    }

    private static Process StartProcess(
        string executable,
        IReadOnlyList<string> arguments,
        string? workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        foreach (var (key, value) in ExtraEnvironment)
        {
            startInfo.Environment[key] = value;
        }

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        if (!process.Start())
        {
            throw new InvalidOperationException($"Nepodařilo se spustit '{executable}'.");
        }

        // yt-dlp never reads stdin, but leaving it open would let it block on a prompt.
        try
        {
            process.StandardInput.Close();
        }
        catch (Exception)
        {
            // Ignore: some platforms do not expose a usable stdin handle.
        }

        return process;
    }

    private static async Task PumpAsync(
        StreamReader reader,
        Action<string>? sink,
        CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (line is null)
                {
                    return;
                }

                sink?.Invoke(line);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation is reported by the caller.
        }
        catch (Exception)
        {
            // A closed pipe just means the process exited.
        }
    }
}
