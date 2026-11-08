namespace YtDlpUi.Core.Services;

/// <summary>
/// Finds a local Deno executable for YouTube's yt-dlp EJS challenge solver.
/// Deno is enabled by default in yt-dlp, but installations outside PATH
/// require an explicit --js-runtimes deno:ABSOLUTE_PATH argument.
/// This class never downloads or executes a program.
/// </summary>
public static class JavaScriptRuntimeLocator
{
    /// <summary>Build an explicit yt-dlp runtime setting if one is needed.</summary>
    /// <remarks>
    /// User configuration and explicit extra arguments must always take precedence.
    /// A manual --js-runtimes node or --no-js-runtimes is never overridden.
    /// </remarks>
    public static string? ResolveArgument(string? configuredRuntime, string? extraArguments)
    {
        if (!string.IsNullOrWhiteSpace(configuredRuntime))
        {
            return configuredRuntime.Trim();
        }

        // Advanced users can control the runtime through extra yt-dlp flags.
        if (CommandLine.Split(extraArguments).Any(token =>
            token is "--js-runtimes" or "--no-js-runtimes" ||
            token.StartsWith("--js-runtimes=", StringComparison.Ordinal)))
        {
            return null;
        }

        return ToArgument(FindDenoExecutable());
    }

    /// <summary>Convert an existing executable path into yt-dlp's runtime notation.</summary>
    public static string? ToArgument(string? executable)
    {
        if (string.IsNullOrWhiteSpace(executable) || !File.Exists(executable))
        {
            return null;
        }

        return "deno:" + Path.GetFullPath(executable);
    }

    /// <summary>Locate setup-managed, per-user, or PATH-installed Deno.</summary>
    /// <remarks>Only use candidates for the current OS; no network calls are made.</remarks>
    public static string? FindDenoExecutable()
    {
        var executable = OperatingSystem.IsWindows() ? "deno.exe" : "deno";

        if (OperatingSystem.IsWindows())
        {
            // Prefer the global version installed by our elevated setup wizard.
            var shared = Path.Combine(UserPaths.MachineToolsDirectory, executable);
            if (File.Exists(shared))
            {
                return shared;
            }

            // Respect a user-managed runtime without requiring administrator rights.
            var privatePath = Path.Combine(UserPaths.LocalBinDirectory, executable);
            if (File.Exists(privatePath))
            {
                return privatePath;
            }

            var userDeno = Path.Combine(UserPaths.Home, ".deno", "bin", executable);
            if (File.Exists(userDeno))
            {
                return userDeno;
            }
        }

        // Deno installed by winget, a distro package manager, or the user.
        return ToolLocator.FindOnPath(executable);
    }
}
