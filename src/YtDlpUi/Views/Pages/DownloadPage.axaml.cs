using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using YtDlpUi.Core.Services;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views.Pages;

public partial class DownloadPage : UserControl
{
    public DownloadPage() => InitializeComponent();

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private async void OnPasteClick(object? sender, RoutedEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top?.Clipboard is null || ViewModel is not { } viewModel)
        {
            return;
        }

        try
        {
            var text = await top.Clipboard.TryGetTextAsync().ConfigureAwait(true);
            var url = UrlDetector.ExtractFirstUrl(text);
            if (url is null)
            {
                viewModel.AddLog("Ve schránce není žádný odkaz.", LogKind.Warning);
                return;
            }

            viewModel.Url = url;
            viewModel.AddLog($"Odkaz vložen ze schránky: {url}", LogKind.App);

            if (viewModel.Settings.AutoAnalyzeOnPaste && viewModel.Toolchain.YtDlpAvailable)
            {
                viewModel.AnalyzeCommand.Execute(null);
            }
        }
        catch (Exception ex)
        {
            ViewModel?.AddLog($"Schránku nelze přečíst: {ex.Message}", LogKind.Warning);
        }
    }

    private async void OnBrowseOutputFolderClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        var folder = await Dialogs
            .PickFolderAsync(this, "Vyberte složku pro ukládání", viewModel.Options.OutputDirectory)
            .ConfigureAwait(true);

        if (folder is not null)
        {
            viewModel.Options.OutputDirectory = folder;
        }
    }
}
