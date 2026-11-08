using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Localization;
using YtDlpUi.Core.Services;

namespace YtDlpUi.ViewModels;

/// <summary>Coordinates the whole window: link input, analysis, presets, queue, log and settings.</summary>
public sealed partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private const int MaxLogLines = 3000;

    private readonly SettingsStore _settingsStore;
    private readonly MetadataService _metadataService = new();
    private readonly HttpClient _httpClient = new();
    private readonly object _queueLock = new();

    private readonly DispatcherTimer _autosaveTimer;
    private CancellationTokenSource? _analysisCts;
    private CancellationTokenSource? _queueCts;
    private Task? _queueTask;
    private bool _suppressPresetSync;
    private bool _disposed;

    public MainWindowViewModel()
    {
        _settingsStore = new SettingsStore();
        Settings = _settingsStore.Load();
        SimpleLocalizer.Instance.SetLanguage(Settings.Language);
        AvailableLanguages = SimpleLocalizer.Instance.AvailableLanguages;
        _selectedLanguage = AvailableLanguages.FirstOrDefault(item =>
            string.Equals(item.Code, SimpleLocalizer.Instance.Language, StringComparison.OrdinalIgnoreCase));

        Options = (Settings.RememberOptions
                      ? SettingsStore.RestoreOptions(Settings.LastOptionsJson)
                      : null)
                  ?? new DownloadOptions();

        if (string.IsNullOrWhiteSpace(Options.OutputDirectory))
        {
            Options.OutputDirectory = string.IsNullOrWhiteSpace(Settings.DefaultOutputDirectory)
                ? UserPaths.DefaultDownloadDirectory
                : Settings.DefaultOutputDirectory;
        }

        if (string.IsNullOrWhiteSpace(Options.OutputTemplate))
        {
            Options.OutputTemplate = DownloadOptions.DefaultOutputTemplate;
        }

        Toolchain = new ToolchainViewModel(Settings);
        Toolchain.CommandChanged += (_, _) => OnToolchainChanged();
        Settings.PropertyChanged += OnSettingsChanged;
        Options.PropertyChanged += OnOptionsChanged;

        // Options change on every keystroke, so persist them once the user pauses
        // instead of only on a graceful window close.
        _autosaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
        _autosaveTimer.Tick += (_, _) =>
        {
            _autosaveTimer.Stop();
            SaveSettings();
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; yt-dlp-gui/1.0)");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        _suppressPresetSync = true;
        SelectedPreset = MatchPresetChoice(Options.Preset);
        _suppressPresetSync = false;

        AddLog("Vítejte! Vložte odkaz na video a stiskněte „Analyzovat“ nebo rovnou „Stáhnout“.", LogKind.App);

        // Apply the remembered theme before the first window is shown.
        ApplyTheme();

        // Make sure the configuration file exists right away so it can be inspected or edited.
        ScheduleAutosave();

        _ = InitializeAsync();
    }

    // ───────────────────────────── state ─────────────────────────────

    /// <summary>Persisted application preferences.</summary>
    public AppSettings Settings { get; }

    /// <summary>All yt-dlp download options, bound directly by the option pages.</summary>
    public DownloadOptions Options { get; }

    /// <summary>yt-dlp / ffmpeg detection and installation.</summary>
    public ToolchainViewModel Toolchain { get; }

    private LanguageOption? _selectedLanguage;

    /// <summary>Languages discovered from JSON files in Locales (no hard-coded list).</summary>
    public IReadOnlyList<LanguageOption> AvailableLanguages { get; private set; }

    /// <summary>The language selected in the header; changing it updates the saved setting.</summary>
    public LanguageOption? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value) && value is not null &&
                !string.Equals(Settings.Language, value.Code, StringComparison.OrdinalIgnoreCase))
            {
                Settings.Language = value.Code;
            }
        }
    }

    /// <summary>
    /// Re-scan Locales whenever the header dropdown opens. New JSON dictionaries become
    /// selectable immediately, including those added while the application is running.
    /// </summary>
    public void RefreshLanguages()
    {
        SimpleLocalizer.Instance.RefreshLanguages();
        AvailableLanguages = SimpleLocalizer.Instance.AvailableLanguages;
        OnPropertyChanged(nameof(AvailableLanguages));
        SelectedLanguage = AvailableLanguages.FirstOrDefault(item =>
            string.Equals(item.Code, SimpleLocalizer.Instance.Language, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Pending and finished downloads.</summary>
    public ObservableCollection<DownloadItemViewModel> Queue { get; } = new();

    /// <summary>Shared console log.</summary>
    public ObservableCollection<LogLine> Log { get; } = new();

    /// <summary>Formats reported for the analysed link.</summary>
    public ObservableCollection<MediaFormat> AvailableFormats { get; } = new();

    /// <summary>Playlist entries reported for the analysed link.</summary>
    public ObservableCollection<MediaEntry> PlaylistEntries { get; } = new();

    /// <summary>All friendly presets (video then audio) shown in the picker.</summary>
    public IReadOnlyList<DownloadPresetFactory.PresetChoice> Presets => DownloadPresetFactory.AllPresets;

    /// <summary>Choices for the playlist handling combo box.</summary>
    public IReadOnlyList<PlaylistHandling> PlaylistOptions { get; } = Enum.GetValues<PlaylistHandling>();

    /// <summary>Choices for the overwrite policy combo box.</summary>
    public IReadOnlyList<OverwritePolicy> OverwriteOptions { get; } = Enum.GetValues<OverwritePolicy>();

    /// <summary>Choices for the merge container combo box.</summary>
    public IReadOnlyList<MergeContainer> MergeContainerOptions { get; } = Enum.GetValues<MergeContainer>();

    /// <summary>Choices for the shortcut file combo box.</summary>
    public IReadOnlyList<LinkFileFormat> LinkFileOptions { get; } = Enum.GetValues<LinkFileFormat>();

    /// <summary>Selectable audio formats.</summary>
    public IReadOnlyList<string> AudioFormats { get; } = ["best", "mp3", "aac", "m4a", "opus", "vorbis", "flac", "alac", "wav"];

    /// <summary>Selectable container remux targets.</summary>
    public IReadOnlyList<string> VideoContainers { get; } = ["mp4", "mkv", "webm", "mov", "avi", "flv"];

    /// <summary>Selectable subtitle formats.</summary>
    public IReadOnlyList<string> SubtitleFormats { get; } = ["best", "srt", "ass", "vtt", "lrc", "srv3", "ttml", "json3"];

    /// <summary>Common subtitle languages offered as quick picks.</summary>
    public IReadOnlyList<string> SubtitleLanguagePresets { get; } =
    [
        "cs", "sk", "en", "en.*", "cs,en", "all", "all,-live_chat",
    ];

    /// <summary>External downloaders understood by yt-dlp.</summary>
    public IReadOnlyList<string> ExternalDownloaders { get; } =
    [
        "native", "aria2c", "axel", "curl", "ffmpeg", "httpie", "wget",
    ];

    /// <summary>Ready-made output filename templates.</summary>
    public IReadOnlyList<string> OutputTemplatePresets { get; } =
    [
        "%(title)s [%(id)s].%(ext)s",
        "%(title)s.%(ext)s",
        "%(uploader)s - %(title)s.%(ext)s",
        "%(uploader)s/%(title)s [%(id)s].%(ext)s",
        "%(playlist_title)s/%(playlist_index)03d - %(title)s.%(ext)s",
        "%(upload_date>%Y-%m-%d)s - %(title)s.%(ext)s",
    ];

    /// <summary>Ready-made subtitle language selections.</summary>
    public IReadOnlyList<string> SubtitleLanguageQuickPicks { get; } =
    [
        "cs", "sk", "en", "cs,sk,en", "en.*", "all", "all,-live_chat",
    ];

    /// <summary>Browsers yt-dlp can read cookies from.</summary>
    public IReadOnlyList<string> Browsers { get; } =
    [
        "firefox", "chrome", "chromium", "brave", "edge", "opera", "vivaldi", "whale", "safari", "none",
    ];

    /// <summary>The link the user wants to download.</summary>
    [ObservableProperty] public partial string Url { get; set; } = string.Empty;

    /// <summary>Metadata of the analysed link.</summary>
    [ObservableProperty] public partial MediaInfo? Info { get; set; }

    /// <summary>Thumbnail bitmap of the analysed link.</summary>
    [ObservableProperty] public partial Bitmap? Thumbnail { get; set; }

    /// <summary>Human readable analysis failure.</summary>
    [ObservableProperty] public partial string? AnalysisError { get; set; }

    /// <summary>True while analysing.</summary>
    [ObservableProperty] public partial bool IsAnalyzing { get; set; }

    /// <summary>A URL detected in the clipboard that was not auto-filled.</summary>
    [ObservableProperty] public partial string? ClipboardUrl { get; set; }

    /// <summary>Status line under the action buttons.</summary>
    [ObservableProperty] public partial string StatusMessage { get; set; } = "Připraveno";

    /// <summary>The item currently being downloaded, shown in the progress bar.</summary>
    [ObservableProperty] public partial DownloadItemViewModel? CurrentItem { get; set; }

    /// <summary>Overall queue completion in percent.</summary>
    [ObservableProperty] public partial double OverallProgress { get; set; }

    /// <summary>Aggregate queue description.</summary>
    [ObservableProperty] public partial string QueueSummary { get; set; } = string.Empty;

    /// <summary>The exact command line for the current settings.</summary>
    [ObservableProperty] public partial string CommandPreview { get; set; } = string.Empty;

    /// <summary>True while the queue is being processed.</summary>
    [ObservableProperty] public partial bool IsQueueRunning { get; set; }

    /// <summary>Selected friendly preset.</summary>
    [ObservableProperty] public partial DownloadPresetFactory.PresetChoice? SelectedPreset { get; set; }

    /// <summary>Format chosen from the analysed format table.</summary>
    [ObservableProperty] public partial MediaFormat? SelectedFormat { get; set; }

    /// <summary>Selected queue entry.</summary>
    [ObservableProperty] public partial DownloadItemViewModel? SelectedQueueItem { get; set; }

    /// <summary>Index of the selected option tab.</summary>
    [ObservableProperty] public partial int SelectedTabIndex { get; set; }

    /// <summary>Timestamp of the last clipboard poll, used by the window timer.</summary>
    public bool ClipboardWatchingEnabled => Settings.AutoFillFromClipboard && Settings.WatchClipboardContinuously;

    /// <summary>Glyph for the theme button: a sun while the dark theme is active.</summary>
    public string ThemeGlyph => Settings.DarkTheme ? "☀" : "🌙";

    /// <summary>Tooltip for the theme button.</summary>
    public string ThemeTooltip => Settings.DarkTheme ? "Přepnout na světlý režim" : "Přepnout na tmavý režim";

    /// <summary>True when the welcome card should be visible.</summary>
    public bool ShowWelcome => !Settings.FirstRunCompleted;

    /// <summary>True when the analysed link is a playlist.</summary>
    public bool InfoIsPlaylist => Info?.IsPlaylist == true;

    /// <summary>True when metadata for a single video is available.</summary>
    public bool HasInfo => Info is not null && Info.IsPlaylist != true;

    // ───────────────────────────── startup ─────────────────────────────

    private async Task InitializeAsync()
    {
        try
        {
            await InitializeCoreAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AddLog($"Při startu se něco nepovedlo: {ex.Message}", LogKind.Error);
            StatusMessage = "Kontrola nástrojů se nezdařila";
        }
    }

    private async Task InitializeCoreAsync()
    {
        AddLog("Kontroluji dostupnost yt-dlp…", LogKind.App);
        await Toolchain.RefreshAsync().ConfigureAwait(true);

        if (Toolchain.YtDlpAvailable)
        {
            AddLog($"yt-dlp nalezeno: {Toolchain.YtDlpVersionText} ({Toolchain.YtDlpLocationText})", LogKind.App);
        }
        else
        {
            AddLog("yt-dlp nebylo nalezeno. Otevřete prosím kartu „Nástroje“ a nainstalujte jej.", LogKind.Warning);
            StatusMessage = "yt-dlp není nainstalováno";
        }

        if (!Toolchain.FfmpegAvailable)
        {
            AddLog("ffmpeg nebyl nalezen – slučování a převod souborů nebude fungovat.", LogKind.Warning);
        }

        RefreshCommandPreview();

        if (Settings.CheckForYtDlpUpdates && Toolchain.YtDlpAvailable)
        {
            _ = Toolchain.CheckLatestCommand.ExecuteAsync(null);
        }
    }

    // ───────────────────────────── clipboard ─────────────────────────────

    /// <summary>
    /// Called by the window whenever the clipboard content changes. Fills the link box when it is
    /// empty, otherwise offers the detected link through <see cref="ClipboardUrl"/>.
    /// </summary>
    public void OnClipboardTextChanged(string? text)
    {
        if (!Settings.AutoFillFromClipboard)
        {
            return;
        }

        var detected = UrlDetector.ExtractFirstUrl(text);
        if (detected is null || string.Equals(detected, Url, StringComparison.OrdinalIgnoreCase))
        {
            if (detected is null)
            {
                ClipboardUrl = null;
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(Url))
        {
            Url = detected;
            ClipboardUrl = null;
            StatusMessage = "Odkaz vložen ze schránky";

            if (Settings.AutoAnalyzeOnPaste && Toolchain.YtDlpAvailable)
            {
                _ = AnalyzeAsync();
            }

            return;
        }

        ClipboardUrl = detected;
    }

    /// <summary>Take the offered clipboard link.</summary>
    [RelayCommand]
    private async Task AcceptClipboardUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(ClipboardUrl))
        {
            return;
        }

        Url = ClipboardUrl!;
        ClipboardUrl = null;

        if (Settings.AutoAnalyzeOnPaste && Toolchain.YtDlpAvailable)
        {
            await AnalyzeAsync().ConfigureAwait(true);
        }
    }

    /// <summary>Dismiss the clipboard suggestion.</summary>
    [RelayCommand]
    private void DismissClipboardUrl() => ClipboardUrl = null;

    /// <summary>Clear the current link and analysis.</summary>
    [RelayCommand]
    private void ClearUrl()
    {
        Url = string.Empty;
        Info = null;
        Thumbnail?.Dispose();
        Thumbnail = null;
        AnalysisError = null;
        AvailableFormats.Clear();
        PlaylistEntries.Clear();
        StatusMessage = "Připraveno";
        OnPropertyChanged(nameof(HasInfo));
        OnPropertyChanged(nameof(InfoIsPlaylist));
    }

    // ───────────────────────────── analysis ─────────────────────────────

    /// <summary>Fetch metadata for the current link.</summary>
    [RelayCommand(CanExecute = nameof(CanAnalyze))]
    private async Task AnalyzeAsync()
    {
        if (Toolchain.Command is null)
        {
            AnalysisError = "yt-dlp není dostupné. Otevřete kartu Nástroje.";
            return;
        }

        var urls = UrlDetector.ExtractAllUrls(Url);
        var target = urls.Count > 0 ? urls[0] : Url.Trim();
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        _analysisCts?.Cancel();
        _analysisCts?.Dispose();
        _analysisCts = new CancellationTokenSource();

        IsAnalyzing = true;
        AnalysisError = null;
        StatusMessage = "Analyzuji odkaz…";
        AddLog($"Analyzuji {target}", LogKind.App);

        try
        {
            var info = await _metadataService
                .AnalyzeAsync(Toolchain.Command, Options, target, true, _analysisCts.Token)
                .ConfigureAwait(true);

            Info = info;
            AvailableFormats.Clear();
            foreach (var format in info.Formats)
            {
                AvailableFormats.Add(format);
            }

            PlaylistEntries.Clear();
            foreach (var entry in info.Entries)
            {
                PlaylistEntries.Add(entry);
            }

            OnPropertyChanged(nameof(HasInfo));
            OnPropertyChanged(nameof(InfoIsPlaylist));

            StatusMessage = info.IsPlaylist
                ? $"Playlist: {info.DisplayTitle} ({info.PlaylistCount ?? info.Entries.Count} položek)"
                : info.DisplayTitle;

            AddLog(info.IsPlaylist
                ? $"Nalezen playlist „{info.DisplayTitle}“ s {info.PlaylistCount ?? info.Entries.Count} položkami."
                : $"Nalezeno: {info.DisplayTitle} ({Formatting.Duration(info.Duration)})", LogKind.App);

            _ = LoadThumbnailAsync(info.ThumbnailUrl);
        }
        catch (AnalysisException ex)
        {
            AnalysisError = ex.Message;
            Info = null;
            AvailableFormats.Clear();
            PlaylistEntries.Clear();
            Thumbnail?.Dispose();
            Thumbnail = null;
            OnPropertyChanged(nameof(HasInfo));
            OnPropertyChanged(nameof(InfoIsPlaylist));
            StatusMessage = "Analýza se nezdařila";
            AddLog(ex.Message, LogKind.Error);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Analýza zrušena";
        }
        catch (Exception ex)
        {
            AnalysisError = ex.Message;
            AddLog($"Chyba při analýze: {ex.Message}", LogKind.Error);
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private bool CanAnalyze() => !IsAnalyzing && !string.IsNullOrWhiteSpace(Url) && Toolchain.YtDlpAvailable;

    private async Task LoadThumbnailAsync(string? url)
    {
        var previous = Thumbnail;
        Thumbnail = null;
        previous?.Dispose();

        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(url).ConfigureAwait(true);
            using var stream = new MemoryStream(bytes);
            Thumbnail = new Bitmap(stream);
        }
        catch (Exception)
        {
            // A missing thumbnail must never break analysis.
            Thumbnail = null;
        }
    }

    /// <summary>Copy a format id into the manual format box.</summary>
    [RelayCommand]
    private void UseSelectedFormat()
    {
        if (SelectedFormat?.FormatId is not { Length: > 0 } id)
        {
            return;
        }

        // Clear any preset first (that resets the format box), then apply the picked format.
        _suppressPresetSync = true;
        SelectedPreset = MatchPresetChoice(DownloadPreset.Custom);
        _suppressPresetSync = false;

        Options.Preset = DownloadPreset.Custom;
        Options.Format = id;

        AddLog($"Formát nastaven na „{id}“.", LogKind.App);
        StatusMessage = $"Vybraný formát: {id}";
    }

    // ───────────────────────────── queue ─────────────────────────────

    /// <summary>Add the current link to the queue and make sure processing runs.</summary>
    [RelayCommand(CanExecute = nameof(CanDownload))]
    private void Download()
    {
        var urls = UrlDetector.ExtractAllUrls(Url);
        if (urls.Count == 0 && UrlDetector.LooksLikeUrl(Url))
        {
            urls = [Url.Trim()];
        }

        if (urls.Count == 0 && !string.IsNullOrWhiteSpace(Url))
        {
            urls = [Url.Trim()];
        }

        if (urls.Count == 0)
        {
            StatusMessage = "Nejprve vložte odkaz na video.";
            return;
        }

        foreach (var url in urls)
        {
            if (Queue.Any(item => item.State == DownloadState.Queued && item.Url == url))
            {
                continue;
            }

            Queue.Add(CreateQueueItem(url));
        }

        Url = string.Empty;
        StatusMessage = urls.Count == 1
            ? "Přidáno do fronty"
            : $"Přidáno {urls.Count} odkazů do fronty";

        StartQueueIfIdle();
    }

    /// <summary>
    /// Kick off the queue without blocking the caller, so the user can keep adding links
    /// while a download is already running.
    /// </summary>
    private void StartQueueIfIdle() => _ = StartQueueSafeAsync();

    private async Task StartQueueSafeAsync()
    {
        try
        {
            await EnsureQueueRunningAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            AddLog($"Frontu se nepodařilo spustit: {ex.Message}", LogKind.Error);
        }
    }

    private bool CanDownload() =>
        !string.IsNullOrWhiteSpace(Url) && Toolchain.YtDlpAvailable;

    /// <summary>Add the current link without starting, so the user can review the queue first.</summary>
    [RelayCommand(CanExecute = nameof(CanDownload))]
    private void Enqueue()
    {
        var urls = UrlDetector.ExtractAllUrls(Url);
        if (urls.Count == 0 && !string.IsNullOrWhiteSpace(Url))
        {
            urls = [Url.Trim()];
        }

        foreach (var url in urls)
        {
            Queue.Add(CreateQueueItem(url));
        }

        if (urls.Count > 0)
        {
            Url = string.Empty;
            StatusMessage = "Přidáno do fronty";
        }
    }

    private DownloadItemViewModel CreateQueueItem(string url)
    {
        var item = new DownloadItemViewModel(url)
        {
            PresetLabel = DownloadOptions.PresetLabelFor(Options.Preset),
            State = DownloadState.Queued,
        };

        if (Info is not null && string.Equals(Info.WebPageUrl, url, StringComparison.OrdinalIgnoreCase))
        {
            item.Title = Info.DisplayTitle;
        }

        if (Toolchain.Command is { } command)
        {
            var args = YtDlpArgumentBuilder.Build(Options, [url]);
            item.CommandPreview = command.Preview(args);
        }

        UpdateQueueSummary();
        return item;
    }

    /// <summary>Start the queue workers if they are not already running.</summary>
    public async Task EnsureQueueRunningAsync()
    {
        if (_queueTask is { IsCompleted: false })
        {
            return;
        }

        if (!Queue.Any(item => item.State == DownloadState.Queued))
        {
            UpdateQueueSummary();
            return;
        }

        if (Toolchain.Command is null)
        {
            AddLog("Nelze stahovat – yt-dlp není dostupné.", LogKind.Error);
            return;
        }

        _queueCts?.Dispose();
        _queueCts = new CancellationTokenSource();
        IsQueueRunning = true;
        UpdateQueueSummary();

        var token = _queueCts.Token;
        _queueTask = RunQueueAsync(token);

        try
        {
            await _queueTask.ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // Expected when the user cancels.
        }
        finally
        {
            IsQueueRunning = false;
            CurrentItem = null;
            OverallProgress = 0;
            UpdateQueueSummary();
            AddLog("Fronta dokončena.", LogKind.App);
        }
    }

    private async Task RunQueueAsync(CancellationToken cancellationToken)
    {
        var workers = Math.Clamp(Settings.MaxConcurrentDownloads, 1, 4);
        var tasks = new List<Task>(workers);
        for (var i = 0; i < workers; i++)
        {
            tasks.Add(WorkerLoopAsync(cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(true);
    }

    private async Task WorkerLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DownloadItemViewModel? item;
            lock (_queueLock)
            {
                item = Queue.FirstOrDefault(candidate => candidate.State == DownloadState.Queued);
                if (item is null)
                {
                    return;
                }

                item.State = DownloadState.Downloading;
            }

            UpdateQueueSummary();

            try
            {
                await RunItemAsync(item, cancellationToken).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                item.State = DownloadState.Cancelled;
                return;
            }
            catch (Exception ex)
            {
                item.State = DownloadState.Failed;
                item.ErrorMessage = ex.Message;
                AddLog($"[{item.DisplayTitle}] {ex.Message}", LogKind.Error);
            }
            finally
            {
                UpdateQueueSummary();
            }
        }
    }

    private async Task RunItemAsync(DownloadItemViewModel item, CancellationToken cancellationToken)
    {
        var command = Toolchain.Command;
        if (command is null)
        {
            item.State = DownloadState.Failed;
            item.ErrorMessage = "yt-dlp není dostupné.";
            return;
        }

        CurrentItem = item;
        AddLog($"━━ Začínám stahovat: {item.DisplayTitle}", LogKind.App);

        if (item.CommandPreview is { Length: > 0 } preview)
        {
            AddLog($"   $ {preview}", LogKind.Debug);
        }

        var outcome = await DownloadEngine
            .RunAsync(
                command,
                Options,
                item.Url,
                onEvent: e => DispatchProgress(item, e),
                cancellationToken: cancellationToken)
            .ConfigureAwait(true);

        if (outcome.Cancelled)
        {
            item.State = DownloadState.Cancelled;
            item.StatusText = "Zrušeno";
            AddLog($"[{item.DisplayTitle}] zrušeno.", LogKind.Warning);
            return;
        }

        if (outcome.Success)
        {
            item.State = DownloadState.Completed;
            item.Progress = 100;
            item.FilePath = outcome.FilePath;
            item.Title ??= Path.GetFileNameWithoutExtension(outcome.FilePath);
            AddLog($"✓ Hotovo: {outcome.FilePath ?? item.DisplayTitle}", LogKind.App);
            StatusMessage = "Stahování dokončeno";
            return;
        }

        item.State = DownloadState.Failed;
        item.ErrorMessage = outcome.ErrorMessage ?? $"yt-dlp skončilo s kódem {outcome.ExitCode}.";
        AddLog($"✗ Chyba u „{item.DisplayTitle}“: {item.ErrorMessage}", LogKind.Error);
        StatusMessage = "Stahování selhalo";
    }

    private void DispatchProgress(DownloadItemViewModel item, YtDlpEvent evt)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => DispatchProgress(item, evt));
            return;
        }

        switch (evt)
        {
            case ProgressEvent progressEvent:
                item.State = DownloadState.Downloading;
                item.ApplyProgress(progressEvent.Progress);
                if (ReferenceEquals(CurrentItem, item))
                {
                    OverallProgress = item.Progress ?? OverallProgress;
                }

                break;

            case PostProcessEvent postProcess:
                item.State = DownloadState.PostProcessing;
                item.StatusText = string.IsNullOrWhiteSpace(postProcess.PostProcessor)
                    ? "Dokončuji soubor…"
                    : $"Dokončuji ({postProcess.PostProcessor})…";
                break;

            case DestinationEvent destination:
                item.FilePath = destination.Path;
                item.Title ??= Path.GetFileNameWithoutExtension(destination.Path);
                AddLog(destination.Path, LogKind.Debug);
                break;

            case LogEvent logEvent:
                var kind = logEvent.Level switch
                {
                    YtDlpLogLevel.Error => LogKind.Error,
                    YtDlpLogLevel.Warning => LogKind.Warning,
                    YtDlpLogLevel.Debug => LogKind.Debug,
                    _ => LogKind.Output,
                };
                AddLog(logEvent.Text, kind);
                break;
        }
    }

    /// <summary>Cancel the running download and drain the queue.</summary>
    [RelayCommand]
    private void CancelAll()
    {
        try
        {
            _queueCts?.Cancel();
        }
        catch (Exception)
        {
            // Nothing to cancel.
        }

        lock (_queueLock)
        {
            foreach (var item in Queue.Where(i => i.State == DownloadState.Queued).ToList())
            {
                item.State = DownloadState.Cancelled;
            }
        }

        AddLog("Ruším stahování…", LogKind.Warning);
        UpdateQueueSummary();
    }

    /// <summary>Remove one queue entry.</summary>
    [RelayCommand]
    private void RemoveQueueItem(DownloadItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        Queue.Remove(item);
        if (ReferenceEquals(SelectedQueueItem, item))
        {
            SelectedQueueItem = null;
        }

        UpdateQueueSummary();
    }

    /// <summary>Put a finished or failed item back into the queue.</summary>
    [RelayCommand]
    private void RetryQueueItem(DownloadItemViewModel? item)
    {
        if (item is null || item.IsRunning)
        {
            return;
        }

        item.ResetForRetry();
        UpdateQueueSummary();
        StartQueueIfIdle();
    }

    /// <summary>Remove finished entries from the list.</summary>
    [RelayCommand]
    private void ClearFinished()
    {
        foreach (var item in Queue.Where(i => i.IsFinished).ToList())
        {
            Queue.Remove(item);
        }

        UpdateQueueSummary();
    }

    /// <summary>Open the folder of a finished download.</summary>
    [RelayCommand]
    private void OpenItemFolder(DownloadItemViewModel? item)
    {
        var target = item?.FilePath;
        if (!ShellUtil.OpenContainingFolder(target) && !ShellUtil.OpenFolder(Options.OutputDirectory))
        {
            AddLog("Složku se nepodařilo otevřít.", LogKind.Warning);
        }
    }

    /// <summary>Open the destination folder.</summary>
    [RelayCommand]
    private void OpenOutputFolder()
    {
        var directory = string.IsNullOrWhiteSpace(Options.OutputDirectory)
            ? UserPaths.DefaultDownloadDirectory
            : Options.OutputDirectory;

        if (!ShellUtil.OpenFolder(directory))
        {
            AddLog($"Složku „{directory}“ se nepodařilo otevřít.", LogKind.Warning);
        }
    }

    private void UpdateQueueSummary()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(UpdateQueueSummary);
            return;
        }

        var total = Queue.Count;
        if (total == 0)
        {
            QueueSummary = "Fronta je prázdná";
            OverallProgress = 0;
            return;
        }

        var done = Queue.Count(i => i.State == DownloadState.Completed);
        var failed = Queue.Count(i => i.State == DownloadState.Failed);
        var running = Queue.Count(i => i.IsRunning);
        var queued = Queue.Count(i => i.State == DownloadState.Queued);

        var parts = new List<string>();
        if (running > 0)
        {
            parts.Add($"{running} běží");
        }

        if (queued > 0)
        {
            parts.Add($"{queued} ve frontě");
        }

        if (done > 0)
        {
            parts.Add($"{done} hotovo");
        }

        if (failed > 0)
        {
            parts.Add($"{failed} chyb");
        }

        QueueSummary = string.Join(" · ", parts);

        if (running == 0)
        {
            OverallProgress = (done + failed) / (double)total * 100;
        }
    }

    // ───────────────────────────── presets & preview ─────────────────────────────

    partial void OnSelectedPresetChanged(DownloadPresetFactory.PresetChoice? value)
    {
        if (_suppressPresetSync || value is null)
        {
            return;
        }

        DownloadPresetFactory.Apply(Options, value.Preset);
        StatusMessage = $"Profil: {value.Title}";
        AddLog($"Zvolen profil „{value.Title}“.", LogKind.App);
    }

    private static DownloadPresetFactory.PresetChoice MatchPresetChoice(DownloadPreset preset)
    {
        return DownloadPresetFactory.AllPresets.FirstOrDefault(choice => choice.Preset == preset)
               ?? DownloadPresetFactory.AllPresets[0];
    }

    private void OnOptionsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DownloadOptions.Preset) && !_suppressPresetSync)
        {
            var match = MatchPresetChoice(Options.Preset);
            if (!Equals(SelectedPreset, match))
            {
                _suppressPresetSync = true;
                SelectedPreset = match;
                _suppressPresetSync = false;
            }
        }

        RefreshCommandPreview();
        ScheduleAutosave();
    }

    private void OnSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AppSettings.DarkTheme))
        {
            ApplyTheme();
        }

        if (e.PropertyName == nameof(AppSettings.Language))
        {
            SimpleLocalizer.Instance.SetLanguage(Settings.Language);
            SelectedLanguage = AvailableLanguages.FirstOrDefault(item =>
                string.Equals(item.Code, SimpleLocalizer.Instance.Language, StringComparison.OrdinalIgnoreCase));
        }

        ScheduleAutosave();
    }

    private void OnToolchainChanged()
    {
        // Pass the private FFmpeg location explicitly to yt-dlp when necessary.
        if (string.IsNullOrWhiteSpace(Options.FfmpegLocation) &&
            Toolchain.FfmpegAvailable &&
            !string.IsNullOrWhiteSpace(Toolchain.FfmpegLocationText))
        {
            Options.FfmpegLocation = Toolchain.FfmpegLocationText;
        }

        AnalyzeCommand.NotifyCanExecuteChanged();
        DownloadCommand.NotifyCanExecuteChanged();
        EnqueueCommand.NotifyCanExecuteChanged();
        RefreshCommandPreview();
    }

    /// <summary>Recompute the command preview line.</summary>
    public void RefreshCommandPreview()
    {
        var urls = UrlDetector.ExtractAllUrls(Url);
        var target = urls.Count > 0 ? urls[0] : "<odkaz>";
        var args = YtDlpArgumentBuilder.Build(Options, [target], machineReadableProgress: true);

        CommandPreview = Toolchain.Command is { } command
            ? command.Preview(args)
            : CommandLine.Format("yt-dlp", args);
    }

    partial void OnUrlChanged(string value)
    {
        RefreshCommandPreview();
        AnalyzeCommand.NotifyCanExecuteChanged();
        DownloadCommand.NotifyCanExecuteChanged();
        EnqueueCommand.NotifyCanExecuteChanged();
    }

    // ───────────────────────────── log ─────────────────────────────

    /// <summary>Append a line to the shared log, marshalling onto the UI thread.</summary>
    public void AddLog(string text, LogKind kind)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => AddLog(text, kind));
            return;
        }

        foreach (var line in text.Split('\n'))
        {
            Log.Add(new LogLine(line.TrimEnd('\r'), kind));
        }

        while (Log.Count > MaxLogLines)
        {
            Log.RemoveAt(0);
        }
    }

    /// <summary>Clear the log pane.</summary>
    [RelayCommand]
    private void ClearLog()
    {
        Log.Clear();
        AddLog("Log vymazán.", LogKind.App);
    }

    /// <summary>The whole log as plain text, for the clipboard.</summary>
    public string GetLogText()
    {
        var builder = new StringBuilder();
        foreach (var line in Log)
        {
            builder.Append('[').Append(line.Stamp).Append("] ").AppendLine(line.Text);
        }

        return builder.ToString();
    }

    // ───────────────────────────── theme & lifetime ─────────────────────────────

    private void ApplyTheme()
    {
        // The variant must be set on the application: a window-level variant would win and
        // make the theme button look dead once a window had pinned its own value.
        if (Avalonia.Application.Current is { } app)
        {
            app.RequestedThemeVariant = Settings.DarkTheme
                ? Avalonia.Styling.ThemeVariant.Dark
                : Avalonia.Styling.ThemeVariant.Light;
        }

        OnPropertyChanged(nameof(ThemeGlyph));
        OnPropertyChanged(nameof(ThemeTooltip));
    }

    /// <summary>Set the output filename template from a ready-made choice.</summary>
    [RelayCommand]
    private void UseOutputTemplate(string? template)
    {
        if (!string.IsNullOrWhiteSpace(template))
        {
            Options.OutputTemplate = template!;
        }
    }

    /// <summary>Set the subtitle languages from a ready-made choice.</summary>
    [RelayCommand]
    private void UseSubtitleLanguages(string? languages)
    {
        if (!string.IsNullOrWhiteSpace(languages))
        {
            Options.SubLangs = languages!;
            Options.WriteSubs = true;
        }
    }

    /// <summary>Set a SponsorBlock category selection.</summary>
    [RelayCommand]
    private void UseSponsorBlock(string? spec)
    {
        if (string.IsNullOrWhiteSpace(spec))
        {
            return;
        }

        // "remove:sponsor,selfpromo" or "mark:intro,outro"
        var parts = spec!.Split(':', 2);
        if (parts.Length != 2)
        {
            return;
        }

        if (parts[0] == "remove")
        {
            Options.SponsorBlockRemove = parts[1];
        }
        else
        {
            Options.SponsorBlockMark = parts[1];
        }
    }

    /// <summary>Restore every download option to its default value.</summary>
    [RelayCommand]
    private void ResetOptions()
    {
        var directory = Options.OutputDirectory;
        var fresh = new DownloadOptions
        {
            OutputDirectory = string.IsNullOrWhiteSpace(directory) ? UserPaths.DefaultDownloadDirectory : directory,
        };

        _suppressPresetSync = true;
        SelectedPreset = MatchPresetChoice(fresh.Preset);
        _suppressPresetSync = false;

        Options.Preset = fresh.Preset;
        Options.Format = fresh.Format;
        Options.FormatSort = null;
        Options.FormatSortForce = false;
        Options.MergeContainer = MergeContainer.Default;
        Options.ExtractAudio = false;
        Options.AudioFormat = fresh.AudioFormat;
        Options.AudioQuality = fresh.AudioQuality;
        Options.EmbedMetadata = false;
        Options.EmbedThumbnail = false;
        Options.WriteSubs = false;
        Options.WriteAutoSubs = false;
        Options.SubLangs = fresh.SubLangs;
        Options.SubFormat = fresh.SubFormat;
        Options.EmbedSubs = false;
        Options.ConvertSubs = null;
        Options.WriteThumbnail = false;
        Options.WriteAllThumbnails = false;
        Options.ConvertThumbnails = null;
        Options.PlaylistHandling = PlaylistHandling.Default;
        Options.PlaylistItems = null;
        Options.PlaylistReverse = false;
        Options.PlaylistRandom = false;
        Options.LazyPlaylist = false;
        Options.FlatPlaylist = false;
        Options.MaxDownloads = 0;
        Options.SkipPlaylistAfterErrors = 0;
        Options.IgnoreErrors = false;
        Options.AbortOnError = false;
        Options.MinFilesize = null;
        Options.MaxFilesize = null;
        Options.Date = null;
        Options.DateAfter = null;
        Options.DateBefore = null;
        Options.AgeLimit = 0;
        Options.MatchFilters = null;
        Options.BreakMatchFilters = null;
        Options.LiveFromStart = false;
        Options.WaitForVideo = null;
        Options.Overwrite = OverwritePolicy.Default;
        Options.RestrictFilenames = false;
        Options.WindowsFilenames = false;
        Options.TrimFilenames = 0;
        Options.NoContinue = false;
        Options.NoPart = false;
        Options.UseMtime = false;
        Options.OutputTemplate = DownloadOptions.DefaultOutputTemplate;
        Options.TempDirectory = null;
        Options.Proxy = string.Empty;
        Options.SocketTimeout = fresh.SocketTimeout;
        Options.Impersonate = string.Empty;
        Options.LimitRate = string.Empty;
        Options.Retries = fresh.Retries;
        Options.FragmentRetries = fresh.FragmentRetries;
        Options.FileAccessRetries = fresh.FileAccessRetries;
        Options.ExtractorRetries = fresh.ExtractorRetries;
        Options.ConcurrentFragments = fresh.ConcurrentFragments;
        Options.BufferSize = null;
        Options.HttpChunkSize = null;
        Options.SleepRequests = 0;
        Options.SleepInterval = 0;
        Options.MaxSleepInterval = 0;
        Options.SleepSubtitles = 0;
        Options.ForceIpv4 = false;
        Options.ForceIpv6 = false;
        Options.NoCheckCertificates = false;
        Options.LegacyServerConnect = false;
        Options.AddHeaders = null;
        Options.GeoVerificationProxy = null;
        Options.XForwardedFor = null;
        Options.JsRuntimes = null;
        Options.RemoteComponents = null;
        Options.ExternalDownloader = null;
        Options.ExternalDownloaderArgs = null;
        Options.CookiesFile = null;
        Options.CookiesFromBrowser = null;
        Options.Username = null;
        Options.Password = null;
        Options.TwoFactor = null;
        Options.VideoPassword = null;
        Options.Netrc = false;
        Options.NetrcLocation = null;
        Options.ClientCertificate = null;
        Options.ClientCertificateKey = null;
        Options.ClientCertificatePassword = null;
        Options.SponsorBlockMark = null;
        Options.SponsorBlockRemove = null;
        Options.SponsorBlockApi = null;
        Options.ParseMetadata = null;
        Options.RemoveChapters = null;
        Options.DownloadSections = null;
        Options.ForceKeyframesAtCuts = false;
        Options.PostprocessorArgs = null;
        Options.RemuxVideo = null;
        Options.RecodeVideo = null;
        Options.KeepVideo = false;
        Options.EmbedChapters = false;
        Options.SplitChapters = false;
        Options.WriteDescription = false;
        Options.WriteInfoJson = false;
        Options.WriteComments = false;
        Options.WritePlaylistMetafiles = false;
        Options.Xattrs = false;
        Options.LinkFile = LinkFileFormat.None;
        Options.ExecCommand = null;
        Options.ExtraArguments = string.Empty;
        Options.IgnoreConfig = false;
        Options.ConfigLocations = null;
        Options.PluginDirs = null;
        Options.ExtractorArgs = null;
        Options.Verbose = false;
        Options.NoWarnings = false;
        Options.WritePages = false;
        Options.PrintTraffic = false;
        Options.BatchFile = null;
        Options.DownloadArchive = null;
        Options.BreakOnExisting = false;
        Options.BreakPerInput = false;
        Options.CacheDirectory = null;
        Options.RemoveCache = false;

        AddLog("Všechna nastavení stahování byla obnovena na výchozí hodnoty.", LogKind.App);
        StatusMessage = "Nastavení obnoveno";
    }

    /// <summary>Switch between the dark and light theme.</summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        Settings.DarkTheme = !Settings.DarkTheme;
        ApplyTheme();
        AddLog(Settings.DarkTheme ? "Zapnut tmavý režim." : "Zapnut světlý režim.", LogKind.App);
    }

    /// <summary>Jump to the tools tab so the user can install yt-dlp.</summary>
    [RelayCommand]
    private void ShowToolsTab() => SelectedTabIndex = 8;

    /// <summary>Dismiss the welcome card.</summary>
    [RelayCommand]
    private void DismissWelcome()
    {
        Settings.FirstRunCompleted = true;
        OnPropertyChanged(nameof(ShowWelcome));
    }

    private void ScheduleAutosave()
    {
        if (_disposed)
        {
            return;
        }

        _autosaveTimer.Stop();
        _autosaveTimer.Start();
    }

    /// <summary>Write settings and the current options snapshot to disk.</summary>
    private void SaveSettings()
    {
        Settings.DefaultOutputDirectory = string.IsNullOrWhiteSpace(Options.OutputDirectory)
            ? Settings.DefaultOutputDirectory
            : Options.OutputDirectory;

        if (Settings.RememberOptions)
        {
            Settings.LastOptionsJson = SettingsStore.StoreOptions(Options);
        }

        _settingsStore.Save(Settings);
    }

    /// <summary>Persist settings and options. Safe to call more than once.</summary>
    public void Shutdown()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _autosaveTimer.Stop();

        try
        {
            _analysisCts?.Cancel();
            _queueCts?.Cancel();
        }
        catch (Exception)
        {
            // Ignore shutdown races.
        }

        SaveSettings();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Shutdown();
        _analysisCts?.Dispose();
        _queueCts?.Dispose();
        _httpClient.Dispose();
    }
}
