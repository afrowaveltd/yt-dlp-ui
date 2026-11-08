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

var ffmpegPath = Path.Combine("somewhere", "ffmpeg");
var options = new DownloadOptions { FfmpegLocation = ffmpegPath };
var argumentVector = YtDlpArgumentBuilder.Build(options, ["https://example.org/video"]);
var index = argumentVector.IndexOf("--ffmpeg-location");
Require(index >= 0 && argumentVector[index + 1] == ffmpegPath, "FFmpeg location argument");
Require(argumentVector[^1] == "https://example.org/video", "URL is the final argument");

Console.WriteLine("All localization and command-line smoke tests passed.");
