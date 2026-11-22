# FFmpeg converter

[Česky](cs.md) · [Index](../Documentation/en.md) · [Features](../Features/en.md)

The **Convert** tab turns a file you already have into another format. It is a thin, explicit front end to FFmpeg: you choose what the result should be, the page shows the exact file that will be produced, and nothing is written until you press **Convert**.

## Workflow

1. Choose the **source file**. The path box accepts any file; **Browse…** opens a picker filtered to media.
2. The page reads the file with **ffprobe** and shows its summary (codec, resolution, duration, size).
3. Choose the **format**, **quality** and optionally a **maximum height**.
4. Optionally set **trim** points, or tick **Remove audio**.
5. Optionally choose an **output folder**; the exact destination is shown before you start.
6. Press **Convert**. Progress and speed appear on the page and in the main log.

## Target formats

| Group | Formats |
|---|---|
| Video | MP4 (H.264 + AAC), MKV, WebM (VP9 + Opus), MOV, AVI, GIF |
| Audio | MP3, M4A (AAC), Opus, FLAC, WAV |

MP4 is the safest choice for televisions, phones and browsers, and it is written with the index at the front (`+faststart`) so playback can begin before the file is fully read. MKV accepts almost any codec. WebM is free-format but slower to encode.

Audio targets keep only the soundtrack and drop the picture automatically.

## Quality tiers

| Tier | Video | Audio |
|---|---|---|
| High | CRF 18, slow encoder preset | 320 kbit/s (Opus/WebM: 160) |
| Balanced (default) | CRF 23, medium preset | 192 kbit/s (Opus/WebM: 128) |
| Small | CRF 28, veryfast preset | 128 kbit/s (Opus/WebM: 96) |

Lossless targets (FLAC, WAV) ignore the quality tier.

## Downscaling and trimming

- **Maximum height** limits the picture height. Smaller sources are never upscaled, and **Keep the original size** leaves the resolution untouched.
- **Trim from** and **Trim to** accept seconds (`90`), minutes (`1:30`) or hours (`00:01:30`). Leaving both empty converts the whole file. Seeking happens before the input, so trimming a long file is fast and still frame-accurate.

## The original file is always protected

Conversion never overwrites its own source. Converting `clip.mp4` into MP4 produces `clip (converted).mp4`. An existing output file is replaced only when **Overwrite an existing file** is ticked.

A cancelled conversion removes the partial output instead of leaving a broken file behind.

## Requirements and limitations

- **FFmpeg** and **ffprobe** must be available. The **Tools** tab shows their status and can install them on Windows.
- Conversion runs at whatever speed the CPU manages; the page shows an `N×` speed factor and lets you cancel.
- Only one conversion runs at a time; the button is disabled while a job is active.
- The page exposes the common cases. Anything not covered here can be passed to yt-dlp through the **Advanced → extra arguments** box when downloading, or by using FFmpeg directly.
- Subtitle and multi-track handling is not exposed; FFmpeg keeps the default streams.

[Player](../Player/en.md) · [Troubleshooting](../Troubleshooting/en.md)
