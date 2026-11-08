using Avalonia.Data;
using Avalonia.Markup.Xaml;
using YtDlpUi.Core.Localization;

namespace YtDlpUi.Localization;

/// <summary>
/// XAML: Text="{loc:Tr Key='Download'}". Returns a binding so language changes
/// refresh every affected control without rebuilding the window.
/// </summary>
public sealed class TrExtension : MarkupExtension
{
    /// <summary>English sentence used as the translation key and fallback.</summary>
    public string Key { get; set; } = string.Empty;

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new Binding(nameof(LocalizedText.Value))
        {
            Source = SimpleLocalizer.Instance.For(Key),
            Mode = BindingMode.OneWay,
        };
}
