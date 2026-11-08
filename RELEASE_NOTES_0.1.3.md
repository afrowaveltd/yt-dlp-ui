# yt-dlp GUI v0.1.3 — Fix YouTube JavaScript runtime warning

This maintenance **pre-release** addresses the repeated yt-dlp warning **“No supported JavaScript runtime could be found”** on YouTube. Without a runtime, yt-dlp may miss formats or fail as YouTube changes.

## Fixed
- Automatically detect Deno (including shared Windows ProgramData, per-user folders and PATH) and pass `--js-runtimes deno:PATH` for video **analysis and downloads**.
- Windows Setup adds a **third independent, initially checked** option to install **Deno** alongside yt-dlp and FFmpeg; all are optional and downloads are consent-based.
- Download the official Deno ZIP with its SHA-256 sidecar, validate Deno ≥ 2.3.0, and retain a previous executable on failed replacement.
- Preserve explicit custom JavaScript runtime settings and advanced yt-dlp arguments.
- Add regression checks for runtime path handling, override precedence and Windows setup script integrity.

## Downloads

| File | Platform |
|---|---|
| `yt-dlp-gui-setup-0.1.3-win-x64.exe` | Windows 10/11 x64 setup (requires UAC) |
| `yt-dlp-gui_0.1.3_win-x64_portable.zip` | Windows x64 portable |
| `yt-dlp-gui_0.1.3_amd64.deb` | Debian/Ubuntu x64 |
| `yt-dlp-gui-0.1.3-1.x86_64.rpm` | RPM-based Linux x64 |
| `yt-dlp-gui_0.1.3_linux-x86_64.tar.gz` | Linux x64 portable |
| `SHA256SUMS.txt` | SHA-256 checksums |

Already have Deno? Just keep it updated or install manually with `winget install --id DenoLand.Deno -e`. Official standalone yt-dlp executables already include EJS scripts; other installation methods may need a matching yt-dlp-ejs package. See the [yt-dlp EJS guide](https://github.com/yt-dlp/yt-dlp/wiki/EJS).

**Known limits:** CI confirms build, test and package creation; interactive Deno setup and actual YouTube downloading require manual confirmation. Unsigned community pre-release. The Android prototype is not included.

[Dokumentace česky](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.3/Docs/Release-0.1.3/cs.md) · [English notes](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.3/Docs/Release-0.1.3/en.md) · [Report a bug](https://github.com/afrowaveltd/yt-dlp-ui/issues)
