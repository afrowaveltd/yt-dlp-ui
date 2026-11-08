# yt-dlp GUI v0.1.3 — YouTube JavaScript runtime support

[Česky](cs.md) · [Documentation](../Documentation/en.md) · [Versioning](../Versioning/en.md)

**[Download v0.1.3](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.3)** · [Previous v0.1.2](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.2)

This maintenance pre-release fixes YouTube's repeated warning **"No supported JavaScript runtime could be found"**, which may otherwise prevent yt-dlp from offering certain formats or extracting videos in the future.

## Fixed

- Both metadata analysis and media downloads now **automatically discover a compatible Deno executable**, including installations outside PATH. On Windows the search includes the shared setup tools in ProgramData, the per-user tools folder, the user's Deno directory and PATH. The GUI supplies yt-dlp with the correct absolute path via `--js-runtimes deno:PATH`.
- Windows setup offers **three independent, initially selected options**: yt-dlp (required), FFmpeg (recommended) and **Deno (recommended for YouTube JavaScript challenges)**. Each can be unselected.
- The Deno installer fetches the official Windows x64 ZIP and its companion SHA-256 checksum, extracts only the expected `deno.exe`, checks that Deno is at least **2.3.0**, and preserves the previous working installation if replacement fails.
- Explicit runtime settings (for example, Node 22+) and advanced yt-dlp arguments take precedence over autodetection. No runtime is installed without user selection.
- Added smoke checks for argument precedence/path handling and Windows CI checks for Deno installation scripts.

## Installation and compatibility

- The Windows installer still requires UAC approval and installs machine-wide; Deno is placed under `%ProgramData%\yt-dlp-ui\tools\deno.exe`. Normal GUI use remains non-administrator.
- If you **already have Deno**, installing a new copy is not needed. For a manual Windows install run `winget install --id DenoLand.Deno -e`, reopen the GUI, and confirm `deno --version`.
- On Linux install a current Deno using its official installation instructions or your package manager; the GUI looks for Deno on PATH.
- Official standalone yt-dlp executables bundle the needed `yt-dlp-ejs` scripts. Python/pip and Linux distribution packages may require the matching EJS dependency; see the [yt-dlp EJS guide](https://github.com/yt-dlp/yt-dlp/wiki/EJS).
- The separate Deno runtime retains its own MIT license; the GUI remains MIT, and yt-dlp/FFmpeg have their own licenses.
- No unrelated downloader functionality was added. The experimental Android version is not included.

## Tests and known limits

Windows and Linux CI build and run the smoke tests, inspect installation packaging and produce the standard five artifacts plus SHA256SUMS. The scripts are checked statically, but **interactive Windows UAC installation, Deno installation and real YouTube extraction still need manual verification** on a target PC. YouTube may change independently of the GUI. Binaries are not code-signed; this release remains pre-release.

[Report an issue](https://github.com/afrowaveltd/yt-dlp-ui/issues) with the OS, GUI version, yt-dlp and Deno versions, steps to reproduce and sanitized logs.
