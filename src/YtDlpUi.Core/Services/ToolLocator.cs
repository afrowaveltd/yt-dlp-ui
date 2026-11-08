using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YtDlpUi.Core.Services;

/// <summary>A resolved, runnable yt-dlp command.</summary>
public sealed class YtDlpCommand
{
    /// <summary>Executable to launch (absolute path when known).</summary>
    public required string FileName { get; init; }

    /// <summary>Arguments that must precede the yt-dlp arguments (used for <c>python -m yt_dlp</c>).</summary>
    public IReadOnlyList<string> PrefixArguments { get; init; } = Array.Empty<string>();

    /// <summary>Human readable location shown in the UI.</summary>
    public required string DisplayPath { get; init; }

    /// <summary>Version string reported by <c>--version</c>.</summary>
    public string? Version { get; init; }

    /// <summary>True when yt-dlp was started through the Python module instead of a standalone binary.</summary>
    public bool IsPythonModule => PrefixArguments.Count > 0;

    /// <summary>Full argument vector for the given yt-dlp arguments.</summary>
    public List<string> With(IEnumerable<string> arguments)
    {
        var combined = new List<string>(PrefixArguments);
        combined.AddRange(arguments);
        return combined;
    }

    /// <summary>Copy-pasteable command for the given argument vector.</summary>
    public string Preview(IEnumerable<string> arguments) => CommandLine.Format(FileName, With(arguments));
}

/// <summary>Result of probing for an external tool.</summary>
public sealed record ToolStatus(string Name, bool Found, string? Path, string? Version, string? Message)
{
    /// <summary>Convenience factory for a missing tool.</summary>
    public static ToolStatus Missing(string name, string message) => new(name, false, null, null, message);

    /// <summary>Convenience factory for an available tool.</summary>
    public static ToolStatus Ok(string name, string path, string? version) => new(name, true, path, version, null);
}

/// <summary>Finds yt-dlp and ffmpeg on the current machine.</summary>
public static class ToolLocator
{
    /// <summary>Locate yt-dlp, honouring an explicit override path first.</summary>
    public static async Task<ToolStatus> LocateYtDlpAsync(string? overridePath, CancellationToken cancellationToken = default)
    {
        foreach (var command in EnumerateYtDlpCandidates(overridePath))
        {
            if (!IsLaunchable(command.FileName))
            {
                continue;
            }

            var version = await QueryVersionAsync(command, cancellationToken).ConfigureAwait(false);
            if (version is not null)
            {
                return ToolStatus.Ok("yt-dlp", command.DisplayPath, version);
            }
        }

        return ToolStatus.Missing("yt-dlp", "yt-dlp nebyl v systému nalezen.");
    }

    /// <summary>Locate ffmpeg, honouring an explicit override path first.</summary>
    public static async Task<ToolStatus> LocateFfmpegAsync(string? overridePath, CancellationToken cancellationToken = default)
    {
        foreach (var candidate in EnumerateFfmpegCandidates(overridePath))
        {
            if (!IsLaunchable(candidate.FileName))
            {
                continue;
            }

            var version = await QueryFfmpegVersionAsync(candidate.FileName, cancellationToken).ConfigureAwait(false);
            if (version is not null)
            {
                return ToolStatus.Ok("ffmpeg", candidate.DisplayPath, version);
            }
        }

        return ToolStatus.Missing("ffmpeg", "ffmpeg nebyl v systému nalezen.");
    }

    /// <summary>Resolve a command that is already known to work.</summary>
    public static async Task<YtDlpCommand?> ResolveYtDlpCommandAsync(string? overridePath, CancellationToken cancellationToken = default)
    {
        foreach (var command in EnumerateYtDlpCandidates(overridePath))
        {
            if (!IsLaunchable(command.FileName))
            {
                continue;
            }

            var version = await QueryVersionAsync(command, cancellationToken).ConfigureAwait(false);
            if (version is not null)
            {
                return new YtDlpCommand
                {
                    FileName = command.FileName,
                    PrefixArguments = command.PrefixArguments,
                    DisplayPath = command.DisplayPath,
                    Version = version,
                };
            }
        }

        return null;
    }

