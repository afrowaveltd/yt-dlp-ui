using YtDlpUi.ViewModels;

/// <summary>
/// Minimal stand-in that exposes the converter the way MainWindowViewModel does, so the real
/// ConverterPage.axaml can be rendered without building the whole main window.
/// </summary>
internal sealed class ConverterPageContext
{
    public ConverterPageContext(ConverterViewModel converter) => Converter = converter;

    public ConverterViewModel Converter { get; }
}
