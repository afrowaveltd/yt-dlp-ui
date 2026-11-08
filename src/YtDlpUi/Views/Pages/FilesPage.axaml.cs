using Avalonia.Controls;
using Avalonia.Interactivity;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views.Pages;

public partial class FilesPage : UserControl
{
    public FilesPage() => InitializeComponent();

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private async void OnBrowseOutputClick(object? sender, RoutedEventArgs e)
    {
        var folder = await Dialogs
            .PickFolderAsync(this, "Vyberte cílovou složku", ViewModel?.Options.OutputDirectory)
            .ConfigureAwait(true);

        if (folder is not null && ViewModel is { } viewModel)
        {
            viewModel.Options.OutputDirectory = folder;
        }
    }

    private async void OnBrowseTempClick(object? sender, RoutedEventArgs e)
    {
        var folder = await Dialogs
            .PickFolderAsync(this, "Vyberte dočasnou složku", ViewModel?.Options.TempDirectory)
            .ConfigureAwait(true);

        if (folder is not null && ViewModel is { } viewModel)
        {
            viewModel.Options.TempDirectory = folder;
        }
    }

    private async void OnBrowseCacheClick(object? sender, RoutedEventArgs e)
    {
        var folder = await Dialogs
            .PickFolderAsync(this, "Vyberte složku cache", ViewModel?.Options.CacheDirectory)
            .ConfigureAwait(true);

        if (folder is not null && ViewModel is { } viewModel)
        {
            viewModel.Options.CacheDirectory = folder;
        }
    }

    private async void OnBrowseBatchClick(object? sender, RoutedEventArgs e)
    {
        var file = await Dialogs
            .PickFileAsync(this, "Vyberte dávkový soubor", null, "*.txt", "*")
            .ConfigureAwait(true);

        if (file is not null && ViewModel is { } viewModel)
        {
            viewModel.Options.BatchFile = file;
        }
    }
}
