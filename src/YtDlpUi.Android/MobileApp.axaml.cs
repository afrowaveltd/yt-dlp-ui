using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using YtDlpUi.Android.Services;
using YtDlpUi.Android.ViewModels;
using YtDlpUi.Android.Views;
using YtDlpUi.Core.Localization;

namespace YtDlpUi.Android;

/// <summary>Dedicated mobile UI; only the platform-neutral Core library is shared.</summary>
public partial class MobileApp : Application
{
    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        SimpleLocalizer.Instance.SetLanguage("system");
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IActivityApplicationLifetime activities)
        {
            activities.MainViewFactory = () =>
            {
                var vm = new MobileViewModel(new AndroidDirectDownloadService());
                SharedLinkInbox.Register(vm);
                return new HomeView { DataContext = vm };
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
