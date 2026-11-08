using YtDlpUi.Core.Localization;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException("SMOKE TEST FAILED: " + message);
    }
}

var localization = SimpleLocalizer.Instance;
localization.SetLanguage("en");
Require(localization.Translate("Download") == "Download", "English dictionaryless fallback");
var observable = localization.For("Download");
Require(observable.Value == "Download", "English observable translation");
var updates = 0;
observable.PropertyChanged += (_, propertyChange) =>
{
    if (propertyChange.PropertyName == nameof(LocalizedText.Value))
    {
        updates++;
    }
};

localization.SetLanguage("cs");
Require(localization.Translate("Download") == "Stáhnout", "Czech translation");
Require(observable.Value == "Stáhnout" && updates > 0, "Live translation update");
Require(localization.Translate("Untranslated English phrase") == "Untranslated English phrase",
    "Missing translation fallback");
localization.SetLanguage("fr-FR");
Require(localization.Language == "en", "Unsupported language fallback");
Require(observable.Value == "Download", "Observable fallback after switching");

// Adding a locale file must make it selectable without recompilation or restart.
var localeFolder = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-locale-test-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(localeFolder);
try
{
    var dynamicLocalizer = new SimpleLocalizer(localeFolder);
    Require(dynamicLocalizer.AvailableLanguages.Any(item => item.Code == "en"),
        "English fallback must always be selectable");
    Require(dynamicLocalizer.AvailableLanguages.Any(item => item.Code == "cs"),
        "Bundled Czech dictionary must be discovered");
    Require(!dynamicLocalizer.AvailableLanguages.Any(item => item.Code == "pt-BR"),
        "Test locale should not yet exist");

    File.WriteAllText(Path.Combine(localeFolder, "pt-BR.json"),
        "{\"Download\":\"Baixar\",\"About\":\"Sobre\"}");
    File.WriteAllText(Path.Combine(localeFolder, "fr.json"),
        "{\"Download\":\"Télécharger\"}");
    File.WriteAllText(Path.Combine(localeFolder, "de.json"), "{ invalid json");
    dynamicLocalizer.RefreshLanguages();

    Require(dynamicLocalizer.AvailableLanguages.Any(item => item.Code == "pt-BR"),
        "New regional JSON locale should appear in the language menu");
    Require(dynamicLocalizer.AvailableLanguages.Any(item => item.Code == "fr"),
        "New language should appear in the language menu");
    Require(!dynamicLocalizer.AvailableLanguages.Any(item => item.Code == "de"),
        "Invalid JSON locale should not be displayed");

    var dynamicLabel = dynamicLocalizer.For("Download");
    dynamicLocalizer.SetLanguage("pt-BR");
    Require(dynamicLocalizer.Language == "pt-BR" && dynamicLabel.Value == "Baixar",
        "Full regional language code must be preserved and translated");
    Require(dynamicLocalizer.Translate("Missing key") == "Missing key",
        "New locale must preserve English fallback");

    dynamicLocalizer.SetLanguage("fr-CA");
    Require(dynamicLocalizer.Language == "fr" && dynamicLabel.Value == "Télécharger",
        "Parent culture should be used when full regional locale is absent");

    File.WriteAllText(Path.Combine(localeFolder, "fr.json"), "{\"Download\":\"Télécharger encore\"}");
    dynamicLocalizer.RefreshLanguages();
    Require(dynamicLabel.Value == "Télécharger encore",
        "Opening language menu should reload edited JSON translations");

    File.WriteAllText(Path.Combine(localeFolder, "cs.json"), "{\"Download\":\"Stáhnout vlastní\"}");
    dynamicLocalizer.RefreshLanguages();
    dynamicLocalizer.SetLanguage("cs");
    Require(dynamicLocalizer.Translate("Download") == "Stáhnout vlastní",
        "User-provided dictionary should override the bundled one");

    File.Delete(Path.Combine(localeFolder, "fr.json"));
    dynamicLocalizer.RefreshLanguages();
    Require(!dynamicLocalizer.AvailableLanguages.Any(item => item.Code == "fr"),
        "Removed custom language should disappear from the menu");
}
finally
{
    Directory.Delete(localeFolder, recursive: true);
}

var ffmpegPath = Path.Combine("somewhere", "ffmpeg");
var options = new DownloadOptions { FfmpegLocation = ffmpegPath };
var argumentVector = YtDlpArgumentBuilder.Build(options, ["https://example.org/video"]);
var index = argumentVector.IndexOf("--ffmpeg-location");
Require(index >= 0 && argumentVector[index + 1] == ffmpegPath, "FFmpeg location argument");
Require(argumentVector[^1] == "https://example.org/video", "URL is the final argument");

DocumentationChecks.Verify();
Console.WriteLine("All localization and command-line smoke tests passed.");
