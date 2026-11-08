using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace YtDlpUi.Android;

/// <summary>Android entry point for the Avalonia mobile application.</summary>
[Application]
public sealed class ApplicationHost : AvaloniaAndroidApplication<MobileApp>
{
    /// <summary>Required constructor for Android runtime activation.</summary>
    public ApplicationHost(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    /// <inheritdoc />
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont();
}
