using System.Text.RegularExpressions;
using System.Xml.Linq;

internal static class DocumentationChecks
{
    /// <summary>
    /// Verify the paired bilingual documentation, README mirror and local links.
    /// Runs with the existing smoke test executable on Windows and Linux CI.
    /// </summary>
    public static void Verify()
    {
        var root = FindRepositoryRoot();
        if (root is null)
        {
            // Allow running the smoke executable outside the source checkout.
            return;
        }

        var docs = Path.Combine(root, "Docs");
        Require(Directory.Exists(docs), "Docs directory must exist");

        var topics = Directory.EnumerateDirectories(docs).OrderBy(path => path).ToArray();
        Require(topics.Length >= 9, "Expected bilingual documentation topics");
        foreach (var topic in topics)
        {
            Require(File.Exists(Path.Combine(topic, "en.md")), "Missing en.md in " + topic);
            Require(File.Exists(Path.Combine(topic, "cs.md")), "Missing cs.md in " + topic);
            Require(new FileInfo(Path.Combine(topic, "en.md")).Length > 200, "English page is empty: " + topic);
            Require(new FileInfo(Path.Combine(topic, "cs.md")).Length > 200, "Czech page is empty: " + topic);
        }

        var rootReadme = File.ReadAllText(Path.Combine(root, "README.md"));
        var expectedMirror = rootReadme
            .Replace("](screenshots/", "](../../screenshots/", StringComparison.Ordinal)
            .Replace("](Docs/", "](../", StringComparison.Ordinal)
            .Replace("](LICENSE)", "](../../LICENSE)", StringComparison.Ordinal);

        var englishMirror = File.ReadAllText(Path.Combine(docs, "README", "en.md"));
        Require(englishMirror == expectedMirror, "Docs/README/en.md differs from the canonical English README");
        var props = File.ReadAllText(Path.Combine(root, "Directory.Build.props"));
        var setup = File.ReadAllText(Path.Combine(root, "packaging", "windows", "setup.iss"));
        var match = Regex.Match(props, @"<Version>([0-9]+\.[0-9]+\.[0-9]+)</Version>");
        Require(match.Success, "App version must follow MAJOR.MINOR.PATCH");
        var version = match.Groups[1].Value;
        Require(rootReadme.Contains(version, StringComparison.Ordinal),
            "README must reference the current version");
        Require(setup.Contains("#define AppVersion \"" + version + "\"", StringComparison.Ordinal),
            "Installer fallback must match Directory.Build.props");

        var markdownFiles = topics.SelectMany(topic => Directory.EnumerateFiles(topic, "*.md"))
            .Append(Path.Combine(root, "README.md"));

        foreach (var markdown in markdownFiles)
        {
            ValidateLocalLinks(markdown);
        }

        VerifyLanguagePickerLayout(root);
        VerifyDenoUiLayout(root);
        VerifyPlaylistPreviewTemplate(root);

        Console.WriteLine($"Documentation smoke checks passed: {topics.Length} bilingual topics and local links.");
    }

    /// <summary>
    /// Regression guard for Linux emoji-font clipping in the language selector.
    /// A fixed vector icon and an empty Grid column keep the globe fully visible.
    /// The Avalonia XAML compiler additionally validates these elements on CI.
    /// </summary>
    private static void VerifyLanguagePickerLayout(string root)
    {
        var path = Path.Combine(root, "src", "YtDlpUi", "Views", "MainWindow.axaml");
        var document = XDocument.Load(path);
        XNamespace avalonia = "https://github.com/avaloniaui";

        var languageGrid = document.Descendants(avalonia + "Grid")
            .FirstOrDefault(element =>
                (string?)element.Attribute("ColumnDefinitions") == "30,18,175");
        Require(languageGrid is not null,
            "Language selector needs reserved vector icon / spacer / ComboBox columns");

        var globe = languageGrid!.Elements(avalonia + "Canvas").FirstOrDefault(element =>
            (string?)element.Attribute("Grid.Column") == "0");
        Require(globe is not null &&
                globe.Elements(avalonia + "Ellipse").Count() == 2 &&
                globe.Elements(avalonia + "Line").Count() == 3,
            "Language globe must be a complete vector, not a clipped emoji glyph");

        Require(languageGrid.Elements(avalonia + "ComboBox").Any(element =>
                (string?)element.Attribute("Grid.Column") == "2"),
            "Language ComboBox must stay outside the globe and its spacer");
    }

