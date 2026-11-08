using System.Net;
using System.Security.Cryptography;
using System.Text;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

/// <summary>Offline regression checks for 0.1.1 reliability/security fixes.</summary>
internal static class HardeningSmokeChecks
{
    public static async Task VerifyAsync()
    {
        VerifyQueueSnapshot();
        VerifyDirectorySwap();
        VerifySettingsSave();
        await VerifyStandaloneInstallerAsync();
        Console.WriteLine("0.1.1 hardening smoke checks passed.");
    }

    private static void VerifyQueueSnapshot()
    {
        var live = new DownloadOptions
        {
            Format = "bv*+ba/b",
            OutputDirectory = "/test/first",
            Password = "transient-secret",
            ExtraArguments = "--socket-timeout 15"
        };
        var queued = live.Snapshot();
        live.Format = "ba";
        live.OutputDirectory = "/test/second";
        live.Password = "changed";
        live.ExtraArguments = "--socket-timeout 30";

        Require(!ReferenceEquals(queued, live), "Queue options must be independent objects");
        Require(queued.Format == "bv*+ba/b" && queued.OutputDirectory == "/test/first",
            "Editing UI options must not alter previously queued format/output");
        Require(queued.Password == "transient-secret" && queued.ExtraArguments == "--socket-timeout 15",
            "One in-memory queue item must retain its transient credentials and parameters");
    }

    private static void VerifyDirectorySwap()
    {
        var root = NewTempDirectory();
        try
        {
            var target = Path.Combine(root, "ffmpeg");
            var stage = Path.Combine(root, "staged");
            Directory.CreateDirectory(target);
            Directory.CreateDirectory(stage);
            File.WriteAllText(Path.Combine(target, "ffmpeg.exe"), "old working executable");
            File.WriteAllText(Path.Combine(stage, "ffmpeg.exe"), "new verified executable");
            FileDeployment.ReplaceDirectory(stage, target);
            Require(File.ReadAllText(Path.Combine(target, "ffmpeg.exe")) == "new verified executable",
                "Verified installation must replace existing version");
            Require(!Directory.Exists(stage), "Staging directory must be moved into destination");
            Require(!Directory.EnumerateDirectories(root, ".yt-dlp-ui-backup-*").Any(),
                "Successful deployment should clean old backup");

            bool rejected = false;
            try
            {
                FileDeployment.ReplaceDirectory(Path.Combine(root, "missing-stage"), target);
            }
            catch (DirectoryNotFoundException)
            {
                rejected = true;
            }

            Require(rejected, "Invalid staging must fail");
            Require(File.ReadAllText(Path.Combine(target, "ffmpeg.exe")) == "new verified executable",
                "Invalid staging must preserve existing FFmpeg");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static void VerifySettingsSave()
    {
        var root = NewTempDirectory();
        try
        {
            var path = Path.Combine(root, "config", "settings.json");
            var store = new SettingsStore(path);
            Require(store.Save(new AppSettings { Language = "en", DarkTheme = true }),
                "Initial settings file creation");
            Require(store.Save(new AppSettings { Language = "cs", DarkTheme = false }),
                "Atomic settings replacement");
            var loaded = store.Load();
            Require(loaded.Language == "cs" && !loaded.DarkTheme,
                "Latest complete settings must be loadable");
            Require(!Directory.EnumerateFiles(Path.GetDirectoryName(path)!, "*.tmp").Any(),
                "Atomic save must leave no temporary file");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task VerifyStandaloneInstallerAsync()
    {
        var asset = YtDlpInstaller.StandaloneAssetName();
        var data = Encoding.UTF8.GetBytes("test binary payload");
        var digest = Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
        var manifest = $"{digest}  {asset}\n{new string('a', 64)}  another-file\n";

        Require(YtDlpInstaller.ReadPublishedChecksum(manifest, asset) == digest,
            "Installer must select exact publisher checksum by asset name");
        Require(YtDlpInstaller.ReadPublishedChecksum(manifest, "another-file") == new string('a', 64),
            "Checksums cannot be selected by order");
        bool ambiguous = false;
        try
        {
            YtDlpInstaller.ReadPublishedChecksum(manifest + manifest, asset);
        }
        catch (InvalidDataException)
        {
            ambiguous = true;
        }

        Require(ambiguous, "Duplicate manifest entries must be rejected");

        var root = NewTempDirectory();
        try
        {
            var installed = Path.Combine(root, OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp");
            var oldBytes = Encoding.UTF8.GetBytes("old previously installed file");
            File.WriteAllBytes(installed, oldBytes);

            using (var client = new HttpClient(new FakeReleaseHandler(manifest, data)))
            using (var installer = new YtDlpInstaller(client))
            {
                await installer.DownloadStandaloneAsync(installed);
            }

            Require(File.ReadAllBytes(installed).SequenceEqual(data),
                "Verified yt-dlp update should replace the old executable");

            var broken = $"{new string('0', 64)}  {asset}\n";
            bool rejected = false;
            try
            {
                using var client = new HttpClient(new FakeReleaseHandler(broken, oldBytes));
                using var installer = new YtDlpInstaller(client);
                await installer.DownloadStandaloneAsync(installed);
            }
            catch (InvalidDataException)
            {
                rejected = true;
            }

            Require(rejected, "SHA-256 mismatch must reject the downloaded file");
            Require(File.ReadAllBytes(installed).SequenceEqual(data),
                "Checksum failure must leave the installed tool intact");
            Require(!Directory.EnumerateFiles(root, "*.download").Any(),
                "Failed installation must remove its temporary download");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string NewTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-0.1.1-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException("HARDENING TEST FAILED: " + message);
        }
    }

    private sealed class FakeReleaseHandler(string manifest, byte[] payload) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var isChecksum = request.RequestUri?.AbsolutePath.EndsWith(
                "/SHA2-256SUMS", StringComparison.OrdinalIgnoreCase) ?? false;
            var bytes = isChecksum ? Encoding.UTF8.GetBytes(manifest) : payload;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            });
        }
    }
}
