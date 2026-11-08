# yt-dlp GUI v0.1.2 — Windows setup permissions fix

This **maintenance pre-release** fixes the Windows setup requiring administrator privileges for installing yt-dlp and FFmpeg. No new download features were added.

## Fixed
- **UAC elevation:** Windows installer now requests administrator approval and installs the GUI machine-wide into Program Files.
- **Shared dependencies:** the existing independent checkboxes install SHA-256-verified yt-dlp and FFmpeg/ffprobe into `%ProgramData%\yt-dlp-ui\tools`, accessible to non-admin desktop users.
- **Reliable detection:** the GUI finds these shared tools automatically; user settings stay in each person's `%LOCALAPPDATA%`.
- **Validation/rollback:** optional tools must start successfully, with protection of existing executables if replacement fails.
- **Diagnostics:** optional dependency log moved to `%ProgramData%\yt-dlp-ui\logs\dependency-install.log`.

**Upgrade note:** v0.1.0 and v0.1.1 Windows setup installs were per-user. Uninstall the old per-user GUI before installing v0.1.2 to avoid duplicate shortcuts. Personal settings and tools are retained. Setup needs administrator credentials via UAC; everyday GUI use does not.

## Download

| Asset | Platform |
| --- | --- |
| `yt-dlp-gui-setup-0.1.2-win-x64.exe` | Windows x64 elevated installer |
| `yt-dlp-gui_0.1.2_win-x64_portable.zip` | Windows x64 portable |
| `yt-dlp-gui_0.1.2_amd64.deb` | Debian/Ubuntu x64 |
| `yt-dlp-gui-0.1.2-1.x86_64.rpm` | RPM-based Linux x64 |
| `yt-dlp-gui_0.1.2_linux-x86_64.tar.gz` | Portable Linux x64 |
| `SHA256SUMS.txt` | SHA-256 checksums |

**Testing:** Windows/Linux builds, smoke tests and installer package creation are automated. UAC consent and machine-wide installation still need manual verification on Windows. The GUI is MIT licensed; yt-dlp and FFmpeg have independent licenses. Binaries are unsigned and this is still a **pre-release**. The experimental Android application is not included.

[Installation guide (EN)](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.2/Docs/Installation/en.md) · [Instalace (CZ)](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.2/Docs/Installation/cs.md) · [Report an issue](https://github.com/afrowaveltd/yt-dlp-ui/issues)
