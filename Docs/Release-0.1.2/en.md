# Patch release v0.1.2 — Windows administrator setup

[Česky](cs.md) · [Documentation](../Documentation/en.md) · [Versioning](../Versioning/en.md)

**[Download v0.1.2](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.2)** · [Previous v0.1.1](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1)

This maintenance pre-release corrects Windows installation permissions and availability of optional tools. **No new download features** are added. The Linux packages and Android development are not changed.

## Fixed

- The Windows Inno Setup installer now explicitly requests **administrator elevation through UAC**.
- The GUI is installed machine-wide in **Program Files**, with Start menu shortcuts for all users.
- The wizard retains two independent checkboxes for yt-dlp and FFmpeg. When selected, it installs verified binaries in the shared **`%ProgramData%\yt-dlp-ui\tools`** directory instead of the elevated account's personal folder.
- Normal non-administrator GUI sessions automatically detect the shared yt-dlp/FFmpeg and can use them without elevation. User-specific settings and languages remain under **`%LOCALAPPDATA%\yt-dlp-ui`**.
- Existing machine-wide tools are checked by launching their version commands. Failed yt-dlp replacement attempts restore the prior executable; verified FFmpeg/FFprobe archives are staged and tested before replacing the previous folder.
- The setup log is now at **`%ProgramData%\yt-dlp-ui\logs\dependency-install.log`**.
- CI checks that Inno Setup requests administrator privileges, validates PowerShell installer syntax and tests Linux/Windows builds.

## Upgrading from 0.1.0 or 0.1.1

Previous Windows installers installed a **per-user** copy in `%LOCALAPPDATA%\Programs\yt-dlp-gui`. This patch changes the installation scope to **all users**. Uninstall the previous per-user GUI via Windows Settings before running the new setup, to prevent duplicate shortcuts. The user's personal settings and existing per-user tools are preserved.

The new setup may ask for administrator credentials via Windows UAC. This is **only for installation**; normal use and downloads do not require administrator privileges. The standard Tools tab still supports per-user tool installation.

Machine-wide tools are retained after GUI uninstall to avoid removing dependencies other Windows accounts may use; an administrator can remove them manually if no longer needed.

## Validation and limitations

The published workflow builds and tests on Windows and Linux, packages Windows setup/portable and Linux DEB/RPM/tar.gz, and attaches `SHA256SUMS.txt`. Automated checks verify installer configuration and PowerShell syntax but **cannot simulate a real UAC consent dialog or run a full elevated GUI installation on a user's PC**. That final scenario still needs a real Windows test.

The software is a **pre-release** with MIT-licensed GUI code; FFmpeg and yt-dlp are separate programs with their own licenses. Unsigned binaries may show system warnings. The Android prototype is not bundled in v0.1.2.

[Report an issue](https://github.com/afrowaveltd/yt-dlp-ui/issues) with Windows version, install log (without sensitive details), and reproduction steps.
