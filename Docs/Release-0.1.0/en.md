# First release preparation — 0.1.0

[Česky](cs.md) · [Index](../Documentation/en.md) · [Development and verification](../Development-and-Verification/en.md)

**Status: preparing 0.1.0. No GitHub Release or immutable tag is claimed to exist.** This document records the intended first preview scope and what must still be verified.

## Planned release content

- Avalonia/.NET 10 GUI for yt-dlp with nine tabs, URL analysis, media presets, queue, command preview and external tool status.
- Desktop light/dark theme, clipboard support and hot-switchable language selector in the main window header.
- Dictionaryless JSON locales bundled in English/Czech, with discovery from embedded/application/user `Locales` directories, plus English fallback.
- Windows x64 Inno Setup installer with independent optional yt-dlp and FFmpeg checkboxes, per-user folders and normal uninstall; portable Windows x64.
- Linux DEB, RPM and portable tar.gz builds; packaging integrates with system dependencies rather than bundling yt-dlp/FFmpeg.
- English/Czech README and the paired `Docs/<Topic>/{en,cs}.md` documentation.
- Smoke checks for localized text and JSON discovery, plus GitHub Actions builds for Windows/Linux.

## Verified by CI (when the current candidate succeeds)

- [ ] Verify final commit SHA and green Linux/Windows matrix on [GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions).
- [ ] Verify DEB metadata/contents, RPM metadata/contents, Linux archive and Windows setup/portable artifacts.
- [ ] Verify documentation pairs, version consistency and local links.

CI should mark these steps complete only once the final `0.1.0` version changes have built successfully.

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
- No code-signing certificate or signed public release is currently claimed.

## Publication sequence (not yet performed)

1. Fix any failures from the acceptance checklist and update both language versions of the docs.
2. Confirm `Directory.Build.props`, Inno Setup fallback version, package filenames and published docs all say **0.1.0**.
3. Confirm final CI is green for the exact commit intended for release.
4. Create an immutable `v0.1.0` Git tag **only after acceptance**.
5. Publish a GitHub Release using the tag and attach the validated packages and appropriate checksums.
6. Announce only artifacts actually published, and retain release notes for later fixes.

[Repository](https://github.com/afrowaveltd/yt-dlp-ui) · [Install guide](../Installation/en.md)
