using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YtDlpUi.Core.Localization;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.ViewModels;

/// <summary>One selectable conversion format with a live-translated label.</summary>
public sealed record TargetOption(ConversionTarget Value, LocalizedText Label);

/// <summary>One selectable quality tier with a live-translated label.</summary>
public sealed record QualityOption(ConversionQuality Value, LocalizedText Label);

/// <summary>One selectable maximum height; zero keeps the original size.</summary>
public sealed record HeightOption(int Value, LocalizedText Label);

/// <summary>
/// Everything the converter needs from its owner. Keeping it a small value object makes the
/// page testable without constructing the full main window view model.
/// </summary>
public sealed record ConverterHost(
    Func<string?> FfmpegPath,
    Func<bool> IsFfmpegAvailable,
    Action<string, LogKind> Log);

/// <summary>
/// Drives the FFmpeg conversion page: source selection, probe summary, conversion settings
/// and the running conversion with progress.
/// </summary>
public sealed partial class ConverterViewModel : ObservableObject, IDisposable
{
    private readonly ConverterHost _host;
    private CancellationTokenSource? _conversionCts;
    private CancellationTokenSource? _probeCts;
    private FfmpegTools? _tools;
    private bool _syncing;
    private bool _disposed;

    public ConverterViewModel(ConverterHost host, string? outputDirectory = null)
    {
        _host = host;

        Options = new ConversionOptions
        {
            OutputDirectory = outputDirectory ?? string.Empty,
            Target = ConversionTarget.Mp4,
            Quality = ConversionQuality.Balanced,
        };

        var localizer = SimpleLocalizer.Instance;
        Targets = Enum.GetValues<ConversionTarget>()
            .Select(target => new TargetOption(target, localizer.For(ConversionTargets.Label(target))))
            .ToList();

        Qualities = Enum.GetValues<ConversionQuality>()
            .Select(quality => new QualityOption(quality, localizer.For(QualityLabel(quality))))
            .ToList();

        HeightChoices = new[] { 0, 2160, 1440, 1080, 720, 480, 360, 240 }
            .Select(height => new HeightOption(
                height,
                localizer.For(height == 0 ? "Keep the original size" : height + "p")))
            .ToList();

        _syncing = true;
        SelectedTargetOption = Targets.First(option => option.Value == Options.Target);
        SelectedQualityOption = Qualities.First(option => option.Value == Options.Quality);
        SelectedHeightOption = HeightChoices.First(option => option.Value == Options.MaxHeight);
        _syncing = false;

        Options.PropertyChanged += (_, _) => OnOptionsChanged();
    }

    /// <summary>Conversion settings bound directly by the page.</summary>
    public ConversionOptions Options { get; }

    /// <summary>Selectable output formats.</summary>
    public IReadOnlyList<TargetOption> Targets { get; }

    /// <summary>Selectable quality tiers.</summary>
    public IReadOnlyList<QualityOption> Qualities { get; }

    /// <summary>Selectable maximum heights.</summary>
    public IReadOnlyList<HeightOption> HeightChoices { get; }

    /// <summary>Currently selected output format.</summary>
    [ObservableProperty] public partial TargetOption? SelectedTargetOption { get; set; }

    /// <summary>Currently selected quality tier.</summary>
    [ObservableProperty] public partial QualityOption? SelectedQualityOption { get; set; }

    /// <summary>Currently selected maximum height.</summary>
    [ObservableProperty] public partial HeightOption? SelectedHeightOption { get; set; }

    /// <summary>Probe result of the selected source file.</summary>
    [ObservableProperty] public partial MediaProfile? SourceProfile { get; private set; }

    /// <summary>True while ffprobe reads the source.</summary>
    [ObservableProperty] public partial bool IsProbing { get; private set; }

    /// <summary>True while FFmpeg is converting.</summary>
    [ObservableProperty] public partial bool IsRunning { get; private set; }

    /// <summary>Progress of the running conversion in percent.</summary>
    [ObservableProperty] public partial double? Progress { get; private set; }

    /// <summary>Human readable state of the page.</summary>
    [ObservableProperty] public partial string StatusText { get; set; } = string.Empty;

    /// <summary>Error text for the last failed conversion or probe.</summary>
    [ObservableProperty] public partial string? ErrorMessage { get; private set; }

    /// <summary>Path of the produced file after a successful conversion.</summary>
    [ObservableProperty] public partial string? ResultPath { get; private set; }

