# yt-dlp GUI 0.1.0 — First public preview

Welcome to the first **public preview** of **yt-dlp GUI**, an open-source Avalonia/.NET 10 desktop frontend for [yt-dlp](https://github.com/yt-dlp/yt-dlp).

This is a **pre-release**: community testing, bug reports and improvements are welcome.

## Highlights

- URL analysis, clipboard and drag-and-drop, thumbnails, quality and audio-only presets.
- Download queue, progress reporting, retries, and copyable yt-dlp command preview.
- Nine tabs covering formats, playlists, subtitles/metadata, network, files, authentication, advanced settings and tools.
- English/Czech JSON dictionaries, language selector in the header and optional extra `Locales/*.json` dictionaries without recompilation.
- Light/dark theme and saved settings.
- Windows x64 setup wizard with two separate optional checkboxes to install yt-dlp and FFmpeg, verified using publisher SHA-256 checksums.
- Debian/Ubuntu DEB, RPM for Fedora/openSUSE-style systems and portable Linux x64 packages.

## Download

| Asset | Intended platform |
| --- | --- |
| `yt-dlp-gui-setup-0.1.0-win-x64.exe` | Windows 10/11 x64 installer |
| `yt-dlp-gui_0.1.0_win-x64_portable.zip` | Portable Windows x64 |
| `yt-dlp-gui_0.1.0_amd64.deb` | Debian/Ubuntu x64 |
| `yt-dlp-gui-0.1.0-1.x86_64.rpm` | RPM-based Linux x64 |
| `yt-dlp-gui_0.1.0_linux-x86_64.tar.gz` | Portable Linux x64 |
| `SHA256SUMS.txt` | SHA-256 checksums |

Unpack portable archives completely before starting the app. Packages include the .NET runtime. **yt-dlp is required** to download; FFmpeg/ffprobe support merging and conversion. The GUI installers do **not** bundle these external executables.

## Preview limitations

- Only part of the user interface is currently translated; some advanced controls and messages remain in Czech.
- The Windows setup is x64 only. No macOS installation package is provided.
- CI confirms builds, smoke tests and package structure, not every real download site or interactive installation path.
- Binaries are not code-signed. Verify the provided SHA-256 values.
- Download only media you are authorized to access; respect site terms and copyright.

## Documentation and feedback

- [README (English)](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.0/README.md) · [README (česky)](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.0/Docs/README/cs.md)
- [Documentation (English)](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.0/Docs/Documentation/en.md) · [Dokumentace (česky)](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.0/Docs/Documentation/cs.md)
- [Report a problem or suggest an improvement](https://github.com/afrowaveltd/yt-dlp-ui/issues)

The GUI source is released under [MIT](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.0/LICENSE). yt-dlp and FFmpeg have independent licenses. No application telemetry or advertising.
