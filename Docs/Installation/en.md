# Installation and distribution

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [Getting started](../Getting-Started/en.md)

The **[0.1.5 maintenance pre-release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.5)** is published with installable binaries and SHA-256 checksums. [GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions) artifacts are for ongoing development; the public preview binaries are **not code-signed**. For source builds use the repository root as your working directory.

## Platform matrix

| Platform | Format | Current build/verification |
|---|---|---|
| Windows 10/11 x64 | Setup.exe (Inno Setup) | Compiled and uploaded by Windows CI; interactive download flow requires a manual test |
| Windows 10/11 x64 | Portable folder | Published and uploaded by Windows CI |
| Windows ARM64 | Portable folder | `build-windows.ps1 -Runtime win-arm64` exists; not built by current matrix |
| Debian / Ubuntu x64 | DEB | Built and structurally inspected on Linux CI |
| Fedora / openSUSE-like x64 | RPM | Built and structurally inspected on Linux CI; target distributions not smoke-installed |
| Other Linux x64 | Portable tar.gz | Built and contents inspected on Linux CI |
| Linux arm64 | DEB/RPM/tar scripts | Script-level support exists; current CI tests x64 only |
| macOS | Source/manual publish | No documented or CI-tested installer; no macOS release artifact |

### External tool responsibilities

- `yt-dlp` does the downloading. A runnable copy is needed for the GUI to download.
- `ffmpeg` and usually `ffprobe` are needed for merging streams and many audio/video conversions.
- The GUI does **not** bundle these executables into its DEB/RPM/Windows Setup or portable archives. Third-party licenses remain separate from the GUI's MIT license.
- Tools can also be supplied by system installation or a custom path on the **Tools** tab.

## Windows x64: guided installer

From [v0.1.5](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.5), download the Windows setup EXE and launch `yt-dlp-gui-setup-0.1.5-win-x64.exe`.

The Inno Setup wizard:
1. **Requests UAC administrator approval** and installs for all Windows users into `%ProgramFiles%\yt-dlp-gui`.
2. Creates Start menu shortcuts, offers a desktop shortcut, and three **independent initially checked** options: install yt-dlp / install FFmpeg / install Deno (YouTube JavaScript runtime).
3. Downloads selected yt-dlp, FFmpeg and Deno over HTTPS, verifies their publisher SHA-256 and installs them under `%ProgramData%\yt-dlp-ui\tools`. The installer verifies the executables can start, including Deno 2.3.0 or newer.
4. Leaves individual app preferences under `%LOCALAPPDATA%\yt-dlp-ui`. The desktop GUI does **not** need administrator rights to run or download media.
5. Reports optional download errors without uninstalling the GUI. Check `%ProgramData%\yt-dlp-ui\logs\dependency-install.log`; rerun Setup with UAC for shared repair. The Tools tab can install a separate per-user tool copy.
6. Registers normal Windows uninstall. Machine-wide tools are retained after uninstall, to avoid breaking other users and applications.

**Migration from v0.1.0/v0.1.1:** the old setup installed into `%LOCALAPPDATA%\Programs\yt-dlp-gui`. Uninstall that old *per-user* copy through Windows Settings before installing this machine-wide version to avoid duplicate application shortcuts. This does **not** remove your personal settings or previously installed external tools. If the old copy was under another Windows user, that user should uninstall it.

The setup requires administrative credentials via UAC; Deno is optional and only installed when its checkbox is selected. Deno is recommended by yt-dlp and not bundled inside our own GUI. do not routinely launch the installed GUI elevated. This is an Inno Setup installer, not an MSI or an ARM64 installer.

### Windows portable

The release file `yt-dlp-gui_0.1.5_win-x64_portable.zip` contains a self-contained folder with `yt-dlp-gui.exe`. Extract the entire folder and run the EXE; do not move just the EXE away from its runtime dependencies.

### Windows manual build

~~~powershell
.\scripts\build-windows.ps1
.\publish\win-x64\yt-dlp-gui.exe
~~~

With [Inno Setup 6](https://jrsoftware.org/isinfo.php) installed:

~~~powershell
.\scripts\build-windows-installer.ps1
~~~

This compiles the setup EXE to `artifacts/yt-dlp-gui-setup-0.1.5-win-x64.exe`. For an ARM64 portable build:

~~~powershell
.\scripts\build-windows.ps1 -Runtime win-arm64
~~~

## Debian / Ubuntu

~~~bash
./scripts/package-deb.sh
sudo apt install ./artifacts/yt-dlp-gui_0.1.5_amd64.deb
~~~

The package installs into `/opt/yt-dlp-gui`, creates the `/usr/bin/yt-dlp-gui` launcher and a desktop menu entry. It **recommends** `yt-dlp` and `ffmpeg`; APT may install recommended dependencies depending on its configuration. It does not run network dependency downloads in a post-install hook. **Starting with v0.1.5**, Deno is optional: open Tools → Deno → Install Deno for Windows / Linux to download a SHA-256-verified user-private copy into `~/.deno/bin` (or `$DENO_INSTALL/bin`). No `sudo`, package-manager modification or PATH change is needed.

To remove the package: `sudo apt remove yt-dlp-gui`.

## Fedora / openSUSE / RPM distributions

~~~bash
./scripts/package-rpm.sh              # requires rpmbuild
sudo dnf install ./artifacts/yt-dlp-gui-0.1.5-1.x86_64.rpm
~~~

On openSUSE, use the appropriate `zypper install ./...rpm` invocation. The package uses `/opt/yt-dlp-gui` and a launcher under `/usr/bin`, with recommended yt-dlp/FFmpeg dependencies. RPM packaging is **built on Ubuntu CI**; test actual installation on the target distribution before calling it production-ready.

## Portable Linux and per-user installation

~~~bash
./scripts/package-linux-tar.sh
tar -xzf artifacts/yt-dlp-gui_0.1.5_linux-x86_64.tar.gz
./yt-dlp-gui/yt-dlp-gui
~~~

The archive is self-contained but assumes the target OS has required desktop/native libraries. It does not modify system packages.

To install from source into `~/.local/`, use:

~~~bash
./scripts/install.sh
yt-dlp-gui
./scripts/install.sh --uninstall
~~~

This installs to `~/.local/opt/yt-dlp-gui` and creates user-local launcher, icon and menu entry. Ensure `~/.local/bin` is on PATH. Do not confuse this path with the system-wide DEB/RPM installation.

## Prerequisites for building

- .NET **10 SDK** as selected by `global.json`.
- Linux packaging: `dpkg-deb` for DEB, `rpmbuild` for RPM, `tar` for the archive.
- Windows setup: Inno Setup 6 and PowerShell.
- GUI run-time display/native libraries may vary with the target desktop environment.

Packaging scripts derive the version from `Directory.Build.props`; do not hard-code another version into documentation or distribute mislabeled packages.

[Build verification](../Development-and-Verification/en.md) · [Troubleshooting](../Troubleshooting/en.md)
