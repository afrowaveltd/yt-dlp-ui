# Release 0.1.5 — Playlist preview display fix

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.5)

**Maintenance pre-release for the Windows/Linux desktop application.**

## What changed

When analyzing a playlist, the list under the playlist title mistakenly displayed `YtDlpUi.Core.Models.MediaEntry` instead of video titles. Its Avalonia item template had been declared for a different model, `LogLine`.

- The playlist list template now matches the real **`MediaEntry`** collection type.
- Entries display **index and title** through `Display`, and **duration** when available.
- A new regression smoke test checks the template type, XML namespace and display/duration bindings.
- Playlist analysis, downloads, format options and queue behavior are **unchanged**.

This is a display-only fix; no migration or special configuration is needed. Deno functionality introduced in v0.1.3 and v0.1.4 remains unchanged.

## Verification

Windows and Linux CI builds, smoke tests, packaging checks and SHA-256 release manifest run before publishing. CI can detect incorrect template wiring, but it does not simulate every playlist or external website. Interactive manual testing is recommended.

This is an **unsigned pre-release**; localization is not yet complete. No Android APK is included in the desktop release.

[Installation](../Installation/en.md) · [Features](../Features/en.md) · [Report an issue](https://github.com/afrowaveltd/yt-dlp-ui/issues)
