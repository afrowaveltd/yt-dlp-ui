# Release 0.1.6 — Reliable playlist title rendering

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.6)

**Maintenance pre-release for Windows/Linux desktop.**

A playlist preview could continue showing the CLR type `YtDlpUi.Core.Models.MediaEntry` instead of video titles in v0.1.5. This version addresses both the XAML template and the fallback path.

- The `ListBox.ItemTemplate` on the Download page is now **unconditional**, without a restrictive `DataType`.
- `MediaEntry.ToString()` now returns a readable **position, title and duration** when a template is not applied. Missing titles have a readable placeholder.
- The UI still shows separate title and duration fields whenever the explicit template renders.
- A new `YtDlpUi.UiSmokeTests` project uses **Avalonia.Headless** to construct the actual compiled Download page and check visible playlist text on Windows and Linux CI. The simpler core and XAML regression tests remain.
- No changes to playlist parsing, downloads, authentication, or output formats.

## Verification and limits

Both desktop CI platforms run the new UI test as well as existing smoke tests. GitHub release packaging also reruns the headless UI checks before publishing binaries. The test confirms headless UI binding, not every physical display or supported media site.

The release remains an unsigned community pre-release with partial localization. Experimental Android is not packaged.

[Installation](../Installation/en.md) · [Features](../Features/en.md) · [Report an issue](https://github.com/afrowaveltd/yt-dlp-ui/issues)
