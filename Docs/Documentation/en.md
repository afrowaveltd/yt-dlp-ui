# Documentation — yt-dlp GUI

**Language:** English · [Česky](cs.md) · [Repository README](../../README.md)

This index covers the published **0.1.5 community maintenance pre-release** and separates implemented functionality, build verification and manual validation still needed. Each topic lives at `Docs/<Topic>/en.md` with a Czech sibling `cs.md`.

## User guides

1. [Getting started](../Getting-Started/en.md) — first launch, download a video, basic controls.
2. [Installation and distribution](../Installation/en.md) — Windows setup/portable, DEB, RPM, portable Linux, dependency choices.
3. [Features](../Features/en.md) — all nine tabs, queue, formats, command preview and keyboard shortcuts.
4. [Localization](../Localization/en.md) — dictionaryless JSON and hot-added language files.
5. [Configuration and storage](../Configuration/en.md) — settings, directories and privacy.

## Maintenance and development

6. [Troubleshooting](../Troubleshooting/en.md) — missing tools, installers, network, Linux graphics and log inspection.
7. [Development and verification](../Development-and-Verification/en.md) — .NET 10, project structure, smoke checks and CI artifacts.
8. [Versioning and release policy](../Versioning/en.md) — PATCH fixes, MINOR previews, stable v1+, Git tags and release rules.
9. [Android prototype](../Android/en.md) — separate touch interface, shared core, direct HTTPS downloads; not part of v0.1.0.
10. [Release 0.1.5 fix](../Release-0.1.5/en.md) — playlist titles and durations in the analysis preview.
11. [Release 0.1.4 fixes](../Release-0.1.4/en.md) — Deno status indicators, Tools installer and Linux discovery.
12. [Release 0.1.3 fixes](../Release-0.1.3/en.md) — YouTube JavaScript runtime and optional Deno setup.
13. [Release 0.1.2 fixes](../Release-0.1.2/en.md) — Windows UAC setup and shared tool fixes.
14. [Release 0.1.1 fixes](../Release-0.1.1/en.md) — historical desktop patch and tests.
15. [Release 0.1.0 status](../Release-0.1.0/en.md) — **[published preview](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)**, known gaps and community-test checklist.

## Localized README

- [English README mirror](../README/en.md) — mirrors the root README with adapted relative links.
- [Czech README](../README/cs.md) — localized counterpart of the repository README.
- [Root README](../../README.md) — canonical GitHub entry point (English).

## Rules for maintaining documentation

- **English first:** a new topic gets `Docs/Topic/en.md` and `Docs/Topic/cs.md`.
- Describe **the source as implemented**, not intentions as if they were shipped.
- When features, config paths or package formats change, update the README, both languages and the release status.
- Test local links and code examples. Do not claim that CI compilation substitutes for real Windows/Linux installation tests.
- Keep the existing screenshots in `screenshots/`; use relative paths from each document.

[Project repository](https://github.com/afrowaveltd/yt-dlp-ui) · [Current CI builds](https://github.com/afrowaveltd/yt-dlp-ui/actions)
