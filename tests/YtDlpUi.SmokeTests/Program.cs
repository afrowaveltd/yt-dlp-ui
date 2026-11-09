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

// Android URL intake and strict direct-file detection.
Require(MobileLinkParser.TryExtract("Shared: https://media.example.net/clip.mp4", out var shared) &&
    shared == "https://media.example.net/clip.mp4", "Extract an Android shared link");
Require(!MobileLinkParser.TryExtract("no media URL", out _), "Reject invalid share text");
Require(MobileLinkParser.TryGetDirectMedia("https://media.example.net/video.mp4?token=abc",
    out var mediaUri, out var safeName) &&
    mediaUri is not null && safeName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase),
    "Accept HTTPS direct video files");
Require(MobileLinkParser.TryGetDirectMedia("https://media.example.net/song.mp3",
    out _, out _), "Accept HTTPS direct audio files");
Require(!MobileLinkParser.TryGetDirectMedia("https://youtube.com/watch?v=123",
    out _, out _), "Video websites require a genuine Android yt-dlp backend");
Require(!MobileLinkParser.TryGetDirectMedia("http://media.example.net/video.mp4",
    out _, out _), "Reject insecure HTTP");
Require(!MobileLinkParser.TryGetDirectMedia("file:///tmp/clip.mp4",
    out _, out _), "Reject local file URIs");
Require(!MobileLinkParser.TryGetDirectMedia("https://media.example.net/installer.exe",
    out _, out _), "Reject nonmedia extension");
// Mobile single-stream format choices are explicit and do not depend on FFmpeg.
Require(MobileDownloadFormats.For(MobileDownloadQuality.Best) == "b",
    "Android best preset uses a single audio/video stream");
Require(MobileDownloadFormats.For(MobileDownloadQuality.UpTo720p) == "b[height<=720]/b",
    "720p preset has a compatible fallback");
Require(MobileDownloadFormats.For(MobileDownloadQuality.UpTo480p) == "b[height<=480]/b",
    "480p preset has a compatible fallback");
Require(MobileDownloadFormats.For(MobileDownloadQuality.AudioOriginal) == "ba",
    "Audio-only preset downloads existing audio, without transcoding");

// Deno used by YouTube challenge solver must be explicitly routed when outside PATH.
// Test formatting without running untrusted binaries or making network calls.
var denoDir = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-deno test " + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(denoDir);
try
{
    var deno = Path.Combine(denoDir, OperatingSystem.IsWindows() ? "deno.exe" : "deno");
    File.WriteAllText(deno, "placeholder for a packaged binary");
    var denoRuntime = JavaScriptRuntimeLocator.ToArgument(deno);
    Require(denoRuntime == "deno:" + Path.GetFullPath(deno),
        "Deno runtime paths (including whitespace) must be passed as one yt-dlp argument");
    Require(JavaScriptRuntimeLocator.ToArgument(Path.Combine(denoDir, "missing.exe")) is null,
        "Missing Deno executables must not be advertised");
    Require(JavaScriptRuntimeLocator.ResolveArgument("node", null) == "node",
        "Manual JavaScript runtime setting takes precedence");
    Require(JavaScriptRuntimeLocator.ResolveArgument(null, "--js-runtimes node") is null,
        "Advanced user --js-runtimes must not be overridden");
    Require(JavaScriptRuntimeLocator.ResolveArgument(null, "--js-runtimes=node") is null,
        "Equals-style --js-runtimes must not be overridden");
    Require(JavaScriptRuntimeLocator.ResolveArgument(null, "--no-js-runtimes") is null,
        "Explicit disabling of JavaScript must be respected");

    var manualRuntimeOptions = new DownloadOptions { JsRuntimes = "node" };
    var manualDownload = YtDlpArgumentBuilder.Build(manualRuntimeOptions, ["https://example.org/watch"]);
    var manualAnalysis = YtDlpArgumentBuilder.BuildAnalysisArguments(
        manualRuntimeOptions, "https://example.org/watch", false);
    Require(manualDownload.Contains("--js-runtimes") &&
            manualDownload[manualDownload.IndexOf("--js-runtimes") + 1] == "node",
        "Download must pass manual JavaScript runtime through to yt-dlp");
    Require(manualAnalysis.Contains("--js-runtimes") &&
            manualAnalysis[manualAnalysis.IndexOf("--js-runtimes") + 1] == "node",
        "Analysis and download must use the same JavaScript runtime option");
}
finally
{
    Directory.Delete(denoDir, recursive: true);
}

// Linux runtime discovery must also work from a GUI session without .bashrc PATH.
if (OperatingSystem.IsLinux())
{
    var originalInstall = Environment.GetEnvironmentVariable("DENO_INSTALL");
    var originalPath = Environment.GetEnvironmentVariable("PATH");
    var prefix = Path.Combine(Path.GetTempPath(), "afrowave-deno-discovery-" + Guid.NewGuid().ToString("N"));
    try
    {
        var bin = Path.Combine(prefix, "bin");
        Directory.CreateDirectory(bin);
        var executable = Path.Combine(bin, "deno");
        File.WriteAllText(executable, "test fixture");
        File.SetUnixFileMode(executable, UnixFileMode.UserRead | UnixFileMode.UserWrite |
            UnixFileMode.UserExecute);
        Environment.SetEnvironmentVariable("DENO_INSTALL", prefix);
        Environment.SetEnvironmentVariable("PATH", "/usr/bin:/bin");
        Require(JavaScriptRuntimeLocator.FindDenoExecutable() == Path.GetFullPath(executable),
            "Linux GUI should find Deno in DENO_INSTALL even when PATH does not include it");
        File.SetUnixFileMode(executable, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        Require(JavaScriptRuntimeLocator.FindDenoExecutable() != Path.GetFullPath(executable),
            "Linux discovery must ignore a non-executable file");
    }
    finally
    {
        Environment.SetEnvironmentVariable("DENO_INSTALL", originalInstall);
        Environment.SetEnvironmentVariable("PATH", originalPath);
        Directory.Delete(prefix, recursive: true);
    }
}

// Accept only a properly matched, unambiguous vendor checksum.
var checksumAsset = DenoInstaller.AssetName();
if (DenoInstaller.IsSupported)
{
    Require(checksumAsset is not null, "Supported OS must choose an official Deno ZIP");
    const string digest = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";
    Require(DenoInstaller.PublishedSha256(digest + "  " + checksumAsset, checksumAsset!) == digest,
        "Deno installer must read matching named publisher checksum");
    Require(DenoInstaller.PublishedSha256(digest, checksumAsset!) == digest,
        "Deno installer must also accept a bare official SHA-256 sidecar");
    bool rejected = false;
    try
    {
        DenoInstaller.PublishedSha256(digest + "  wrong.zip", checksumAsset!);
    }
    catch (InvalidDataException)
    {
        rejected = true;
    }

    Require(rejected, "Never accept an unrelated Deno package checksum");
    rejected = false;
    try
    {
        DenoInstaller.PublishedSha256(digest + "\n" + digest, checksumAsset!);
    }
    catch (InvalidDataException)
    {
        rejected = true;
    }

    Require(rejected, "Duplicate checksum entries must fail closed");
}

await HardeningSmokeChecks.VerifyAsync();
DocumentationChecks.Verify();
Console.WriteLine("All localization and command-line smoke tests passed.");
