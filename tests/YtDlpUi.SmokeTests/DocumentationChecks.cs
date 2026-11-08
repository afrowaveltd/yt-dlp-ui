using System.Text.RegularExpressions;

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
        Require(rootReadme.Contains("0.1.0", StringComparison.Ordinal), "README version is missing");
        Require(!rootReadme.Contains("1.0.0", StringComparison.Ordinal), "README still refers to 1.0.0");

        var props = File.ReadAllText(Path.Combine(root, "Directory.Build.props"));
        var setup = File.ReadAllText(Path.Combine(root, "packaging", "windows", "setup.iss"));
        Require(props.Contains("<Version>0.1.0</Version>", StringComparison.Ordinal), "App version must be 0.1.0");
        Require(setup.Contains("#define AppVersion \"0.1.0\"", StringComparison.Ordinal),
            "Installer fallback version must match");

        var markdownFiles = topics.SelectMany(topic => Directory.EnumerateFiles(topic, "*.md"))
            .Append(Path.Combine(root, "README.md"));

        foreach (var markdown in markdownFiles)
        {
            ValidateLocalLinks(markdown);
        }

        Console.WriteLine($"Documentation smoke checks passed: {topics.Length} bilingual topics and local links.");
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
