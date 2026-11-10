# yt-dlp GUI v0.1.7 — Dependency installation fixes

Maintenance **pre-release** addressing Windows installer failures reproduced with v0.1.6.

## Fixed

- **yt-dlp Windows Setup:** download published `SHA2-256SUMS` to disk, then decode as UTF-8. Avoid Windows PowerShell converting a binary GitHub response to `System.Byte[]` text and losing the matching hash.
- **Deno Windows Setup and Tools tab:** recognize the official `Get-FileHash | Format-List` Windows checksum sidecar (`Algorithm`, `Hash`, `Path`) alongside GNU SHA-256 sums.
- Keep SHA-256 verification mandatory. Reject wrong asset names, unsupported algorithms and duplicate or ambiguous hashes; preserve working tools on failed replacement.
- Add C# Deno and PowerShell Windows Setup regression tests covering valid formats, invalid inputs and checksum file decoding.

## Verification

Windows and Linux CI, smoke tests, headless Avalonia UI tests and Windows Setup checks passed for the fix. A clean Windows installation with yt-dlp, FFmpeg and Deno selected was **manually verified on a second PC**, with normal non-admin application use afterward. Wider network/proxy combinations and Linux package installations were not manually covered.

## Downloads

| Asset | Platform |
| --- | --- |
| `yt-dlp-gui-setup-0.1.7-win-x64.exe` | Windows 10/11 x64 setup |
| `yt-dlp-gui_0.1.7_win-x64_portable.zip` | Windows x64 portable |
| `yt-dlp-gui_0.1.7_amd64.deb` | Debian/Ubuntu x64 |
| `yt-dlp-gui-0.1.7-1.x86_64.rpm` | RPM Linux x64 |
| `yt-dlp-gui_0.1.7_linux-x86_64.tar.gz` | Linux x64 portable |
| `SHA256SUMS.txt` | SHA-256 of artifacts |

**Limitations:** unsigned community pre-release, partly localized UI. Android prototype not included.

[English details](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.7/Docs/Release-0.1.7/en.md) · [Česky](https://github.com/afrowaveltd/yt-dlp-ui/blob/v0.1.7/Docs/Release-0.1.7/cs.md) · [Issues](https://github.com/afrowaveltd/yt-dlp-ui/issues)
