# Release 0.1.4 — Deno controls and Linux runtime discovery

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.4)

This is a **desktop-only maintenance pre-release**, finishing the Deno integration started in v0.1.3.

## Delivered

- The top-right status area now has **three indicators**, for **yt-dlp, FFmpeg and Deno**. Deno's indicator reflects whether a compatible runtime was found, with its path in the tooltip.
- **Tools → Deno** displays availability, executable path and version (requires Deno 2.3+), plus an explicit **Install Deno for Windows / Linux** action on supported systems and **Check again** to refresh detection.
- Linux searches for the runtime in `$DENO_INSTALL/bin`, `~/.deno/bin`, `~/.local/bin`, `~/bin` and PATH, including desktop launches that do not use the interactive shell PATH.
- The managed Deno installer downloads the official archive and checksum, validates the executable, and installs a per-user copy **without sudo/UAC**. It does not modify PATH or replace a working executable after failed validation.
- Windows Setup keeps its separate checked yt-dlp / FFmpeg / Deno installation options and requires UAC for shared tools.
- Smoke checks now guard the Deno UI layout and Linux installation path behavior.

Deno remains optional for the GUI, but is recommended for YouTube compatibility with recent yt-dlp. The GUI also preserves manually configured JavaScript runtime options.

## Packages

Windows x64 setup and portable ZIP; Debian/Ubuntu x64 DEB; x64 RPM; Linux x64 portable tar.gz; and SHA-256 checksums. All packages are built by GitHub Actions from the **same release commit**. See [Installation](../Installation/en.md).

## Validation and remaining checks

Windows installer and Deno display were manually confirmed by a project tester. CI verifies compilation, smoke checks, package contents and release checksums; it is **not** a substitute for real downloads and installation on every distribution. Please test Debian/Ubuntu, an RPM distribution, interrupted Deno installation, older installed Deno, and actual YouTube media that you may download.

**Known limits:** unsigned community pre-release, partial UI localization, external yt-dlp/site changes and no bundled Android application. Android stays an experimental separate project.

[Versioning](../Versioning/en.md) · [Troubleshooting](../Troubleshooting/en.md) · [Report an issue](https://github.com/afrowaveltd/yt-dlp-ui/issues)
