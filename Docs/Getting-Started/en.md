# Getting started

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [Installation](../Installation/en.md)

## 1. Prerequisites

- Windows 10/11 x64 or a suitable Linux desktop with a graphical session.
- `yt-dlp` installed or selected in the Windows setup wizard; it performs the actual downloading.
- `ffmpeg` / `ffprobe` recommended for merging video/audio and conversion. Both external projects have their own licenses.
- Internet access to retrieve metadata and media from sites.

The Windows setup and Linux DEB/RPM/tar files are self-contained: you do **not** need to install the .NET runtime to run them. Developing from source requires the .NET 10 SDK.

## 2. Choose a distribution

- **Windows x64:** download `yt-dlp-gui-windows-setup` from [GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions). Extract the archive and run the setup executable. Leave the independent **Install yt-dlp** and **Install FFmpeg** boxes checked for the simplest first run.
- **Debian/Ubuntu:** install the matching `.deb` with `sudo apt install ./filename.deb`; the package manager may install recommended dependencies.
- **Fedora/openSUSE:** use the matching `.rpm` with the appropriate package manager.
- **Other Linux:** unpack the `.tar.gz` archive and launch the included executable; install external tools separately as needed.
- **Developers:** clone the source and run the commands in [Development and verification](../Development-and-Verification/en.md).

These are **CI build artifacts** until the 0.1.0 GitHub Release is actually published.

## 3. First download

1. Copy a direct video or playlist URL (or drag a URL onto the main window).
2. The clipboard watcher may prefill the URL; otherwise paste it into **Download**.
3. Click **Analyze** to inspect title, thumbnail and available formats (or proceed directly to a preset).
4. Choose a preset such as Best, Full HD or Audio only; optionally adjust advanced tabs.
5. Select the output folder and click **Download**, or **Add to queue** to queue more links.
6. Follow progress; use **Open folder** when finished.

The header indicates whether yt-dlp and FFmpeg were detected. Open **Tools** when either is missing. The **language dropdown** is at the upper-right, next to the status indicators.

## 4. Useful shortcuts

| Shortcut | Action |
|---|---|
| `Ctrl+L` | Focus URL input |
| `F5` | Analyze the current URL |
| `Ctrl+Enter` | Start downloading |

## 5. Next

- [All nine tabs](../Features/en.md)
- [Settings and storage](../Configuration/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

**Note:** download results depend on supported sites, cookies, the remote service and the installed yt-dlp version. Make sure you have permission to download the content.