    /// <summary>Full path the conversion will write to, shown before starting.</summary>
    public string OutputPathPreview
    {
        get
        {
            var source = FfmpegArgumentBuilder.ResolveSource(Options);
            return string.IsNullOrWhiteSpace(source)
                ? Localizer("Choose a source file first.")
                : FfmpegArgumentBuilder.BuildOutputPath(Options);
        }
    }

    /// <summary>True when FFmpeg is available, so the page can show its warning banner.</summary>
    public bool FfmpegAvailable => _host.IsFfmpegAvailable();

    /// <summary>True when a readable source file is selected.</summary>
    public bool HasSource => !string.IsNullOrWhiteSpace(Options.SourcePath) && SourceProfile is not null;

    /// <summary>
    /// True when conversion can start: the source must have been read successfully, so a
    /// missing or unreadable file can never enable the button.
    /// </summary>
    public bool CanConvert => !IsRunning && HasSource && _host.IsFfmpegAvailable();

    /// <summary>Read metadata for the current source file and update the summary.</summary>
    public async Task ProbeSourceAsync()
    {
        var source = FfmpegArgumentBuilder.ResolveSource(Options);
        SourceProfile = null;
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasSource));
        NotifyOutputChanged();

        if (string.IsNullOrWhiteSpace(source))
        {
            StatusText = Localizer("Choose a source file first.");
            return;
        }

        if (!File.Exists(source))
        {
            ErrorMessage = Localizer("The selected file does not exist.");
            StatusText = Localizer("The file could not be read.");
            return;
        }

        _probeCts?.Cancel();
        _probeCts?.Dispose();
        _probeCts = new CancellationTokenSource();

        IsProbing = true;
        StatusText = Localizer("Analysing the source file…");

        try
        {
            _tools = await ToolLocator
                .ResolveFfmpegToolsAsync(_host.FfmpegPath(), _probeCts.Token)
                .ConfigureAwait(true);

            if (_tools.FfprobePath is null)
            {
                ErrorMessage = Localizer("FFmpeg is not available. Install it on the Tools tab.");
                StatusText = Localizer("The file could not be read.");
                return;
            }

            var profile = await MediaProbe
                .ProbeAsync(_tools.FfprobePath, source, _probeCts.Token)
                .ConfigureAwait(true);

            if (profile is null)
            {
                ErrorMessage = Localizer("This file could not be read by FFmpeg.");
                StatusText = Localizer("The file could not be read.");
                return;
            }

            SourceProfile = profile;
            StatusText = profile.Summary;

            if (!profile.HasVideo && Options.Target != ConversionTarget.Mp3)
            {
                Options.Target = ConversionTarget.Mp3;
            }
            else if (profile.HasVideo && Options.Target.IsAudio())
            {
                Options.Target = ConversionTarget.Mp4;
            }

            _host.Log($"{Localizer("Source file")}: {profile.FileName} — {profile.Summary}", LogKind.App);
        }
        catch (OperationCanceledException)
        {
            // A newer probe replaced this one.
        }
        finally
        {
            IsProbing = false;
            OnPropertyChanged(nameof(HasSource));
            NotifyOutputChanged();
        }
    }

    /// <summary>Run the configured conversion.</summary>
    [RelayCommand(CanExecute = nameof(CanConvert))]
    private async Task ConvertAsync()
    {
        if (IsRunning)
        {
            return;
        }

        _tools ??= await ToolLocator
            .ResolveFfmpegToolsAsync(_host.FfmpegPath())
            .ConfigureAwait(true);

        if (_tools.FfmpegPath is null)
        {
            ErrorMessage = Localizer("FFmpeg is not available. Install it on the Tools tab.");
            StatusText = Localizer("Conversion failed");
            return;
        }

        var sourcePath = FfmpegArgumentBuilder.ResolveSource(Options);
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
        {
            ErrorMessage = Localizer("The selected file does not exist.");
            StatusText = Localizer("Conversion failed");
            return;
        }

        _conversionCts?.Dispose();
        _conversionCts = new CancellationTokenSource();

        IsRunning = true;
        ErrorMessage = null;
        ResultPath = null;
        Progress = 0;
        StatusText = Localizer("Preparing…");
        ConvertCommand.NotifyCanExecuteChanged();

        var snapshot = Options.Snapshot();
        var output = FfmpegArgumentBuilder.BuildOutputPath(snapshot);
        _host.Log(
            $"{Localizer("Converting")} {Path.GetFileName(snapshot.SourcePath)} → {Path.GetFileName(output)}",
            LogKind.App);

        try
        {
            var duration = SourceProfile?.DurationSeconds;
            var outcome = await ConversionEngine
                .RunAsync(
                    _tools.FfmpegPath,
                    snapshot,
                    duration,
                    progress =>
                    {
                        Progress = progress.Percent(duration) ?? Progress;
                        if (progress.SpeedFactor is > 0)
                        {
                            StatusText = string.Format(
                                CultureInfo.CurrentCulture,
                                Localizer("Converting… {0:0.#}× speed"),
                                progress.SpeedFactor);
                        }
                    },
                    cancellationToken: _conversionCts.Token)
                .ConfigureAwait(true);

            if (outcome.Cancelled)
            {
                StatusText = Localizer("Conversion cancelled");
                Progress = null;
                _host.Log(Localizer("Conversion cancelled."), LogKind.Warning);
                return;
            }

            if (!outcome.Success || outcome.OutputPath is null)
            {
                ErrorMessage = outcome.ErrorMessage ?? Localizer("Conversion failed");
                StatusText = Localizer("Conversion failed");
                Progress = null;
                _host.Log($"{Localizer("Conversion failed")}: {ErrorMessage}", LogKind.Error);
                return;
            }

            ResultPath = outcome.OutputPath;
            Progress = 100;
            StatusText = Localizer("Conversion finished");
            _host.Log($"✓ {Localizer("Conversion finished")}: {outcome.OutputPath}", LogKind.App);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            StatusText = Localizer("Conversion failed");
            _host.Log($"{Localizer("Conversion failed")}: {exception.Message}", LogKind.Error);
        }
        finally
        {
            IsRunning = false;
            NotifyOutputChanged();
        }
    }

    /// <summary>Stop the running conversion.</summary>
    [RelayCommand]
    private void CancelConvert()
    {
        try
        {
            _conversionCts?.Cancel();
        }
        catch (Exception)
        {
            // Nothing to cancel.
        }
    }

    /// <summary>Open the folder that receives the converted file.</summary>
    [RelayCommand]
    private void OpenOutputFolder()
    {
        var target = ResultPath ?? FfmpegArgumentBuilder.BuildOutputPath(Options);
        if (!ShellUtil.OpenContainingFolder(target))
        {
            ShellUtil.OpenFolder(Options.OutputDirectory);
        }
    }

    /// <summary>Play the produced file with the system default player.</summary>
    [RelayCommand]
    private void PlayResult()
    {
        if (ResultPath is not null && !ShellUtil.OpenFile(ResultPath))
        {
            _host.Log(Localizer("The file could not be opened in a player."), LogKind.Warning);
        }
    }

    /// <summary>Load a file into the converter, for example from the player window.</summary>
    public void SetSource(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        Options.SourcePath = path;
        _ = ProbeSourceAsync();
    }

    partial void OnSelectedTargetOptionChanged(TargetOption? value)
    {
        if (_syncing || value is null)
        {
            return;
        }

        _syncing = true;
        Options.Target = value.Value;
        _syncing = false;
        NotifyOutputChanged();
    }

    partial void OnSelectedQualityOptionChanged(QualityOption? value)
    {
        if (_syncing || value is null)
        {
            return;
        }

        _syncing = true;
        Options.Quality = value.Value;
        _syncing = false;
    }

    partial void OnSelectedHeightOptionChanged(HeightOption? value)
    {
        if (_syncing || value is null)
        {
            return;
        }

        _syncing = true;
        Options.MaxHeight = value.Value;
        _syncing = false;
    }

    private void OnOptionsChanged()
    {
        if (!_syncing)
        {
            _syncing = true;
            SelectedTargetOption = Targets.FirstOrDefault(option => option.Value == Options.Target) ?? SelectedTargetOption;
            SelectedQualityOption = Qualities.FirstOrDefault(option => option.Value == Options.Quality) ?? SelectedQualityOption;
            SelectedHeightOption = HeightChoices.FirstOrDefault(option => option.Value == Options.MaxHeight) ?? SelectedHeightOption;
            _syncing = false;
        }

        NotifyOutputChanged();
    }

    private void NotifyOutputChanged()
    {
        OnPropertyChanged(nameof(OutputPathPreview));
        OnPropertyChanged(nameof(CanConvert));
        ConvertCommand.NotifyCanExecuteChanged();
    }

    private static string QualityLabel(ConversionQuality quality) => quality switch
    {
        ConversionQuality.High => "High",
        ConversionQuality.Balanced => "Balanced",
        _ => "Small",
    };

    private static string Localizer(string english) => SimpleLocalizer.Instance.Translate(english);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _probeCts?.Cancel();
        _conversionCts?.Cancel();
        _probeCts?.Dispose();
        _conversionCts?.Dispose();
    }
}
