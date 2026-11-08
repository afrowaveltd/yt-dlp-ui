using Avalonia.Controls;
using Avalonia.Interactivity;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views.Pages;

public partial class PlaylistPage : UserControl
{
    public PlaylistPage() => InitializeComponent();

    private async void OnBrowseArchiveClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var file = await Dialogs
            .PickFileAsync(this, "Vyberte soubor archivu", null, "*.txt", "*")
            .ConfigureAwait(true);

        if (file is not null)
        {
            viewModel.Options.DownloadArchive = file;
        }
    }
}
