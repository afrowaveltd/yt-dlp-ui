# yt-dlp GUI v0.1.6 — Reliable playlist preview rendering

This **desktop maintenance pre-release** fixes a playlist rendering problem that remained visible in v0.1.5: the analyzed playlist entries still displayed `YtDlpUi.Core.Models.MediaEntry` instead of their titles.

## Fixed

- Remove the unnecessary `DataType` selector from the playlist list's **explicit Avalonia `ItemTemplate`**, making the template unconditional for each item.
- Add human-readable `MediaEntry.PreviewText` / `ToString()` output. Even if a platform falls back to default list item rendering, it shows the **position, title and available duration** instead of a .NET type name.
- Preserve the formatted duration column when the explicit item template is applied.
- Add **real Avalonia headless UI tests** that load the compiled Download page, bind sample playlist entries, and verify the visible text controls on **Linux and Windows**. Keep model and XAML regression checks too.

This change affects playlist preview presentation only. The playlist parser, queue, download process, conversion and existing Deno functionality are unchanged.

## Downloads

| File | Platform |
|---|---|
| `yt-dlp-gui-setup-0.1.6-win-x64.exe` | Windows 10/11 x64 Setup (UAC) |
| `yt-dlp-gui_0.1.6_win-x64_portable.zip` | Windows x64 portable |
| `yt-dlp-gui_0.1.6_amd64.deb` | Debian / Ubuntu x64 |
| `yt-dlp-gui-0.1.6-1.x86_64.rpm` | RPM-based Linux x64 |
| `yt-dlp-gui_0.1.6_linux-x86_64.tar.gz` | Linux x64 portable |
| `SHA256SUMS.txt` | SHA-256 checksums |

## Verification

Both Windows and Linux CI must pass compilation, command-line/model smoke checks, **real headless XAML rendering checks**, and package validation before publication. Headless tests exercise UI layout and bindings but are not a substitute for every installed desktop configuration or real media download.

This remains an **unsigned community pre-release** with partially localized UI. The experimental Android client is **not included**.

[English details](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.6/Docs/Release-0.1.6/en.md) · [Česky](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.6/Docs/Release-0.1.6/cs.md) · [Issues](https://github.com/afrowaveltd/yt-dlp-ui/issues)
