# Release 0.1.7 — Fixed yt-dlp and Deno dependency installation

[Česky](cs.md) · [Documentation](../Documentation/en.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.7)

In v0.1.6 the Windows installer failed to find the published yt-dlp.exe checksum. The Deno installer rejected the official Windows checksum sidecar. FFmpeg installed correctly.

- Windows Setup now explicitly downloads and decodes `SHA2-256SUMS` as UTF-8 instead of casting HTTP binary content to a string.
- Both machine-wide and per-user Deno installation recognize Windows PowerShell `Get-FileHash` checksum output as well as GNU format.
- Publisher SHA-256 verification remains mandatory and rejects unrelated, duplicate or malformed checksum records.
- New regression tests exercise PowerShell and C# parsing and decoding.

Windows and Linux CI passed for the underlying fix; the Windows Setup installation of all three optional tools was manually confirmed on a second PC. This does not mean all proxies, networks or Linux package manager configurations were tested.

The GUI is still an unsigned, partially localized community pre-release; Android is not included. Download, playlist and FFmpeg behavior are otherwise unchanged.

[Installation](../Installation/en.md) · [Troubleshooting](../Troubleshooting/en.md) · [Full release notes](../../RELEASE_NOTES_0.1.7.md)
