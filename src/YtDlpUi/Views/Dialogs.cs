using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace YtDlpUi.Views;

/// <summary>Small wrappers around the platform storage provider used by the "Procházet…" buttons.</summary>
public static class Dialogs
{
    /// <summary>Ask the user for a folder.</summary>
    public static async Task<string?> PickFolderAsync(Visual anchor, string title, string? startAt = null)
    {
        var top = TopLevel.GetTopLevel(anchor);
        if (top is null)
        {
            return null;
        }

        try
        {
            var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                SuggestedStartLocation = await TryGetFolderAsync(top, startAt).ConfigureAwait(true),
            });

            var folder = folders.FirstOrDefault();
            return folder?.Path.LocalPath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Ask the user for an existing file.</summary>
    public static async Task<string?> PickFileAsync(
        Visual anchor,
        string title,
        string? startAt = null,
        params string[] patterns)
    {
        var top = TopLevel.GetTopLevel(anchor);
        if (top is null)
        {
            return null;
        }

        try
        {
            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                SuggestedStartLocation = await TryGetFolderAsync(top, startAt).ConfigureAwait(true),
            };

            if (patterns.Length > 0)
            {
                options.FileTypeFilter = [new FilePickerFileType(title) { Patterns = patterns }];
            }

            var files = await top.StorageProvider.OpenFilePickerAsync(options);
            var file = files.FirstOrDefault();
            return file?.Path.LocalPath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<IStorageFolder?> TryGetFolderAsync(TopLevel top, string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(Core.Services.ToolLocator.ExpandHome(path!)))
        {
            return null;
        }

        try
        {
            return await top.StorageProvider
                .TryGetFolderFromPathAsync(Core.Services.ToolLocator.ExpandHome(path!))
                .ConfigureAwait(true);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
