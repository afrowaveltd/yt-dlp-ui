using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YtDlpUi.Core.Localization;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.ViewModels;

/// <summary>One media file offered in the player window.</summary>
public sealed partial class MediaItemViewModel : ObservableObject
{
    public MediaItemViewModel(string path)
    {
        FilePath = path;
        FileName = Path.GetFileName(path);
        IsVideo = MediaLibrary.IsVideoFile(path);
    }

    /// <summary>Absolute path of the media file.</summary>
    public string FilePath { get; }

    /// <summary>File name shown in the list.</summary>
    public string FileName { get; }

    /// <summary>True when the extension belongs to a video container.</summary>
    public bool IsVideo { get; }

    /// <summary>Placeholder icon: picture for video, note for audio.</summary>
    public string Glyph => IsVideo ? "🎬" : "🎵";

    /// <summary>Last write time, shown as the list subtitle.</summary>
    public string UpdatedText
    {
        get
        {
            try
            {
                return File.GetLastWriteTime(FilePath).ToString("g");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}

/// <summary>
/// Everything the player needs from its owner. A small value object keeps the window
/// testable without constructing the whole main window view model.
/// </summary>
public sealed record PlayerHost(
    Func<string?> DownloadDirectory,
    Func<string?> FfmpegPath,
    Action<string, LogKind> Log,
    Action<string> ConvertRequested);

/// <summary>
/// Drives the player window: browsing downloaded media, reading its technical details and
/// showing a scrubable preview frame. Actual playback is handed to the system video player,
/// which keeps the application free of native media dependencies.
/// </summary>
public sealed partial class PlayerViewModel : ObservableObject, IDisposable
{
    private readonly PlayerHost _host;
    private CancellationTokenSource? _previewCts;
    private FfmpegTools? _tools;
    private bool _disposed;

    public PlayerViewModel(PlayerHost host, string? startPath = null)
    {
        _host = host;
        Refresh();
        SelectInitial(startPath);
    }

    /// <summary>Media files found in the download folder.</summary>
    public ObservableCollection<MediaItemViewModel> Items { get; } = new();

    /// <summary>File highlighted in the list.</summary>
    [ObservableProperty] public partial MediaItemViewModel? SelectedItem { get; set; }

    /// <summary>Technical details of the selected file.</summary>
    [ObservableProperty] public partial MediaProfile? Profile { get; private set; }

    /// <summary>Preview picture at the current position.</summary>
    [ObservableProperty] public partial Bitmap? PreviewFrame { get; private set; }

    /// <summary>Position used for the preview frame, in seconds.</summary>
    [ObservableProperty] public partial double PositionSeconds { get; set; }

    /// <summary>Duration of the selected file in seconds.</summary>
    [ObservableProperty] public partial double DurationSeconds { get; private set; }

    /// <summary>True while the preview frame or the metadata is being read.</summary>
    [ObservableProperty] public partial bool IsBusy { get; private set; }

    /// <summary>Hint shown under the preview.</summary>
    [ObservableProperty] public partial string StatusText { get; set; } = string.Empty;

    /// <summary>True when the selected file has a picture to preview.</summary>
    [ObservableProperty] public partial bool HasVideo { get; private set; }

    /// <summary>True when the list contains at least one file.</summary>
    public bool HasItems => Items.Count > 0;

    /// <summary>True when the position slider can be used.</summary>
    public bool CanScrub => HasVideo && DurationSeconds > 0;

    /// <summary>Name of the system player the file will open in, when one is known.</summary>
    public string SystemPlayerHint => Localizer("The file opens in your system video player.");

    /// <summary>Rescan the download folder.</summary>
    [RelayCommand]
    private void Refresh()
    {
        var keep = SelectedItem?.FilePath;
        Items.Clear();

        foreach (var path in MediaLibrary.Scan(_host.DownloadDirectory()))
        {
            Items.Add(new MediaItemViewModel(path));
        }

        OnPropertyChanged(nameof(HasItems));

        if (Items.Count == 0)
        {
            SelectedItem = null;
            StatusText = Localizer("No media files found in the download folder.");
            return;
        }

        SelectedItem = Items.FirstOrDefault(item =>
                           string.Equals(item.FilePath, keep, StringComparison.Ordinal))
                       ?? Items[0];
    }

    /// <summary>Open the selected file in the system video player.</summary>
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Play()
    {
        if (SelectedItem is null)
        {
            return;
        }

        if (ShellUtil.OpenFile(SelectedItem.FilePath))
        {
            StatusText = string.Format(Localizer("Opening {0} in the system player…"), SelectedItem.FileName);
            _host.Log($"{Localizer("Playing")}: {SelectedItem.FileName}", LogKind.App);
        }
        else
        {
            StatusText = Localizer("No player is associated with this file type.");
            _host.Log(Localizer("No player is associated with this file type."), LogKind.Warning);
        }
    }

    /// <summary>Reveal the selected file in the file manager.</summary>
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void OpenFolder()
    {
        if (SelectedItem is not null)
        {
            ShellUtil.OpenContainingFolder(SelectedItem.FilePath);
        }
    }

    /// <summary>Send the selected file to the converter tab.</summary>
    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Convert()
    {
        if (SelectedItem is not null)
        {
            _host.ConvertRequested(SelectedItem.FilePath);
        }
    }

    private bool HasSelection() => SelectedItem is not null;

    private void SelectInitial(string? startPath)
    {
        if (Items.Count == 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(startPath))
        {
            var match = Items.FirstOrDefault(item =>
                string.Equals(item.FilePath, startPath, StringComparison.Ordinal));
            if (match is not null)
            {
                SelectedItem = match;
                return;
            }
        }

        SelectedItem ??= Items[0];
    }

    partial void OnSelectedItemChanged(MediaItemViewModel? value)
    {
        PlayCommand.NotifyCanExecuteChanged();
        OpenFolderCommand.NotifyCanExecuteChanged();
        ConvertCommand.NotifyCanExecuteChanged();
        _ = LoadSelectionAsync(value);
    }

    partial void OnPositionSecondsChanged(double value) => RequestPreview(value);

    private async Task LoadSelectionAsync(MediaItemViewModel? item)
    {
        DisposePreview();
        Profile = null;
        HasVideo = false;
        DurationSeconds = 0;
        PositionSeconds = 0;
        OnPropertyChanged(nameof(CanScrub));

        if (item is null)
        {
            return;
        }

        IsBusy = true;
        StatusText = Localizer("Reading media information…");

        try
        {
            _tools ??= await ToolLocator
                .ResolveFfmpegToolsAsync(_host.FfmpegPath())
                .ConfigureAwait(true);

            if (_tools.FfprobePath is not null)
            {
                Profile = await MediaProbe
                    .ProbeAsync(_tools.FfprobePath, item.FilePath)
                    .ConfigureAwait(true);
            }

            Profile ??= new MediaProfile { FilePath = item.FilePath };
            HasVideo = Profile.HasVideo;
            DurationSeconds = Profile.DurationSeconds ?? 0;
            StatusText = Profile.Summary;
            OnPropertyChanged(nameof(CanScrub));

            if (HasVideo)
            {
                RequestPreview(0);
            }
        }
        catch (Exception exception)
        {
            StatusText = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Regrab the preview frame; a newer request cancels the previous one.</summary>
    private void RequestPreview(double position)
    {
        if (!HasVideo || SelectedItem is null)
        {
            return;
        }

        _previewCts?.Cancel();
        _previewCts?.Dispose();
        _previewCts = new CancellationTokenSource();
        _ = GrabPreviewAsync(SelectedItem.FilePath, position, _previewCts.Token);
    }

    private async Task GrabPreviewAsync(string path, double position, CancellationToken cancellationToken)
    {
        try
        {
            _tools ??= await ToolLocator
                .ResolveFfmpegToolsAsync(_host.FfmpegPath(), cancellationToken)
                .ConfigureAwait(true);

            var bytes = await FramePreview
                .GrabPngAsync(_tools.FfmpegPath, path, position, cancellationToken: cancellationToken)
                .ConfigureAwait(true);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (bytes is null)
            {
                DisposePreview();
                return;
            }

            using var stream = new MemoryStream(bytes);
            var bitmap = new Bitmap(stream);
            DisposePreview();
            PreviewFrame = bitmap;
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer preview request.
        }
        catch (Exception)
        {
            DisposePreview();
        }
    }

    private void DisposePreview()
    {
        var previous = PreviewFrame;
        PreviewFrame = null;
        previous?.Dispose();
    }

    private static string Localizer(string english) => SimpleLocalizer.Instance.Translate(english);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _previewCts?.Cancel();
        _previewCts?.Dispose();
        DisposePreview();
    }
}
