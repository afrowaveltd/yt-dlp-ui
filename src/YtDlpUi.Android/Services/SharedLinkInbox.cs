using Android.Content;
using Avalonia.Threading;
using YtDlpUi.Android.ViewModels;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Android.Services;

/// <summary>
/// Routes ACTION_SEND text/plain URLs into the active Avalonia view.
/// Uses a weak reference to avoid keeping destroyed Android activities alive.
/// </summary>
public static class SharedLinkInbox
{
    private static string? _pendingUrl;
    private static WeakReference<MobileViewModel>? _current;

    /// <summary>Called when Android (re)creates a view model for an activity.</summary>
    public static void Register(MobileViewModel model)
    {
        _current = new WeakReference<MobileViewModel>(model);
        if (_pendingUrl is { } url)
        {
            _pendingUrl = null;
            Dispatcher.UIThread.Post(() => model.ReceiveLink(url));
        }
    }

    /// <summary>Read the explicit Android share intent, without starting a download.</summary>
    public static void Receive(Intent? intent)
    {
        if (intent?.Action != Intent.ActionSend ||
            intent.Type is null ||
            !intent.Type.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var text = intent.GetStringExtra(Intent.ExtraText);
        if (!MobileLinkParser.TryExtract(text, out var url))
        {
            return;
        }

        if (_current is not null && _current.TryGetTarget(out var target))
        {
            Dispatcher.UIThread.Post(() => target.ReceiveLink(url));
        }
        else
        {
            _pendingUrl = url;
        }
    }
}