    /// <summary>Every place worth looking for a yt-dlp binary, in priority order.</summary>
    public static IEnumerable<YtDlpCommand> EnumerateYtDlpCandidates(string? overridePath)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            var expanded = ExpandHome(overridePath!.Trim());
            var resolved = File.Exists(expanded) ? Path.GetFullPath(expanded) : FindOnPath(expanded);
            if (resolved is not null && seen.Add(resolved))
            {
                yield return new YtDlpCommand { FileName = resolved, DisplayPath = resolved };
            }
        }

        foreach (var name in BinaryNames())
        {
            var onPath = FindOnPath(name);
            if (onPath is not null && seen.Add(onPath))
            {
                yield return new YtDlpCommand { FileName = onPath, DisplayPath = onPath };
            }
        }

        foreach (var directory in SearchDirectories())
        {
            foreach (var name in BinaryNames())
            {
                var candidate = Path.Combine(directory, name);
                if (File.Exists(candidate))
                {
                    var full = Path.GetFullPath(candidate);
                    if (seen.Add(full))
                    {
                        yield return new YtDlpCommand { FileName = full, DisplayPath = full };
                    }
                }
            }
        }

        // Last resort: an installed Python module without a console script on PATH.
        foreach (var python in PythonNames())
        {
            var interpreter = FindOnPath(python);
            if (interpreter is not null && seen.Add("py:" + python))
            {
                yield return new YtDlpCommand
                {
                    FileName = interpreter,
                    PrefixArguments = ["-m", "yt_dlp"],
                    DisplayPath = interpreter + " -m yt_dlp",
                };
            }
        }
    }

    /// <summary>Locate an executable by name using the PATH environment variable.</summary>
    public static string? FindOnPath(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (name.Contains(Path.DirectorySeparatorChar) || name.Contains('/'))
        {
            return File.Exists(name) ? Path.GetFullPath(name) : null;
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var name2 in WindowsSuffixes(name))
            {
                var candidate = Path.Combine(directory, name2);
                if (File.Exists(candidate) && IsExecutable(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }
        }

        return null;
    }

    /// <summary>Expand a leading <c>~</c> to the user's home directory.</summary>
    public static string ExpandHome(string path)
    {
        if (path is "~" or "~/")
        {
            return UserPaths.Home;
        }

        if (path.StartsWith("~/", StringComparison.Ordinal))
        {
            return Path.Combine(UserPaths.Home, path[2..]);
        }

        return path;
    }

    private static IEnumerable<ToolCandidate> EnumerateFfmpegCandidates(string? overridePath)
    {
        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            var expanded = ExpandHome(overridePath!.Trim());
            if (Directory.Exists(expanded))
            {
                var inDirectory = Path.Combine(expanded, "ffmpeg" + WindowsExecutableSuffix());
                if (File.Exists(inDirectory))
                {
                    yield return new ToolCandidate(inDirectory, inDirectory);
                }
            }
            else
            {
                var resolved = File.Exists(expanded) ? Path.GetFullPath(expanded) : FindOnPath(expanded);
                if (resolved is not null)
                {
                    yield return new ToolCandidate(resolved, resolved);
                }
            }
        }

        var onPath = FindOnPath("ffmpeg");
        if (onPath is not null)
        {
            yield return new ToolCandidate(onPath, onPath);
        }

        if (OperatingSystem.IsWindows())
        {
            var managed = Path.Combine(UserPaths.ManagedFfmpegDirectory, "ffmpeg.exe");
            if (File.Exists(managed))
            {
                yield return new ToolCandidate(managed, managed);
            }
            var shared = Path.Combine(UserPaths.MachineFfmpegDirectory, "ffmpeg.exe");
            if (File.Exists(shared))
            {
                yield return new ToolCandidate(shared, shared);
            }
        }

        foreach (var directory in SearchDirectories())
        {
            var candidate = Path.Combine(directory, "ffmpeg" + WindowsExecutableSuffix());
            if (File.Exists(candidate))
            {
                yield return new ToolCandidate(candidate, candidate);
            }
        }
    }

    private static IEnumerable<string> BinaryNames()
    {
        yield return "yt-dlp";
        yield return "yt-dlp.exe";
    }

    private static IEnumerable<string> WindowsSuffixes(string name)
    {
        yield return name;
        if (OperatingSystem.IsWindows() && !name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            yield return name + ".exe";
        }
    }

    private static string WindowsExecutableSuffix() => OperatingSystem.IsWindows() ? ".exe" : string.Empty;

    private static IEnumerable<string> PythonNames()
    {
        yield return "python3";
        yield return "python";
    }

    private static IEnumerable<string> SearchDirectories()
    {
        yield return UserPaths.LocalBinDirectory;
        if (OperatingSystem.IsWindows())
        {
            // Admin-installed standalone tools are shared with non-admin accounts.
            yield return UserPaths.MachineToolsDirectory;
        }
        yield return Path.Combine(UserPaths.Home, "bin");
        yield return Path.Combine(UserPaths.Home, ".local", "pipx", "venvs", "yt-dlp", "bin");

        if (OperatingSystem.IsWindows())
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            yield return Path.Combine(appData, "yt-dlp");
            yield return Path.Combine(localAppData, "Programs", "yt-dlp");
            yield return Path.Combine(localAppData, "Microsoft", "WinGet", "Links");
        }
        else
        {
            yield return "/usr/local/bin";
            yield return "/usr/bin";
            yield return "/opt/homebrew/bin";
            yield return "/snap/bin";
            yield return "/var/lib/flatpak/exports/bin";
        }
    }

    private static bool IsLaunchable(string fileName)
    {
        return File.Exists(fileName) && IsExecutable(fileName);
    }

    /// <summary>True when the file can be executed by the current user.</summary>
    public static bool IsExecutable(string path)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return File.Exists(path);
            }

            var mode = File.GetUnixFileMode(path);
            const UnixFileMode anyExecute = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            return (mode & anyExecute) != 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<string?> QueryVersionAsync(YtDlpCommand command, CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(25));

            var result = await ProcessRunner
                .RunAsync(command.FileName, command.With(["--version"]), timeout.Token)
                .ConfigureAwait(false);

            if (result.ExitCode != 0)
            {
                return null;
            }

            var version = result.StandardOutput.Trim();
            return version.Length == 0 ? null : version.Split('\n')[0].Trim();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<string?> QueryFfmpegVersionAsync(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(20));

            var result = await ProcessRunner.RunAsync(fileName, ["-version"], timeout.Token).ConfigureAwait(false);
            if (result.ExitCode != 0)
            {
                return null;
            }

            var first = result.StandardOutput.Split('\n').FirstOrDefault()?.Trim();
            return string.IsNullOrWhiteSpace(first) ? "ffmpeg" : first;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private sealed record ToolCandidate(string FileName, string DisplayPath);
}
