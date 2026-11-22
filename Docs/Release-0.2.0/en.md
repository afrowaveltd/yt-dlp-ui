# Release 0.2.0 — built-in player and FFmpeg converter

[Česky](cs.md) · [Documentation](../Documentation/en.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.2.0)

This is a **feature pre-release**, the second preview generation. It adds two capabilities that were previously only available by leaving the application: inspecting what you downloaded, and converting it.

## Player window

- Open with **▶** in the header or **Play** on a finished queue item.
- Lists every video and audio file in the download folder, newest first.
- Shows container, codecs, resolution, duration and size, read with **ffprobe**.
- Renders a **frame you can scrub** with a position slider; each position produces a new still.
- **Play in system player** opens the file in your desktop's video player. The application still bundles no native playback engine, so it stays small and platform-neutral.

## Convert tab

- Source file, target format, quality tier, optional maximum height, optional trim, optional "remove audio".
- Targets: MP4, MKV, WebM, MOV, AVI, GIF, MP3, M4A, Opus, FLAC, WAV.
- Live percentage and speed factor from FFmpeg's machine-readable progress output; the job can be cancelled.
- **The source file is never overwritten.** Converting a file into its own container writes `<name> (converted).<ext>`.
- A cancelled conversion removes its partial output.
- The produced file can be played or revealed from the same page.

## Localization and documentation

- Every new control, option label and status message has an English and a Czech entry; option labels re-translate live when the language changes.
- New docs: [Player](../Player/en.md) and [Converter](../Converter/en.md), plus updated features, versioning and index pages.

## Testing

- Offline smoke checks cover the FFmpeg argument mapping (video, audio, GIF, downscale, trim, `-an`, `-n`), output-path collision handling, time parsing, progress parsing, ffprobe JSON parsing (including cover art that must not count as video), media folder scanning, target mapping and locale parity.
- Headless Avalonia tests render the **real** converter page and **player window**, verify their controls are bound, and confirm the converter's option labels switch language in place.
- Structural guards keep the new tab indices aligned with the tab strip and require the player and converter entry points to exist in the window.
- The conversion pipeline itself was additionally executed against a real FFmpeg 7.1 build: MP4→MP3, →WebM (downscaled), →MKV (audio removed), trimmed MP4, →FLAC, →GIF, plus a same-container conversion that must keep the original file.

## Known limitations

- The player window previews frames; continuous playback is delegated to the system player by design.
- One conversion runs at a time, and subtitle/multi-track handling is not exposed.
- The Convert tab is not shown in the current screenshots.
- The GUI remains an **unsigned, partially localized** community pre-release; several advanced tabs still contain Czech-only labels.
- Windows Setup and Linux packaging were built by CI, but this release was not manually reinstalled on a second PC the way 0.1.7 was.

[Installation](../Installation/en.md) · [Troubleshooting](../Troubleshooting/en.md) · [Full release notes](../../RELEASE_NOTES_0.2.0.md)
