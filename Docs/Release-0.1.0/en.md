# First public preview — v0.1.0

[Česky](cs.md) · [Index](../Documentation/en.md) · [Development and verification](../Development-and-Verification/en.md)

**Status: [v0.1.0 is publicly released](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0) as a pre-release on 8 November 2026.** The release tag is frozen; the checklist below tracks remaining manual testing and potential follow-up patches.

## Planned release content

- Avalonia/.NET 10 GUI for yt-dlp with nine tabs, URL analysis, media presets, queue, command preview and external tool status.
- Desktop light/dark theme, clipboard support and hot-switchable language selector in the main window header.
- Dictionaryless JSON locales bundled in English/Czech, with discovery from embedded/application/user `Locales` directories, plus English fallback.
- Windows x64 Inno Setup installer with independent optional yt-dlp and FFmpeg checkboxes, per-user folders and normal uninstall; portable Windows x64.
- Linux DEB, RPM and portable tar.gz builds; packaging integrates with system dependencies rather than bundling yt-dlp/FFmpeg.
- English/Czech README and the paired `Docs/<Topic>/{en,cs}.md` documentation.
- Smoke checks for localized text and JSON discovery, plus GitHub Actions builds for Windows/Linux.

## Published artifacts and CI verification

- [x] Build/test the tagged application on Windows and Linux: [successful release workflow](https://github.com/afrowaveltd/yt-dlp-ui/actions/runs/42517322864).
- [x] Build and inspect DEB, RPM, portable Linux, Windows setup and portable ZIP.
- [x] Verify bilingual documentation pairs, version 0.1.0 and local links in the smoke checks.

The release workflow uploaded all five packages and `SHA256SUMS.txt`. End-to-end manual installation remains a separate task.

## Manual acceptance checklist

- [ ] Clean Windows x64 setup and uninstall; verify Start menu/desktop shortcuts.
- [ ] Test both dependency options: both selected, one selected, neither selected, and previously installed tools.
- [ ] Test offline/download-failure handling, publisher checksum rejection and retry in Tools.
- [ ] Start portable Windows x64 without modifying installation; launch on Debian/Ubuntu.
- [ ] Install/remove DEB on a real Debian or Ubuntu desktop; verify menu, icon and dependencies.
- [ ] Install/remove RPM on Fedora/openSUSE, not only on the Ubuntu CI builder.
- [ ] Test basic download and merging/audio conversion with content you are permitted to use.
- [ ] Test progress, queue retry/cancel, paths containing spaces and non-ASCII filenames.
- [ ] Test native and custom locales (`cs`, `en`, `pt-BR`), dynamic reload and settings persistence.
- [ ] Review logs and any credential fields for accidental sensitive-data disclosure.

## Known limitations

- Localized controls are only a subset of all messages and advanced tabs.
- The Windows installer is x64 only. ARM64 can be built manually; no tested ARM64 setup or macOS package is part of this first scope.
- Automated tests validate selected behavior and package construction, **not** all websites or installers end-to-end.
- Third-party dependencies are downloaded/installed separately and have their own license obligations; the GUI is MIT.
- The public release binaries are not code-signed; SHA-256 checksums are provided.

## Publication milestones and next steps

1. **Completed:** version 0.1.0, bilingual docs, green Linux/Windows builds and smoke checks.
2. **Completed:** tag `v0.1.0` and [public pre-release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0) with all five packages and checksums.
3. **Next:** collect community reports and perform the remaining manual acceptance tests above.
4. **Later:** release any fixes with a *new* version/tag; do not rewrite the published v0.1.0 tag.

[Repository](https://github.com/afrowaveltd/yt-dlp-ui) · [Install guide](../Installation/en.md)
