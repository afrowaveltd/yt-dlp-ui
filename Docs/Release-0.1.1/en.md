# Maintenance pre-release v0.1.1

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [Versioning](../Versioning/en.md)

**[Download v0.1.1](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1)** · [Previous v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)

This is the first **PATCH** update to our public desktop preview. It introduces **no new user-facing features**. Existing configuration locations and application identity remain unchanged.

## Fixes

1. **Language globe:** replace the partially cropped emoji with a vector icon. A dedicated column and an 18px spacer keep it separate from the language dropdown on Windows/Linux.
2. **Queued downloads:** capture each item's format, output folder and temporary authentication options when queued; subsequent UI edits do not change waiting transfers. Retry keeps the captured configuration.
3. **Queue safety:** a running download cannot be removed from the visible queue without first cancelling it.
4. **yt-dlp installation:** check the exact standalone executable's SHA-256 against the official `SHA2-256SUMS` before replacement; interrupted downloads are cleaned up, and the previous executable remains on error.
5. **FFmpeg installation:** stage validated files and keep the previous FFmpeg/FFprobe directory as a rollback backup until the swap succeeds.
6. **Settings:** write complete JSON to a temporary file and atomically rename it, avoiding truncated `settings.json` after an interrupted write. Disabling remembered options clears any old snapshot.
7. **Sensitive logs:** stop automatically appending raw yt-dlp command previews to the log. These may contain passwords, cookie paths, custom headers or tokens. The on-screen command preview remains available.

## Testing and artifacts

Offline smoke tests cover exact and duplicate checksum entries, successful/rejected fake downloads, preservation of an already installed binary, independent queue snapshots, staged tool replacement and settings save/restore, alongside existing localization and command tests.

CI builds Windows x64 setup and portable ZIP, Linux x64 DEB/RPM/tar.gz and `SHA256SUMS.txt`. The release workflow publishes them only after Linux and Windows tests pass. CI does **not** substitute for testing real GUI launches, every target distribution, website or installer path.

## Upgrade notes

- Existing settings and external tools remain in place. Reinstalling yt-dlp/FFmpeg is not necessary just to update the GUI.
- Windows: run the new setup or replace the entire portable folder. Debian/Ubuntu: upgrade using APT and the new DEB. RPM systems: upgrade using the package manager.
- This patch does **not** include the experimental Android application; it remains under development toward `0.2.0`.
- The GUI remains MIT; yt-dlp and FFmpeg have their own licenses. Binaries are unsigned. Advanced-tab localization is still incomplete.

[Report bugs and suggestions](https://github.com/afrowaveltd/yt-dlp-ui/issues) with sanitized logs, OS version and tool versions. Never publish cookies or passwords. The historical `v0.1.0` tag remains untouched.
