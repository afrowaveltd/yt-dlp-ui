# Features and user workflow

[Česky](cs.md) · [Index](../Documentation/en.md) · [Getting started](../Getting-Started/en.md)

The interface is designed to be friendly for a first download while keeping advanced yt-dlp options available. The **0.1.5 maintenance preview** contains nine tabs:

| Tab | Purpose |
|---|---|
| **Download** | URL, clipboard and drag-and-drop, analysis and thumbnail, preset, output folder, start/queue |
| **Format** | Available streams, format selection/sorting, merge containers, audio extraction and remux/conversion, video sections |
| **Playlist** | Playlist handling, ranges/order, archive, filters, failure policies and livestream choices |
| **Subtitles & metadata** | Subtitle languages/formats, embedding, thumbnails, metadata, chapters, SponsorBlock |
| **Network** | Proxy, timeouts, retries, IPv4/IPv6, download limits, headers and external downloader |
| **Files** | Filename/output templates, destination/temp folders, overwrites, cache and filename options |
| **Authentication** | Browser/file cookies, credentials, `.netrc` and certificate-related yt-dlp switches |
| **Advanced** | Generated command preview, extra arguments, diagnostics and option reset |
| **Tools** | Locate/install/update yt-dlp, detect/install FFmpeg, detect/install Deno on supported Windows/Linux systems, application settings |

Some specialized yt-dlp flags remain in the **custom arguments** area rather than having a dedicated control. A GUI option maps to arguments passed to the installed external yt-dlp version; available website support is controlled by yt-dlp.

## Fast path

1. Copy/paste a URL or drag it onto the window.
2. Click **Analyze** to inspect media details, if desired.
3. Choose a quality preset or **audio-only** preset.
4. Select the output folder; click **Download** or **Add to queue**.
5. Follow queue status and progress; use the log or open the output folder.

A clipboard watcher and automatic analysis can be enabled or disabled in application settings.

## Advanced path

Use the **Format**, **Playlist**, **Subtitles & metadata**, **Network**, **Files** and **Authentication** tabs to choose detailed yt-dlp behavior. The **Advanced** tab displays a copyable preview of the resulting command and accepts extra arguments for less common switches. Be careful with custom flags and authentication data.

After analyzing a playlist, the Download tab previews each entry with its position, title and duration when known.

The right side of the window shows the queue, with progress and controls for retry/removal. The log helps diagnose failed downloads.

## Header and UI controls

The header contains **yt-dlp, FFmpeg and Deno status indicators**. The 🌐 **language selector** is beside them, followed by the light/dark theme control. Languages come from JSON dictionaries, not a hard-coded two-language menu. See [Localization](../Localization/en.md).

| Shortcut | Action |
|---|---|
| `Ctrl+L` | Focus the link field |
| `F5` | Analyze link |
| `Ctrl+Enter` | Start download |

## Known limitations

- Multiple dialogs, status messages and advanced controls have not yet been translated into every UI language.
- No macOS-specific installer or automated macOS build exists.
- External tools, service availability, and network access affect real download results.
- The automated smoke suite is not an end-to-end verification of every yt-dlp switch or target website.

[Help](../Troubleshooting/en.md) · [Release readiness](../Release-0.1.0/en.md)
