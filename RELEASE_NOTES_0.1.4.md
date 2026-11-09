# yt-dlp GUI v0.1.4 — Deno controls and Linux runtime discovery

This **maintenance pre-release** completes the Deno JavaScript-runtime support introduced in v0.1.3. The desktop GUI now displays Deno as a first-class external tool on both Windows and Linux.

## What's changed

- **Three status indicators:** yt-dlp, FFmpeg and Deno appear together in the desktop header. Deno shows a green/red availability indicator; hovering reveals the detected executable location.
- **Deno on the Tools page:** view the detected version and path, recheck detection, and install an optional personal copy on supported Windows/Linux x64 systems without administrator access.
- **Linux discovery:** locate a compatible Deno from `DENO_INSTALL/bin`, `~/.deno/bin`, `~/.local/bin`, `~/bin` or PATH, including GUI sessions that do not inherit interactive shell startup files.
- **Safe installation:** use official Deno archives and their SHA-256 checksums, verify runtime compatibility (2.3.0+), and avoid replacing an existing working installation on validation failure.
- **Regression coverage:** smoke tests verify the desktop indicators, Tools-page Deno controls and managed Linux Deno destination.
- **Aligned documentation:** installation, troubleshooting, feature guides and bilingual README now describe the actual released desktop behavior.

Windows Setup continues to offer **independent, initially selected** yt-dlp, FFmpeg and Deno options for a shared, administrator-approved installation. The new per-user Deno option in the running GUI is separate and optional. Neither setup nor the GUI silently downloads tools without a selected installation action.

## Downloads

| File | Platform |
|---|---|
| `yt-dlp-gui-setup-0.1.4-win-x64.exe` | Windows 10/11 x64 installer (UAC) |
| `yt-dlp-gui_0.1.4_win-x64_portable.zip` | Windows x64 portable |
| `yt-dlp-gui_0.1.4_amd64.deb` | Debian/Ubuntu x64 |
| `yt-dlp-gui-0.1.4-1.x86_64.rpm` | RPM-based Linux x64 |
| `yt-dlp-gui_0.1.4_linux-x86_64.tar.gz` | Linux x64 portable |
| `SHA256SUMS.txt` | Release asset checksums |

**Verification:** automated CI builds, smoke tests, package checks and checksum generation run before the release is published. Windows GUI installation and Deno indicators have also been manually checked by a project tester. Full manual installation and actual YouTube downloads across all Linux distributions remain to be verified.

**Known limitations:** unsigned community pre-release, partial localization, and site-dependent behavior of yt-dlp. macOS installation is not packaged. The experimental Android client is **not part of this desktop release**.

[English release details](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.4/Docs/Release-0.1.4/en.md) · [České poznámky](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.4/Docs/Release-0.1.4/cs.md) · [Report a bug](https://github.com/afrowaveltd/yt-dlp-ui/issues)
