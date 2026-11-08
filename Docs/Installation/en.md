# Installation and distribution

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [Getting started](../Getting-Started/en.md)

The first **[0.1.1 public pre-release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1)** is published with installable binaries and SHA-256 checksums. [GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions) artifacts are for ongoing development; the public preview binaries are **not code-signed**. For source builds use the repository root as your working directory.

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

From [v0.1.1](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1), download the Windows setup EXE and launch `yt-dlp-gui-setup-0.1.1-win-x64.exe`.

The Inno Setup wizard:
1. Installs for the current user in `%LOCALAPPDATA%\Programs\yt-dlp-gui`, without UAC elevation.
2. Creates a Start Menu entry; offers an optional desktop shortcut.
3. Shows two **independent, initially checked** options: **Install yt-dlp** and **Install FFmpeg**. You can untick either.
4. For selected, missing tools, downloads publisher-hosted executables over HTTPS and verifies against publisher-provided SHA-256 checksums. Network access is required.
5. Reports optional tool-download failures without rolling back the installed GUI. Use the **Tools** tab to try again.
6. Registers normal Windows uninstall. Personal copies of external tools in `%LOCALAPPDATA%\yt-dlp-ui\tools` remain after GUI uninstall to avoid deleting separately useful data.

The optional-tool log is in `%LOCALAPPDATA%\yt-dlp-ui\logs\dependency-install.log`. The setup is a per-user installer, **not** an enterprise MSI or a Windows ARM64 setup.

### Windows portable

The release file `yt-dlp-gui_0.1.1_win-x64_portable.zip` contains a self-contained folder with `yt-dlp-gui.exe`. Extract the entire folder and run the EXE; do not move just the EXE away from its runtime dependencies.

### Windows manual build

~~~powershell
.\scripts\build-windows.ps1
.\publish\win-x64\yt-dlp-gui.exe
~~~

With [Inno Setup 6](https://jrsoftware.org/isinfo.php) installed:

~~~powershell
.\scripts\build-windows-installer.ps1
~~~

This compiles the setup EXE to `artifacts/yt-dlp-gui-setup-0.1.1-win-x64.exe`. For an ARM64 portable build:

~~~powershell
.\scripts\build-windows.ps1 -Runtime win-arm64
~~~

## Debian / Ubuntu

~~~bash
./scripts/package-deb.sh
sudo apt install ./artifacts/yt-dlp-gui_0.1.1_amd64.deb
~~~

The package installs into `/opt/yt-dlp-gui`, creates the `/usr/bin/yt-dlp-gui` launcher and a desktop menu entry. It **recommends** `yt-dlp` and `ffmpeg`; APT may install recommended dependencies depending on its configuration. It does not run network dependency downloads in a post-install hook.

To remove the package: `sudo apt remove yt-dlp-gui`.

## Fedora / openSUSE / RPM distributions

~~~bash
./scripts/package-rpm.sh              # requires rpmbuild
sudo dnf install ./artifacts/yt-dlp-gui-0.1.1-1.x86_64.rpm
~~~

On openSUSE, use the appropriate `zypper install ./...rpm` invocation. The package uses `/opt/yt-dlp-gui` and a launcher under `/usr/bin`, with recommended yt-dlp/FFmpeg dependencies. RPM packaging is **built on Ubuntu CI**; test actual installation on the target distribution before calling it production-ready.

## Portable Linux and per-user installation

~~~bash
./scripts/package-linux-tar.sh
tar -xzf artifacts/yt-dlp-gui_0.1.1_linux-x86_64.tar.gz
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
