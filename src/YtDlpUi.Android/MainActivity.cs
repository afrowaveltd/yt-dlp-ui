using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;
using YtDlpUi.Android.Services;

namespace YtDlpUi.Android;

/// <summary>
/// Main launcher and text/plain share target.
/// Receiving a link does not download anything without user interaction.
/// </summary>
[Activity(
    Label = "Afrowave yt-dlp",
    Icon = "@drawable/icon",
    MainLauncher = true,
    Exported = true,
    Theme = "@android:style/Theme.Material.Light.NoActionBar",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
[IntentFilter(new[] { Intent.ActionSend },
    Categories = new[] { Intent.CategoryDefault },
    DataMimeType = "text/plain")]
public sealed class MainActivity : AvaloniaMainActivity
{
    /// <inheritdoc />
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        SharedLinkInbox.Receive(Intent);
        base.OnCreate(savedInstanceState);
    }

    /// <inheritdoc />
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        SharedLinkInbox.Receive(intent);
    }
}
