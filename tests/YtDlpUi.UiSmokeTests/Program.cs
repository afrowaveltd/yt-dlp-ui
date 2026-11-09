using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using YtDlpUi.Core.Models;
using YtDlpUi.Views.Pages;

using var session = HeadlessUnitTestSession.StartNew(typeof(YtDlpUi.App));
await session.Dispatch(() =>
{
    var entries = new ObservableCollection<MediaEntry>
    {
        new() { Index = 1, Title = "C# 11 and .NET 7 features", Duration = 125 },
        new() { Index = 2, Title = "Second playlist video", Duration = 0 },
    };

    // Test the real compiled DownloadPage.axaml, not an approximation of its layout.
    var view = new DownloadPage
    {
        DataContext = new
        {
            PlaylistEntries = entries,
            InfoIsPlaylist = true,
            Info = new MediaInfo { IsPlaylist = true, Title = "Test playlist", PlaylistCount = 2 },
            HasInfo = false,
            ShowWelcome = false,
        },
    };

    var window = new Window { Width = 1200, Height = 1300, Content = view };
    try
    {
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var list = view.GetVisualDescendants().OfType<ListBox>()
            .FirstOrDefault(control => ReferenceEquals(control.ItemsSource, entries));
        if (list is null)
        {
            throw new InvalidOperationException("Playlist ListBox was not bound to MediaEntry collection.");
        }

        // The previous bug passed structural XAML checks but rendered the CLR class name.
        // Inspect the generated text controls after a real headless layout/binding pass.
        var labels = list.GetVisualDescendants().OfType<TextBlock>()
            .Select(control => control.Text ?? string.Empty).ToArray();

        if (!labels.Any(text => text.Contains("C# 11 and .NET 7 features", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                "Playlist title was not rendered. Actual text: " + string.Join(" | ", labels));
        }

        if (labels.Any(text => text.Contains("YtDlpUi.Core.Models.MediaEntry", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Playlist rendered CLR class names instead of video titles.");
        }

        Console.WriteLine("Avalonia headless playlist rendering smoke test passed.");
    }
    finally
    {
        window.Close();
    }
}, CancellationToken.None);
