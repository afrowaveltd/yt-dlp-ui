using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using YtDlpUi.Core.Localization;
using YtDlpUi.Core.Models;
using YtDlpUi.ViewModels;
using YtDlpUi.Views;
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

// ---------------------------------------------------------------------------
// 0.2.0: the FFmpeg converter page must bind to a real ConverterViewModel and
// renderer its controls, not an approximation of the layout.
// ---------------------------------------------------------------------------
await session.Dispatch(() =>
{
    var host = new ConverterHost(
        () => null,
        () => true,
        (message, kind) => { });

    var converter = new ConverterViewModel(host, Path.GetTempPath())
    {
        StatusText = "ready for test",
    };

    var view = new ConverterPage
    {
        // The page expects the main window view model and reaches Converter through it.
        DataContext = new ConverterPageContext(converter),
    };

    var window = new Window { Width = 1200, Height = 1400, Content = view };
    try
    {
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var combos = view.GetVisualDescendants().OfType<ComboBox>().ToArray();
        if (combos.Length < 3)
        {
            throw new InvalidOperationException(
                "Converter page must show format, quality and height selectors, found " + combos.Length);
        }

        // Each selector must be bound to its option list; an unbound box would show nothing.
        var targets = combos.FirstOrDefault(combo => combo.ItemCount == Enum.GetValues<ConversionTarget>().Length);
        if (targets is null)
        {
            throw new InvalidOperationException(
                "Format selector is not bound to the conversion targets. Counts: " +
                string.Join(",", combos.Select(combo => combo.ItemCount)));
        }

        if (targets.SelectedItem is not TargetOption { Value: ConversionTarget.Mp4 })
        {
            throw new InvalidOperationException("Format selector should default to MP4.");
        }

        // Option labels come from the localizer, so they must never leak the record type name.
        var labels = targets.GetVisualDescendants().OfType<TextBlock>()
            .Select(block => block.Text ?? string.Empty).ToArray();
        if (labels.Any(text => text.Contains("TargetOption", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Format selector rendered CLR class names: " + string.Join(" | ", labels));
        }

        var textBoxes = view.GetVisualDescendants().OfType<TextBox>().ToArray();
        if (textBoxes.Length < 3)
        {
            throw new InvalidOperationException("Converter page is missing its path and trim inputs.");
        }

        // Switching the language must update the option labels in place.
        // MKV is used on purpose: its English and Czech labels differ, so a missing
        // translation would fail this check instead of silently falling back.
        var localizer = SimpleLocalizer.Instance;
        var originalLanguage = localizer.Language;
        try
        {
            converter.Options.Target = ConversionTarget.Mkv;
            Dispatcher.UIThread.RunJobs();
            var english = targets.GetVisualDescendants().OfType<TextBlock>()
                .Select(block => block.Text ?? string.Empty).ToArray();
            if (!english.Any(text => text.Contains("keeps any codec", StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "English MKV label was not rendered: " + string.Join(" | ", english));
            }

            localizer.SetLanguage("cs");
            Dispatcher.UIThread.RunJobs();
            var czech = targets.GetVisualDescendants().OfType<TextBlock>()
                .Select(block => block.Text ?? string.Empty).ToArray();
            if (!czech.Any(text => text.Contains("zachová jakýkoli kodek", StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "Czech labels did not reach the format selector: " + string.Join(" | ", czech));
            }
        }
        finally
        {
            localizer.SetLanguage(originalLanguage);
            Dispatcher.UIThread.RunJobs();
        }

        Console.WriteLine("Avalonia headless converter page smoke test passed.");
    }
    finally
    {
        converter.Dispose();
        window.Close();
    }
}, CancellationToken.None);

// ---------------------------------------------------------------------------
// 0.2.0: the player window must render its file list, preview area and actions
// with a real PlayerViewModel, without needing FFmpeg installed.
// ---------------------------------------------------------------------------
await session.Dispatch(() =>
{
    var mediaFolder = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-player-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(mediaFolder);
    try
    {
        // Placeholder bytes: the window lists files by extension and must never crash
        // when metadata cannot be read on a machine without FFmpeg.
        File.WriteAllText(Path.Combine(mediaFolder, "clip-one.mp4"), "not a real video");
        File.WriteAllText(Path.Combine(mediaFolder, "song-two.mp3"), "not real audio");

        var host = new PlayerHost(
            () => mediaFolder,
            () => null,
            (message, kind) => { },
            path => { });

        var player = new PlayerViewModel(host);
        var window = new PlayerWindow { DataContext = player, Width = 900, Height = 700 };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            if (!player.HasItems || player.Items.Count != 2)
            {
                throw new InvalidOperationException(
                    "Player did not list the media files, found " + player.Items.Count);
            }

            var names = window.GetVisualDescendants().OfType<TextBlock>()
                .Select(block => block.Text ?? string.Empty).ToArray();
            if (!names.Any(text => text.Contains("clip-one.mp4", StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "Player window did not render the file names: " + string.Join(" | ", names));
            }

            if (names.Any(text => text.Contains("MediaItemViewModel", StringComparison.Ordinal)))
            {
                throw new InvalidOperationException("Player rendered CLR class names instead of file names.");
            }

            var buttons = window.GetVisualDescendants().OfType<Button>()
                .Select(button => button.Content?.ToString() ?? string.Empty).ToArray();
            if (!buttons.Any(text => text.Contains("Play", StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "Player window is missing its play action: " + string.Join(" | ", buttons));
            }

            Console.WriteLine("Avalonia headless player window smoke test passed.");
        }
        finally
        {
            player.Dispose();
            window.Close();
        }
    }
    finally
    {
        Directory.Delete(mediaFolder, recursive: true);
    }
}, CancellationToken.None);
