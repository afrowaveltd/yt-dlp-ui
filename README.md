# yt-dlp GUI

A friendly **cross-platform desktop interface for [yt-dlp](https://github.com/yt-dlp/yt-dlp)**, built with [Avalonia UI](https://avaloniaui.net/) and .NET 10. Paste a video or playlist URL, choose a preset, and manage downloads without memorizing command-line options. Advanced yt-dlp options remain accessible when you need them.

**Release status:** **[v0.1.0 — first public preview](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)** is available for Windows x64 and Linux x64. It is a community pre-release; feedback and bug reports are welcome.

**Documentation:** English (this page) · [Česky](Docs/README/cs.md) · [Documentation index](Docs/Documentation/en.md) · [Český rozcestník](Docs/Documentation/cs.md)

## What you get

- **Simple downloads:** clipboard URL detection, drag-and-drop, video/playlist analysis, thumbnails, quality presets (up to 4K), audio-only choices, queue and progress.
- **Nine tabs:** Download, Format, Playlist, Subtitles & metadata, Network, Files, Authentication, Advanced and Tools.
- **Power-user options:** format selection, conversions, subtitles, metadata, filters, proxies, filename templates, authentication/cookies, and a copyable yt-dlp command preview.
- **External tools:** yt-dlp detection/update and FFmpeg detection; an optional Windows installer downloads yt-dlp and FFmpeg with SHA-256 verification.
- **Localization:** language selector in the upper-right header beside the tool status indicators. English/Czech dictionaries, automatic discovery of new `Locales/*.json` files, instant switching and English fallback.
- **Desktop packaging:** Windows x64 `setup.exe` and portable build; Linux `.deb`, `.rpm` and portable `.tar.gz`.

Some advanced tabs, runtime messages and dialogs still contain Czech text. **Localization is partial in this 0.1.0 candidate.** A built or packaged artifact is not proof that all installation and download scenarios have been manually tested.

## Screenshots

[![Main window, URL and presets](screenshots/main_screen.png)](screenshots/main_screen.png)

| | |
|---|---|
| [![Format tab](screenshots/format_screen.png)](screenshots/format_screen.png) **Format** | [![Playlist tab](screenshots/playlist_screen.png)](screenshots/playlist_screen.png) **Playlist** |
| [![Subtitles and metadata tab](screenshots/titulky_screen.png)](screenshots/titulky_screen.png) **Subtitles & metadata** | [![Network tab](screenshots/sit_screen.png)](screenshots/sit_screen.png) **Network** |
| [![Files tab](screenshots/soubory_screen.png)](screenshots/soubory_screen.png) **Files** | [![Authentication tab](screenshots/prihlaseni_screen.png)](screenshots/prihlaseni_screen.png) **Authentication** |
| [![Advanced tab](screenshots/pokrocile_screen.png)](screenshots/pokrocile_screen.png) **Advanced** | [![Tools tab](screenshots/nastroje_screen.png)](screenshots/nastroje_screen.png) **Tools** |

Screenshots illustrate the interface at the time they were captured; newer language and installer controls may differ. Click an image for its full size.

## Install or run

### Windows 10/11 (x64)

Download **the Windows setup EXE** from [v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0) and run `yt-dlp-gui-setup-0.1.0-win-x64.exe`. The installer creates Start-menu shortcuts, supports normal uninstall, and offers two **independent, initially selected** options: install yt-dlp and install FFmpeg. Downloads require an internet connection and use publisher-provided SHA-256 checksums. You may clear either checkbox or install the tools later. Setup is a per-user install without elevation.

A **`yt-dlp-gui_0.1.0_win-x64_portable.zip`** portable build is also available in the release. Windows ARM64 has a separate manual publish command, but is **not** covered by the CI installer.

### Debian / Ubuntu, Fedora / openSUSE, other Linux

Download the matching **DEB or RPM** from [v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0), then use your distribution's package manager:

~~~bash
sudo apt install ./yt-dlp-gui_0.1.0_amd64.deb    # Debian/Ubuntu x64
sudo dnf install ./yt-dlp-gui-0.1.0-1.x86_64.rpm # Fedora-style RPM
~~~

Package filenames depend on version and architecture. DEB/RPM **recommend** `yt-dlp` and `ffmpeg` but do not bundle them or run hidden download scripts. Package-manager behavior differs by distribution. For other Linux systems, unpack the portable `tar.gz` and run `./yt-dlp-gui/yt-dlp-gui`. An existing compatible .NET runtime is **not** required by the self-contained builds.

For a per-user Linux installation from source, run `./scripts/install.sh`; for removal, `./scripts/install.sh --uninstall`.

**v0.1.0 is a published, unsigned pre-release.** See [Installation](Docs/Installation/en.md) for exact build commands, dependencies and platform limitations.

## Build from source

Install the **.NET 10 SDK** (see `global.json`) and clone the repository:

~~~bash
git clone https://github.com/afrowaveltd/yt-dlp-ui.git
cd yt-dlp-ui
dotnet restore YtDlpUi.slnx
dotnet build YtDlpUi.slnx -c Release
dotnet run --project src/YtDlpUi/YtDlpUi.csproj
dotnet run --project tests/YtDlpUi.SmokeTests/YtDlpUi.SmokeTests.csproj -c Release
~~~

`yt-dlp` is required to download; `ffmpeg` / `ffprobe` are needed for merging and many conversion workflows. Use the **Tools** tab to check their status or choose custom paths.

## Localization in one minute

The language selector is in the **top-right header** next to the yt-dlp/FFmpeg indicators. Choose your language and it updates already localized controls immediately.

To add a translation without rebuilding, create a JSON dictionary such as `pt-BR.json` in:

- Linux: `~/.config/yt-dlp-ui/Locales/` (or `$XDG_CONFIG_HOME/yt-dlp-ui/Locales/`)
- Windows: `%LOCALAPPDATA%\yt-dlp-ui\Locales\`
- Alternatively: `Locales/` next to the application executable

~~~json
{
  "Download": "Baixar",
  "About": "Sobre"
}
~~~

Reopen the language selector to rescan files. Filenames are culture codes (e.g. `fr.json`, `pt-BR.json`); keys are English UI strings. Missing or empty translations fall back to the English key. Custom dictionaries take precedence over bundled ones. More: [Localization](Docs/Localization/en.md).

## Documentation

| Topic | English | Česky |
|---|---|---|
| Documentation index | [Index](Docs/Documentation/en.md) | [Rozcestník](Docs/Documentation/cs.md) |
| Getting started | [Guide](Docs/Getting-Started/en.md) | [Průvodce](Docs/Getting-Started/cs.md) |
| Installation and packages | [Install](Docs/Installation/en.md) | [Instalace](Docs/Installation/cs.md) |
| Features and workflow | [Features](Docs/Features/en.md) | [Funkce](Docs/Features/cs.md) |
| Localization | [Locales](Docs/Localization/en.md) | [Jazyky](Docs/Localization/cs.md) |
| Configuration and storage | [Settings](Docs/Configuration/en.md) | [Nastavení](Docs/Configuration/cs.md) |
| Troubleshooting | [Help](Docs/Troubleshooting/en.md) | [Řešení problémů](Docs/Troubleshooting/cs.md) |
| Development and verification | [Development](Docs/Development-and-Verification/en.md) | [Vývoj](Docs/Development-and-Verification/cs.md) |
| Versioning and release policy | [Versioning](Docs/Versioning/en.md) | [Verzování](Docs/Versioning/cs.md) |
| First release status | [Release checklist](Docs/Release-0.1.0/en.md) | [Kontrolní seznam](Docs/Release-0.1.0/cs.md) |

## License, privacy and attribution

The **GUI source code** is licensed under [MIT](LICENSE). [yt-dlp](https://github.com/yt-dlp/yt-dlp) and [FFmpeg](https://ffmpeg.org/) are separate third-party projects with their **own licenses**; installing their binaries does not change the GUI license. No application telemetry or ads are included. Metadata lookup, downloads, dependency setup and update checks can still contact their respective online services when requested or enabled.

Remembered download options deliberately omit supported password and 2FA fields. Treat downloaded files, cookies and logs according to your own privacy needs. Download only media you are authorized to access and respect site terms.

**[v0.1.0 is published](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)** as our first community pre-release. See [release status and outstanding tests](Docs/Release-0.1.0/en.md).
