# yt-dlp GUI 0.1.1 — Maintenance pre-release

This **PATCH** release fixes bugs and improves reliability/security. It introduces no new user-facing features.

## Fixed

- Language selector globe no longer gets cut off by the Linux emoji font: replaced with a theme-aware vector icon and fixed spacing.
- Each queued URL keeps its own format, output folder and temporary authentication settings from enqueue time; later edits do not change queued downloads.
- Active queue entries cannot be silently removed while still downloading.
- yt-dlp standalone install now checks the exact publisher SHA-256 before replacing an executable and preserves the previous binary on failure.
- FFmpeg updates use staging and backup/rollback to preserve a working installation.
- Settings write via an atomic rename; turning off remembered options clears older snapshots.
- Raw yt-dlp command previews are no longer automatically logged, reducing exposure of passwords, cookies and tokens.

Expanded Windows/Linux smoke tests cover SHA-256 mismatches, file preservation, independent option snapshots and settings roundtrips.

## Download

| Asset | Platform |
|---|---|
| `yt-dlp-gui-setup-0.1.1-win-x64.exe` | Windows 10/11 x64 installer |
| `yt-dlp-gui_0.1.1_win-x64_portable.zip` | Windows x64 portable |
| `yt-dlp-gui_0.1.1_amd64.deb` | Debian/Ubuntu x64 |
| `yt-dlp-gui-0.1.1-1.x86_64.rpm` | RPM-based Linux x64 |
| `yt-dlp-gui_0.1.1_linux-x86_64.tar.gz` | Portable Linux x64 |
| `SHA256SUMS.txt` | Package SHA-256 checksums |

Builds include .NET runtime but **not** the separate yt-dlp or FFmpeg tools. Windows setup can install them; Linux packages recommend them. The GUI is MIT, external tools have separate licenses.

**Known limitations:** this remains an **unsigned pre-release**. Some advanced UI text is not localized, download success depends on upstream sites, and every installer path or Linux distribution has not been manually tested. **Android is not part of v0.1.1**.

[Documentation EN](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.1/Docs/Release-0.1.1/en.md) · [Dokumentace CZ](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.1/Docs/Release-0.1.1/cs.md) · [GitHub Issues](https://github.com/afrowaveltd/yt-dlp-ui/issues)

Previous v0.1.0 remains available and unchanged.
