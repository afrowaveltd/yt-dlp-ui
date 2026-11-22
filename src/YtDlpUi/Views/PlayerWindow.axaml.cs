using Avalonia.Controls;
using Avalonia.Interactivity;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views;

/// <summary>
/// Window that lists downloaded media, shows a preview frame and hands playback to the
/// system video player.
/// </summary>
public partial class PlayerWindow : Window
{
    public PlayerWindow() => InitializeComponent();

    protected override void OnClosed(EventArgs e)
    {
        (DataContext as PlayerViewModel)?.Dispose();
        base.OnClosed(e);
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    /// <summary>
    /// Focus an existing player window on another file: rescan the folder and select it.
    /// </summary>
    public void SelectFile(string? filePath)
    {
        if (DataContext is not PlayerViewModel viewModel)
        {
            return;
        }

        viewModel.RefreshCommand.Execute(null);

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            var match = viewModel.Items.FirstOrDefault(item =>
                string.Equals(item.FilePath, filePath, StringComparison.Ordinal));

            if (match is not null)
            {
                viewModel.SelectedItem = match;
            }
        }
    }
}
