using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using YtDlpUi.Core.Services;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _clipboardTimer;
    private string? _lastClipboardText;
    private MainWindowViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _clipboardTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(800),
        };
        _clipboardTimer.Tick += async (_, _) => await PollClipboardAsync().ConfigureAwait(true);

        TryLoadIcon();

        // Accept links dragged from a browser.
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void TryLoadIcon()
    {
        try
        {
            Icon = new WindowIcon(
                Avalonia.Platform.AssetLoader.Open(new Uri("avares://yt-dlp-gui/Assets/yt-dlp-gui.png")));
        }
        catch (Exception)
        {
            // A missing icon is cosmetic only.
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_viewModel is not null)
        {
            _viewModel.Log.CollectionChanged -= OnLogChanged;
        }

        _viewModel = DataContext as MainWindowViewModel;

        if (_viewModel is not null)
        {
            _viewModel.Log.CollectionChanged += OnLogChanged;

            if (_viewModel.Settings.WindowWidth > MinWidth)
            {
                Width = _viewModel.Settings.WindowWidth;
            }

            if (_viewModel.Settings.WindowHeight > MinHeight)
            {
                Height = _viewModel.Settings.WindowHeight;
            }
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _clipboardTimer.Start();

        // The link box lives inside a tab page, so wait for the visual tree before focusing.
        Dispatcher.UIThread.Post(FocusUrlBox, DispatcherPriority.Background);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        _clipboardTimer.Stop();

        if (_viewModel is not null)
        {
            _viewModel.Settings.WindowWidth = Width;
            _viewModel.Settings.WindowHeight = Height;
            _viewModel.Shutdown();
        }

        base.OnClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // F5 = analyse, Ctrl+Enter = download, Ctrl+L = focus the link box
        if (e.Key == Key.F5 && _viewModel is not null)
        {
            _viewModel.AnalyzeCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control) && _viewModel is not null)
        {
            _viewModel.DownloadCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.L && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            FocusUrlBox();
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    private void FocusUrlBox()
    {
        var box = this.GetVisualDescendants()
            .OfType<TextBox>()
            .FirstOrDefault(candidate => candidate.Name == "UrlBox");

        box?.Focus();
    }

    private async Task PollClipboardAsync()
    {
        if (_viewModel is null || !_viewModel.ClipboardWatchingEnabled)
        {
            return;
        }

        try
        {
            // Reading the clipboard can fail on Wayland when the window is not focused.
            var text = await Clipboard!.TryGetTextAsync().ConfigureAwait(true);
            if (string.IsNullOrWhiteSpace(text) || string.Equals(text, _lastClipboardText, StringComparison.Ordinal))
            {
                return;
            }

            _lastClipboardText = text;
            _viewModel.OnClipboardTextChanged(text);
        }
        catch (Exception)
        {
            // Ignore transient clipboard errors (no selection, unsupported format, focus loss).
        }
    }

    private void OnLogChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            return;
        }

        var list = this.FindControl<ListBox>("LogList");
        if (list is not null && _viewModel is { Log.Count: > 0 })
        {
            list.ScrollIntoView(_viewModel.Log.Count - 1);
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        var data = e.DataTransfer;
        e.DragEffects = data is not null && (data.Contains(DataFormat.Text) || data.Contains(DataFormat.File))
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        var data = e.DataTransfer;
        if (data is not null)
        {
            var text = data.TryGetText();
            if (!string.IsNullOrWhiteSpace(text) && UrlDetector.ExtractFirstUrl(text) is { } url)
            {
                _viewModel.Url = url;
                e.Handled = true;
                return;
            }

            var firstFile = data.TryGetFiles()?
                .OfType<Avalonia.Platform.Storage.IStorageFile>()
                .FirstOrDefault();

            if (firstFile is not null && UrlDetector.ExtractFirstUrl(firstFile.Path.LocalPath) is { } fileUrl)
            {
                _viewModel.Url = fileUrl;
            }
        }

        e.Handled = true;
    }

    private async void OnCopyLogClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel is null || Clipboard is null)
        {
            return;
        }

        try
        {
            await Clipboard.SetTextAsync(_viewModel.GetLogText()).ConfigureAwait(true);
            _viewModel.AddLog("Log zkopírován do schránky.", LogKind.App);
        }
        catch (Exception)
        {
            _viewModel.AddLog("Log se nepodařilo zkopírovat.", LogKind.Warning);
        }
    }
}
