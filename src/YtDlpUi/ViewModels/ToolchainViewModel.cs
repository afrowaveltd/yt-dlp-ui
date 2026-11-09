using System.Text;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.ViewModels;

/// <summary>Detects, installs and updates yt-dlp, FFmpeg and Deno tools.</summary>
public sealed partial class ToolchainViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly YtDlpInstaller _installer;
    private readonly FfmpegInstaller _ffmpegInstaller = new();
    private readonly DenoInstaller _denoInstaller = new();
    private readonly StringBuilder _log = new();
    private CancellationTokenSource? _installCts;

    public ToolchainViewModel(AppSettings settings)
    {
        _settings = settings;
        _installer = new YtDlpInstaller();
    }

    /// <summary>Raised whenever the resolved yt-dlp command changes.</summary>
    public event EventHandler? CommandChanged;

    /// <summary>The runnable yt-dlp command, or null when it is not available.</summary>
    public YtDlpCommand? Command { get; private set; }

    /// <summary>True when yt-dlp can be executed right now.</summary>
    [ObservableProperty] public partial bool YtDlpAvailable { get; private set; }

    /// <summary>Version string reported by yt-dlp.</summary>
    [ObservableProperty] public partial string YtDlpVersionText { get; private set; } = "–";

    /// <summary>Path or launcher description of yt-dlp.</summary>
    [ObservableProperty] public partial string YtDlpLocationText { get; private set; } = "hledám…";

    /// <summary>True when ffmpeg is available (needed for merging and conversion).</summary>
    [ObservableProperty] public partial bool FfmpegAvailable { get; private set; }

    /// <summary>ffmpeg version line.</summary>
    [ObservableProperty] public partial string FfmpegVersionText { get; private set; } = "–";

    /// <summary>Path of ffmpeg.</summary>
    [ObservableProperty] public partial string FfmpegLocationText { get; private set; } = "hledám…";

    /// <summary>True if Deno 2.3+ is installed and can run.</summary>
    [ObservableProperty] public partial bool DenoAvailable { get; private set; }

    /// <summary>Detected Deno version or a missing-version marker.</summary>
    [ObservableProperty] public partial string DenoVersionText { get; private set; } = "–";

    /// <summary>Path to the detected Deno JavaScript runtime.</summary>
    [ObservableProperty] public partial string DenoLocationText { get; private set; } = "searching…";

    /// <summary>True while a detection or install operation runs.</summary>
    [ObservableProperty] public partial bool IsBusy { get; private set; }

    /// <summary>Text of the install / update activity log.</summary>
    [ObservableProperty] public partial string InstallLogText { get; private set; } = string.Empty;

    /// <summary>Progress of a binary download, or null when indeterminate.</summary>
    [ObservableProperty] public partial double? InstallProgress { get; private set; }

    /// <summary>Hint about a newer yt-dlp release.</summary>
    [ObservableProperty] public partial string? UpdateHint { get; private set; }

    /// <summary>True if the official Windows x64 FFmpeg ZIP can be installed.</summary>
    public bool CanInstallFfmpeg => FfmpegInstaller.IsSupported;

    /// <summary>True when the current OS/architecture supports managed Deno installation.</summary>
    public bool CanInstallDeno => DenoInstaller.IsSupported;

    /// <summary>Whether the suggested JavaScript runtime is absent.</summary>
    public bool DenoMissing => !DenoAvailable;

    /// <summary>True when yt-dlp is missing, so the install card should be prominent.</summary>
    public bool YtDlpMissing => !YtDlpAvailable;

    /// <summary>Convenience flag for enabling download buttons.</summary>
    public bool CanDownload => YtDlpAvailable && !IsBusy;

    /// <summary>Probe the system for yt-dlp, FFmpeg and Deno.</summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        try
        {
            var ytDlpStatus = await ToolLocator
                .LocateYtDlpAsync(_settings.YtDlpPath, cancellationToken)
                .ConfigureAwait(true);

            var command = ytDlpStatus.Found
                ? await ToolLocator.ResolveYtDlpCommandAsync(_settings.YtDlpPath, cancellationToken).ConfigureAwait(true)
                : null;

            Command = command;
            YtDlpAvailable = command is not null;
            YtDlpVersionText = ytDlpStatus.Version ?? "–";
            YtDlpLocationText = ytDlpStatus.Path ?? "nenalezeno";

            var ffmpegStatus = await ToolLocator
                .LocateFfmpegAsync(_settings.FfmpegPath, cancellationToken)
                .ConfigureAwait(true);

            FfmpegAvailable = ffmpegStatus.Found;
            FfmpegVersionText = ffmpegStatus.Version ?? "–";
            FfmpegLocationText = ffmpegStatus.Path ?? "nenalezeno";

            var denoStatus = await DenoInstaller.LocateAsync(cancellationToken).ConfigureAwait(true);
            DenoAvailable = denoStatus.Found;
            DenoVersionText = denoStatus.Version ?? "–";
            DenoLocationText = denoStatus.Path ?? denoStatus.Message ?? "not found";

            CommandChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(DenoMissing));
            OnPropertyChanged(nameof(YtDlpMissing));
            OnPropertyChanged(nameof(CanDownload));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Re-probe the toolchain from a button command.</summary>
    [RelayCommand]
    private Task Refresh() => RefreshAsync();

    /// <summary>Download the official standalone binary into <c>~/.local/bin</c>.</summary>
    [RelayCommand]
    private async Task InstallStandaloneAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ClearLog();
        AppendLog("Stahuji oficiální yt-dlp z GitHubu…");

        try
        {
            var progress = new Progress<DownloadProgress>(p =>
            {
                InstallProgress = p.Percent;
                if (p.TotalBytes is > 0)
                {
                    AppendLog($"  {Formatting.Bytes(p.ReceivedBytes)} / {Formatting.Bytes(p.TotalBytes)}");
                }
            });

            var path = await _installer.DownloadStandaloneAsync(null, progress).ConfigureAwait(true);
            AppendLog($"Uloženo do {path}");

            _settings.YtDlpPath = path;
            await RefreshAsync().ConfigureAwait(true);

            AppendLog(YtDlpAvailable
                ? $"Hotovo – nainstalováno {YtDlpVersionText}."
                : "Stažení proběhlo, ale yt-dlp se stále nespustí. Zkontrolujte prosím oprávnění.");
        }
        catch (Exception ex)
        {
            AppendLog($"Instalace se nezdařila: {ex.Message}");
        }
        finally
        {
            InstallProgress = null;
            IsBusy = false;
            OnPropertyChanged(nameof(CanDownload));
        }
    }

    /// <summary>Install a verified FFmpeg/FFprobe pair into the user's private folder.</summary>
    [RelayCommand]
    private async Task InstallFfmpegAsync()
    {
        if (IsBusy || !CanInstallFfmpeg)
        {
            return;
        }

        IsBusy = true;
        ClearLog();
        InstallProgress = 0;
        _installCts?.Dispose();
        _installCts = new CancellationTokenSource();
        try
        {
            AppendLog("Downloading and verifying FFmpeg for Windows…");
            var progress = new Progress<DownloadProgress>(p => InstallProgress = p.Percent);
            var path = await _ffmpegInstaller.InstallAsync(progress, _installCts.Token).ConfigureAwait(true);
            _settings.FfmpegPath = path;
            await RefreshAsync().ConfigureAwait(true);
            AppendLog(FfmpegAvailable ? "FFmpeg and ffprobe installed successfully." : "FFmpeg was installed but could not be started.");
        }
        catch (OperationCanceledException)
        {
            AppendLog("FFmpeg installation cancelled.");
        }
        catch (Exception ex)
        {
            AppendLog($"FFmpeg installation failed: {ex.Message}");
        }
        finally
        {
            InstallProgress = null;
            IsBusy = false;
            OnPropertyChanged(nameof(CanDownload));
        }
    }

    /// <summary>
    /// Download and install SHA-256-verified Deno in the user's private folder.
    /// This is supported on Windows and Linux and never asks for administrator rights.
    /// </summary>
    [RelayCommand]
    private async Task InstallDenoAsync()
    {
        if (IsBusy || !CanInstallDeno)
        {
            return;
        }

        IsBusy = true;
        ClearLog();
        InstallProgress = 0;
        _installCts?.Dispose();
        _installCts = new CancellationTokenSource();
        try
        {
            AppendLog("Downloading and verifying the official Deno JavaScript runtime…");
            var progress = new Progress<DownloadProgress>(p => InstallProgress = p.Percent);
            var path = await _denoInstaller.InstallAsync(progress, _installCts.Token).ConfigureAwait(true);
            AppendLog($"Deno installed to {path}");
            await RefreshAsync().ConfigureAwait(true);
            AppendLog(DenoAvailable ? $"Deno {DenoVersionText} is ready for YouTube."
                : "Deno was installed but could not be started.");
        }
        catch (OperationCanceledException)
        {
            AppendLog("Deno installation cancelled.");
        }
        catch (Exception ex)
        {
            AppendLog($"Deno installation failed: {ex.Message}");
        }
        finally
        {
            InstallProgress = null;
            IsBusy = false;
            OnPropertyChanged(nameof(CanDownload));
        }
    }

    /// <summary>Install or upgrade yt-dlp through pipx or pip.</summary>
    [RelayCommand]
    private async Task InstallViaPipAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ClearLog();
        AppendLog("Instaluji yt-dlp přes Python…");

        try
        {
            var exitCode = await _installer
                .InstallViaPipAsync(AppendLog, CancellationToken.None)
                .ConfigureAwait(true);

            AppendLog(exitCode == 0
                ? "Instalace dokončena."
                : $"Instalace skončila s kódem {exitCode}. Zkuste prosím ruční instalaci podle README.");

            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AppendLog($"Instalace se nezdařila: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(CanDownload));
        }
    }

    /// <summary>Run <c>yt-dlp --update</c>.</summary>
    [RelayCommand]
    private async Task UpdateYtDlpAsync()
    {
        if (IsBusy || Command is null)
        {
            return;
        }

        IsBusy = true;
        ClearLog();
        AppendLog("Spouštím yt-dlp --update…");

        _installCts?.Dispose();
        _installCts = new CancellationTokenSource();

        try
        {
            var exitCode = await _installer
                .UpdateAsync(Command, AppendLog, _installCts.Token)
                .ConfigureAwait(true);

            if (exitCode != 0)
            {
                AppendLog($"Aktualizace neproběhla (kód {exitCode}).");
                AppendLog("Pokud je yt-dlp nainstalováno přes pip, použijte prosím tlačítko \"Instalovat přes pip\".");
            }

            await RefreshAsync().ConfigureAwait(true);
            UpdateHint = null;
        }
        catch (OperationCanceledException)
        {
            AppendLog("Aktualizace zrušena.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Ask GitHub whether a newer release exists.</summary>
    [RelayCommand]
    private async Task CheckLatestAsync()
    {
        var latest = await _installer.GetLatestReleaseVersionAsync().ConfigureAwait(true);
        if (latest is null)
        {
            UpdateHint = null;
            return;
        }

        var current = YtDlpVersionText;
        UpdateHint = string.Equals(current, latest, StringComparison.OrdinalIgnoreCase)
            ? null
            : $"K dispozici je novější verze {latest} (nainstalováno: {current}).";
    }

    /// <summary>Stop a running install / update.</summary>
    public void CancelRunning()
    {
        try
        {
            _installCts?.Cancel();
        }
        catch (Exception)
        {
            // Nothing to cancel.
        }
    }

    /// <summary>Append a line to the activity log, marshalled onto the UI thread.</summary>
    public void AppendLog(string line)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => AppendLog(line));
            return;
        }

        _log.AppendLine(line);
        if (_log.Length > 20000)
        {
            _log.Remove(0, _log.Length - 16000);
        }

        InstallLogText = _log.ToString();
    }

    private void ClearLog()
    {
        _log.Clear();
        InstallLogText = string.Empty;
    }
}
