using System.Diagnostics;

namespace YtDlpUi.Core.Services;

/// <summary>Opens files, folders and links with the desktop's default handler.</summary>
public static class ShellUtil
{
    /// <summary>Reveal a file in the system file manager (or open its folder).</summary>
    public static bool OpenContainingFolder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var expanded = ToolLocator.ExpandHome(path!.Trim());
        var directory = Directory.Exists(expanded) ? expanded : Path.GetDirectoryName(expanded);
        return string.IsNullOrEmpty(directory) ? false : OpenFolder(directory);
    }

    /// <summary>Open a directory in the system file manager.</summary>
    public static bool OpenFolder(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return false;
        }

        var expanded = ToolLocator.ExpandHome(directory!.Trim());
        if (!Directory.Exists(expanded))
        {
            return false;
        }

        return StartDetached(expanded);
    }

    /// <summary>Open a URL in the default browser.</summary>
    public static bool OpenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return StartDetached(url!.Trim());
    }

    private static bool StartDetached(string target)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (OperatingSystem.IsWindows())
            {
                startInfo.FileName = "explorer.exe";
                startInfo.ArgumentList.Add(target);
            }
            else if (OperatingSystem.IsMacOS())
            {
                startInfo.FileName = "open";
                startInfo.ArgumentList.Add(target);
            }
            else
            {
                startInfo.FileName = "xdg-open";
                startInfo.ArgumentList.Add(target);
            }

            using var process = Process.Start(startInfo);
            return process is not null;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