    /// <summary>
    /// Keep Deno visible alongside yt-dlp and FFmpeg in both the header and
    /// tools page. This guards status bindings and the explicit install action.
    /// </summary>
    private static void VerifyDenoUiLayout(string root)
    {
        XNamespace avalonia = "https://github.com/avaloniaui";
        var header = XDocument.Load(Path.Combine(root, "src", "YtDlpUi", "Views", "MainWindow.axaml"));
        var toolPage = XDocument.Load(Path.Combine(root, "src", "YtDlpUi", "Views", "Pages", "ToolsPage.axaml"));

        static bool HasBinding(XDocument doc, string property) =>
            doc.Descendants().Attributes().Any(attribute =>
                attribute.Value.Contains("Toolchain." + property, StringComparison.Ordinal));

        Require(HasBinding(header, "DenoAvailable") && HasBinding(header, "DenoLocationText"),
            "Deno must have a status indicator and tooltip in the desktop header");
        Require(HasBinding(toolPage, "DenoAvailable") && HasBinding(toolPage, "DenoVersionText") &&
                HasBinding(toolPage, "DenoLocationText") && HasBinding(toolPage, "InstallDenoCommand"),
            "Tools must show Deno status, version, path and an installation command");
        Require(toolPage.Descendants(avalonia + "TextBlock").Any(element =>
                (string?)element.Attribute("Text") == "Deno"),
            "Tools must contain a titled Deno section");
    }

    /// <summary>
    /// Guard the playlist preview against a restrictive DataType matcher.
    /// The directly assigned ItemTemplate must apply to every playlist entry.
    /// </summary>
    private static void VerifyPlaylistPreviewTemplate(string root)
    {
        XNamespace avalonia = "https://github.com/avaloniaui";
        var path = Path.Combine(root, "src", "YtDlpUi", "Views", "Pages", "DownloadPage.axaml");
        var view = XDocument.Load(path);
        var playlist = view.Descendants(avalonia + "ListBox").SingleOrDefault(element =>
            ((string?)element.Attribute("ItemsSource"))?.Contains("PlaylistEntries", StringComparison.Ordinal) == true);

        Require(playlist is not null, "Download page must show the analysed playlist entries");
        var template = playlist!.Descendants(avalonia + "DataTemplate").SingleOrDefault();
        // When ItemTemplate is set directly it should not have a DataType matcher.
        // The type matcher previously prevented item rendering in packaged builds.
        Require(template is not null && template.Attribute("DataType") is null,
            "Explicit playlist ItemTemplate must be unconditional (no DataType selector)");

        var bindings = template!.Descendants(avalonia + "TextBlock")
            .Attributes("Text").Select(attribute => attribute.Value).ToArray();
        Require(bindings.Any(binding => binding.Contains("Binding Display", StringComparison.Ordinal)) &&
                bindings.Any(binding => binding.Contains("Binding Duration", StringComparison.Ordinal)),
            "Playlist preview must display MediaEntry.Display and formatted Duration");
    }

    private static void ValidateLocalLinks(string markdown)
    {
        var content = File.ReadAllText(markdown);
        // Cover Markdown images and links. Ignore web links and in-page anchors.
        foreach (Match match in Regex.Matches(content, @"\]\(([^)\s]+)\)"))
        {
            var target = match.Groups[1].Value;
            if (target.Length == 0 || target.StartsWith('#') ||
                Uri.TryCreate(target, UriKind.Absolute, out _))
            {
                continue;
            }

            var withoutAnchor = target.Split('#', 2)[0];
            var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(markdown)!, withoutAnchor));
            Require(File.Exists(fullPath), $"Broken local link {target} in {markdown}");
        }
    }

    private static string? FindRepositoryRoot()
    {
        for (var path = new DirectoryInfo(Environment.CurrentDirectory); path is not null; path = path.Parent)
        {
            if (File.Exists(Path.Combine(path.FullName, "YtDlpUi.slnx")))
            {
                return path.FullName;
            }
        }

        return null;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException("DOCUMENTATION TEST FAILED: " + message);
        }
    }
}
