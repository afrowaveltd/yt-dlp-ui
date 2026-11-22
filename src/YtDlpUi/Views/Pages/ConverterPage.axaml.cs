using Avalonia.Controls;
using Avalonia.Interactivity;
using YtDlpUi.Core.Services;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views.Pages;

/// <summary>FFmpeg conversion page; the data context is the main window view model.</summary>
public partial class ConverterPage : UserControl
{
    public ConverterPage() => InitializeComponent();

    private ConverterViewModel? Converter =>
        (DataContext as MainWindowViewModel)?.Converter;

    private async void OnBrowseSourceClick(object? sender, RoutedEventArgs e)
    {
        if (Converter is not { } converter)
        {
            return;
        }

        var file = await Dialogs
            .PickFileAsync(
                this,
                "Vyberte soubor ke konverzi",
                converter.Options.SourcePath,
                "*.mp4", "*.mkv", "*.webm", "*.mov", "*.avi", "*.mp3", "*.m4a", "*.opus", "*.flac", "*.wav", "*")
            .ConfigureAwait(true);

        if (file is not null)
        {
            converter.SetSource(file);
        }
    }

    private async void OnBrowseOutputClick(object? sender, RoutedEventArgs e)
    {
        if (Converter is not { } converter)
        {
            return;
        }

        var folder = await Dialogs
            .PickFolderAsync(this, "Vyberte složku pro výsledek", converter.Options.OutputDirectory)
            .ConfigureAwait(true);

        if (folder is not null)
        {
            converter.Options.OutputDirectory = folder;
        }
    }
}
