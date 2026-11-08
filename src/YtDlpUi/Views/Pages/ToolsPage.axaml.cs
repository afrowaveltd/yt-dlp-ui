using Avalonia.Controls;
using Avalonia.Interactivity;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views.Pages;

public partial class ToolsPage : UserControl
{
    public ToolsPage() => InitializeComponent();

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private void OnCancelInstallClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.Toolchain.CancelRunning();
        ViewModel?.Toolchain.AppendLog("Ruším…");
    }

    private async void OnBrowseYtDlpClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        var file = await Dialogs
            .PickFileAsync(this, "Vyberte spustitelný soubor yt-dlp", null, "yt-dlp*", "*")
            .ConfigureAwait(true);

        if (file is not null)
        {
            viewModel.Settings.YtDlpPath = file;
            await viewModel.Toolchain.RefreshAsync().ConfigureAwait(true);
        }
    }

    private async void OnBrowseFfmpegClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        var file = await Dialogs
            .PickFileAsync(this, "Vyberte spustitelný soubor ffmpeg", null, "ffmpeg*", "*")
            .ConfigureAwait(true);

        if (file is not null)
        {
            viewModel.Settings.FfmpegPath = file;
            await viewModel.Toolchain.RefreshAsync().ConfigureAwait(true);
        }
    }

    private async void OnBrowseDefaultOutputClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        var folder = await Dialogs
            .PickFolderAsync(this, "Vyberte výchozí složku pro ukládání", viewModel.Settings.DefaultOutputDirectory)
            .ConfigureAwait(true);

        if (folder is not null)
        {
            viewModel.Settings.DefaultOutputDirectory = folder;
        }
    }
}
