using Avalonia.Controls;
using Avalonia.Interactivity;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views.Pages;

public partial class AdvancedPage : UserControl
{
    public AdvancedPage() => InitializeComponent();

    private async void OnCopyCommandClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var top = TopLevel.GetTopLevel(this);
        if (top?.Clipboard is null)
        {
            return;
        }

        try
        {
            await top.Clipboard.SetTextAsync(viewModel.CommandPreview).ConfigureAwait(true);
            viewModel.AddLog("Příkaz zkopírován do schránky.", LogKind.App);
        }
        catch (Exception)
        {
            viewModel.AddLog("Příkaz se nepodařilo zkopírovat.", LogKind.Warning);
        }
    }

    private async void OnBrowseConfigClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var file = await Dialogs
            .PickFileAsync(this, "Vyberte konfigurační soubor yt-dlp", null, "*.conf", "*.txt", "*")
            .ConfigureAwait(true);

        if (file is not null)
        {
            viewModel.Options.ConfigLocations = file;
        }
    }
}
