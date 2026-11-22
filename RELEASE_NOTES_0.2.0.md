# yt-dlp GUI v0.2.0 — built-in player and FFmpeg converter

**Feature pre-release** (second preview generation). Adds a player window for downloaded media and a converter tab powered by FFmpeg.

## Added

- **Player window** — opened from the header `▶` button or **Play** on a finished queue item. It lists media in the download folder, reads container/codec/resolution/duration/size with ffprobe, shows a preview frame, and provides a position slider that renders a new still at each position. **Play in system player** hands the file to the desktop's configured video player; the application deliberately ships no native playback engine.
- **Convert tab** — source file, target format (MP4, MKV, WebM, MOV, AVI, GIF, MP3, M4A, Opus, FLAC, WAV), quality tier, optional maximum height, optional trim range and optional audio removal.
- **Source protection** — converting a file into its own container writes `<name> (converted).<ext>`; the original is never overwritten. A cancelled conversion deletes its partial output.
- **Live conversion progress** — percentage and `N×` speed factor parsed from FFmpeg's `-progress pipe:1` output, with a working cancel button.
- **Documentation** — new [Player](Docs/Player/en.md) and [Converter](Docs/Converter/en.md) topics in English and Czech, plus updated features, versioning and index pages.

## Changed

- The tab strip now has **ten** tabs; **Convert** sits between *Files* and *Authentication*.
- Every new control, option label and status message is localized in English and Czech, and the converter's option labels re-translate live when the language changes.
- Default conversion quality is the **balanced** tier (CRF 23 / 192 kbit/s).

## Verification

- Offline smoke tests: FFmpeg argument mapping (video, audio, GIF, downscale, trim, `-an`, `-n`), output-path collision handling, time parsing, progress parsing, ffprobe JSON parsing including cover-art rejection, media folder scanning, target mapping and English/Czech locale parity.
- Headless Avalonia tests render the real converter page and player window, assert their controls are bound, and confirm live re-translation.
- Structural guards keep `ConverterTabIndex`/`ToolsTabIndex` aligned with the tab strip and require the player and converter entry points in the window.
- The conversion pipeline was executed against a real FFmpeg 7.1 build: MP4→MP3, →WebM (downscaled to 240p), →MKV (audio removed), trimmed MP4, →FLAC, →GIF, and a same-container conversion that keeps the original file.
- CI builds Windows x64 and Linux x64 packages and runs both test suites.

## Downloads

| Asset | Platform |
| --- | --- |
| `yt-dlp-gui-setup-0.2.0-win-x64.exe` | Windows 10/11 x64 setup |
| `yt-dlp-gui_0.2.0_win-x64_portable.zip` | Windows x64 portable |
| `yt-dlp-gui_0.2.0_amd64.deb` | Debian/Ubuntu x64 |
| `yt-dlp-gui-0.2.0-1.x86_64.rpm` | RPM Linux x64 |
| `yt-dlp-gui_0.2.0_linux-x86_64.tar.gz` | Linux x64 portable |
| `SHA256SUMS.txt` | SHA-256 of artifacts |

**Limitations:** unsigned community pre-release, partly localized UI, one conversion at a time, no subtitle/multi-track conversion options, and the Convert tab is not shown in the current screenshots. Windows Setup and Linux packages were produced by CI but were not manually reinstalled on a second PC in this round. Android prototype not included.

[English details](Docs/Release-0.2.0/en.md) · [Česky](Docs/Release-0.2.0/cs.md) · [Issues](https://github.com/afrowaveltd/yt-dlp-ui/issues)
