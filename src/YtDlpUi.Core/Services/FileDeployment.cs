namespace YtDlpUi.Core.Services;

/// <summary>
/// Transactional directory swaps for private tool installations.
/// Both paths should be on the same filesystem; an existing target is preserved
/// if the new tree cannot be installed.
/// </summary>
public static class FileDeployment
{
    /// <summary>
    /// Replace <paramref name="destination"/> with an already prepared and validated
    /// directory. Keep a backup during the move, restore it on failure and remove
    /// that backup only after a successful installation.
    /// </summary>
    public static void ReplaceDirectory(string stagedDirectory, string destination)
    {
        if (!Directory.Exists(stagedDirectory))
        {
            throw new DirectoryNotFoundException(stagedDirectory);
        }

        var parent = Path.GetDirectoryName(Path.GetFullPath(destination))
            ?? throw new ArgumentException("Destination must have a parent.", nameof(destination));
        Directory.CreateDirectory(parent);
        var backup = Path.Combine(parent, ".yt-dlp-ui-backup-" + Guid.NewGuid().ToString("N"));
        var hadExisting = Directory.Exists(destination);
        if (hadExisting)
        {
            Directory.Move(destination, backup);
        }

        try
        {
            Directory.Move(stagedDirectory, destination);
        }
        catch
        {
            // Do not obscure the original move failure with a cleanup error.
            if (hadExisting && Directory.Exists(backup) && !Directory.Exists(destination))
            {
                Directory.Move(backup, destination);
            }

            throw;
        }

        // Backup cleanup failure is not installation failure. The new destination
        // is already active and the backup remains available for manual cleanup.
        if (hadExisting)
        {
            try
            {
                Directory.Delete(backup, recursive: true);
            }
            catch (IOException)
            {
                // Old binaries may be in use on Windows.
            }
            catch (UnauthorizedAccessException)
            {
                // Leave the backup rather than claiming that installation failed.
            }
        }
    }
}
