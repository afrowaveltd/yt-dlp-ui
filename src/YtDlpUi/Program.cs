using Avalonia;

namespace YtDlpUi;

internal static class Program
{
    // Avalonia configuration; the same builder is reused by the design-time previewer.
    [STAThread]
    public static void Main(string[] args)
    {
        // YTDLP_GUI_TRACE=1 routes Avalonia warnings (including binding errors) to the console.
        if (Environment.GetEnvironmentVariable("YTDLP_GUI_TRACE") == "1")
        {
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

        // Virtual machines, remote desktops and containers often lack working OpenGL.
        // Setting YTDLP_GUI_SOFTWARE_RENDER=1 forces Skia's CPU renderer.
        if (Environment.GetEnvironmentVariable("YTDLP_GUI_SOFTWARE_RENDER") == "1")
        {
            builder = builder.With(new X11PlatformOptions
            {
                RenderingMode = [X11RenderingMode.Software],
            });
        }

        return builder;
    }
}
