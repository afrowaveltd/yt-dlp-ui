# Player

[Česky](cs.md) · [Index](../Documentation/en.md) · [Features](../Features/en.md)

The player window lets you look through what you have already downloaded without leaving the application. Open it with the **▶** button in the header, or with **Play** on a finished item in the download queue.

## What the window shows

| Area | Contents |
|---|---|
| File list | every video and audio file in the current download folder, newest first |
| Preview | one frame taken by FFmpeg from the selected position |
| Position slider | moves through the file; each position renders a new frame |
| Details | container, video and audio codec, resolution, duration and file size |
| Actions | **Play in system player**, **Convert this file**, **Open folder** |

Press **Refresh** after downloading something new; the window does not watch the folder continuously.

## How playback works

The window renders **still frames**, it does not decode video into the application. The **Play in system player** button opens the file in whatever video player your desktop is configured to use — for example VLC, mpv, Celluloid or the default GNOME/KDE player.

This is a deliberate design choice: the application does not bundle or link a native playback engine, so it stays small and behaves the same on every desktop. If no player is associated with the file type, the window reports that instead of failing silently.

Audio-only files show a note instead of a preview image and still open in your audio player.

## Converting from the player

**Convert this file** selects the file in the **Convert** tab and switches to it, so the playback you just checked can be turned into another format in one step. See [Converter](../Converter/en.md).

## Requirements and limitations

- Preview frames need **FFmpeg**; without it the list and details still work but no picture appears.
- Technical details come from **ffprobe**, which normally ships next to FFmpeg.
- Only files inside the configured download folder are listed. Change that folder on the **Download** or **Files** tab.
- The window shows one folder, not your whole disk; use **Open folder** to reach the file in your file manager.
- Very large or unusual containers may fail to produce a preview frame; playback is unaffected.

[Installation](../Installation/en.md) · [Troubleshooting](../Troubleshooting/en.md)
