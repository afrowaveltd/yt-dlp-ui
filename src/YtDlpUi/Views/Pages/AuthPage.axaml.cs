using Avalonia.Controls;
using Avalonia.Interactivity;
using YtDlpUi.ViewModels;

namespace YtDlpUi.Views.Pages;

public partial class AuthPage : UserControl
{
    public AuthPage() => InitializeComponent();

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private void OnUseFirefoxClick(object? sender, RoutedEventArgs e) => SetBrowser("firefox");

    private void OnUseChromeClick(object? sender, RoutedEventArgs e) => SetBrowser("chrome");

    private void OnUseChromiumClick(object? sender, RoutedEventArgs e) => SetBrowser("chromium");

    private void OnUseBraveClick(object? sender, RoutedEventArgs e) => SetBrowser("brave");

    private void OnClearBrowserClick(object? sender, RoutedEventArgs e) => SetBrowser(string.Empty);

    private void SetBrowser(string browser)
    {
        if (ViewModel is { } viewModel)
        {
            viewModel.Options.CookiesFromBrowser = browser;
            viewModel.AddLog(
                browser.Length == 0
                    ? "Načítání cookies z prohlížeče vypnuto."
                    : $"Cookies se budou načítat z prohlížeče: {browser}.",
                LogKind.App);
        }
    }

    private async void OnBrowseCookiesClick(object? sender, RoutedEventArgs e)
    {
        var file = await Dialogs
            .PickFileAsync(this, "Vyberte soubor s cookies", null, "*.txt", "*")
            .ConfigureAwait(true);

        if (file is not null && ViewModel is { } viewModel)
        {
            viewModel.Options.CookiesFile = file;
        }
    }

    private async void OnBrowseCertificateClick(object? sender, RoutedEventArgs e)
    {
        var file = await Dialogs
            .PickFileAsync(this, "Vyberte klientský certifikát", null, "*.pem", "*.crt", "*")
            .ConfigureAwait(true);

        if (file is not null && ViewModel is { } viewModel)
        {
            viewModel.Options.ClientCertificate = file;
        }
    }

    private async void OnBrowseCertificateKeyClick(object? sender, RoutedEventArgs e)
    {
        var file = await Dialogs
            .PickFileAsync(this, "Vyberte soukromý klíč", null, "*.pem", "*.key", "*")
            .ConfigureAwait(true);

        if (file is not null && ViewModel is { } viewModel)
        {
            viewModel.Options.ClientCertificateKey = file;
        }
    }
}
