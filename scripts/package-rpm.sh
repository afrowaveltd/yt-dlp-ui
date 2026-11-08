#!/usr/bin/env bash
# Build a Fedora / openSUSE / RHEL-family RPM of the self-contained desktop GUI.
# Usage: ./scripts/package-rpm.sh [--from-publish]
set -euo pipefail
ROOT="$(cd "$(dirname "$BASH_SOURCE")/.." && pwd)"
FROM_PUBLISH=0
for arg in "$@"; do
  case "$arg" in
    --from-publish) FROM_PUBLISH=1 ;;
    -h|--help) echo "Usage: $0 [--from-publish]"; exit 0 ;;
    *) echo "Unknown argument: $arg" >&2; exit 2 ;;
  esac
done
command -v rpmbuild >/dev/null || { echo "rpmbuild not installed (install rpm-build or rpm)." >&2; exit 1; }
case "$(uname -m)" in
  x86_64|amd64) ARCH=x86_64; RID=linux-x64 ;;
  aarch64|arm64) ARCH=aarch64; RID=linux-arm64 ;;
  *) echo "Unsupported architecture: $(uname -m)" >&2; exit 1 ;;
esac
VERSION="$(sed -n 's/.*<Version>\([^<]*\)<\/Version>.*/\1/p' "$ROOT/Directory.Build.props" | head -1)"
if [ "$FROM_PUBLISH" -eq 0 ]; then
  RID="$RID" bash "$ROOT/scripts/build.sh" "$ROOT/publish"
fi
test -x "$ROOT/publish/yt-dlp-gui"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT
mkdir -p "$WORK/SOURCES" "$WORK/SPECS" "$WORK/BUILD" "$WORK/BUILDROOT" "$WORK/RPMS"
STAGE="$WORK/staged"
install -d "$STAGE/opt/yt-dlp-gui" "$STAGE/usr/bin" "$STAGE/usr/share/applications" \
  "$STAGE/usr/share/icons/hicolor/256x256/apps" "$STAGE/usr/share/licenses/yt-dlp-gui"
cp -a "$ROOT/publish/." "$STAGE/opt/yt-dlp-gui/"
chmod 755 "$STAGE/opt/yt-dlp-gui/yt-dlp-gui"
ln -s /opt/yt-dlp-gui/yt-dlp-gui "$STAGE/usr/bin/yt-dlp-gui"
install -m 644 "$ROOT/packaging/yt-dlp-gui.desktop" "$STAGE/usr/share/applications/yt-dlp-gui.desktop"
install -m 644 "$ROOT/src/YtDlpUi/Assets/yt-dlp-gui.png" "$STAGE/usr/share/icons/hicolor/256x256/apps/yt-dlp-gui.png"
install -m 644 "$ROOT/LICENSE" "$STAGE/usr/share/licenses/yt-dlp-gui/LICENSE"
tar -C "$STAGE" -czf "$WORK/SOURCES/payload.tar.gz" .

cat >"$WORK/SPECS/yt-dlp-gui.spec" <<EOF
Name:           yt-dlp-gui
Version:        $VERSION
Release:        1
Summary:        Friendly graphical frontend for yt-dlp
License:        MIT
URL:            https://github.com/afrowaveltd/yt-dlp-ui
Source0:        payload.tar.gz
BuildArch:      $ARCH
Recommends:     yt-dlp
Recommends:     ffmpeg

%description
An Avalonia desktop interface for yt-dlp, with download presets, queue,
subtitles and advanced options. yt-dlp and FFmpeg are separate programs.

%prep

%build

%install
mkdir -p %{buildroot}
tar -xzf %{SOURCE0} -C %{buildroot}

%files
/opt/yt-dlp-gui
/usr/bin/yt-dlp-gui
/usr/share/applications/yt-dlp-gui.desktop
/usr/share/icons/hicolor/256x256/apps/yt-dlp-gui.png
%license /usr/share/licenses/yt-dlp-gui/LICENSE
EOF

# Pin v4 RPM packaging for wider distribution compatibility with older DNF/rpm.
rpmbuild -bb --define "_topdir $WORK" --define "_rpmformat 4" \
  --define "_build_id_links none" "$WORK/SPECS/yt-dlp-gui.spec"
mkdir -p "$ROOT/artifacts"
find "$WORK/RPMS" -name '*.rpm' -exec cp -f {} "$ROOT/artifacts/" \;
echo "RPM files:"
ls -l "$ROOT/artifacts/"*.rpm
