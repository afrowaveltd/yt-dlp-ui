# yt-dlp GUI v0.1.5 — Playlist preview display fix

This **desktop maintenance pre-release** fixes the playlist preview introduced in earlier builds, where the entry list showed `YtDlpUi.Core.Models.MediaEntry` instead of the video titles.

## Fixed

- The playlist entries in **Download → playlist preview** now use the correct Avalonia `MediaEntry` data template.
- Each entry displays its **playlist position and video title**, with the **duration** alongside it where available.
- The UI no longer falls back to printing the model's class name.
- Added a smoke regression check that verifies the template's model type and the title/duration bindings.

The fix is **presentation-only**; playlist parsing, downloaded media, queue processing and output formats are unchanged. Features from v0.1.4 (Deno in the header and Tools panel, automatic Linux discovery, user-installable Deno) remain available.

## Downloads

| File | Platform |
|---|---|
| `yt-dlp-gui-setup-0.1.5-win-x64.exe` | Windows 10/11 x64 setup (UAC) |
| `yt-dlp-gui_0.1.5_win-x64_portable.zip` | Windows x64 portable |
| `yt-dlp-gui_0.1.5_amd64.deb` | Debian/Ubuntu x64 |
| `yt-dlp-gui-0.1.5-1.x86_64.rpm` | RPM-based Linux x64 |
| `yt-dlp-gui_0.1.5_linux-x86_64.tar.gz` | Linux x64 portable |
| `SHA256SUMS.txt` | SHA-256 checksums |

## Verification and limitations

GitHub Actions builds both desktop platforms, runs smoke checks and verifies release packages and SHA-256 sums before publication. The playlist regression is automatically checked in CI. Actual downloads depend on yt-dlp and the supported service, and interactive UI testing is still recommended.

This remains an **unsigned community pre-release** with partially localized UI. The experimental **Android application is not included**.

[English details](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.5/Docs/Release-0.1.5/en.md) · [České poznámky](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.5/Docs/Release-0.1.5/cs.md) · [Report a bug](https://github.com/afrowaveltd/yt-dlp-ui/issues)
