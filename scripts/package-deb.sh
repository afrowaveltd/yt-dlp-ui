#!/usr/bin/env bash
# Build a Debian/Ubuntu package containing the self-contained Linux desktop application.
# Usage:
#   ./scripts/package-deb.sh
#   ./scripts/package-deb.sh --from-publish   # package an existing ./publish (CI/testing)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
FROM_PUBLISH=0
for arg in "$@"; do
  case "$arg" in
    --from-publish) FROM_PUBLISH=1 ;;
    -h|--help)
      echo "Usage: $0 [--from-publish]"
      exit 0 ;;
    *) echo "Unknown argument: $arg" >&2; exit 2 ;;
  esac
done

if ! command -v dpkg-deb >/dev/null 2>&1; then
  echo "dpkg-deb not found (install dpkg-dev)." >&2
  exit 1
fi

ARCH="$(dpkg --print-architecture)"
case "$ARCH" in
  amd64) RID=linux-x64 ;;
  arm64) RID=linux-arm64 ;;
  *) echo "Unsupported Debian architecture: $ARCH" >&2; exit 1 ;;
esac

VERSION="$(sed -n 's/.*<Version>\([^<]*\)<\/Version>.*/\1/p' "$ROOT/Directory.Build.props" | head -n 1)"
if [ -z "$VERSION" ]; then
  echo "Unable to read package version from Directory.Build.props" >&2
  exit 1
fi

if [ "$FROM_PUBLISH" -eq 0 ]; then
  RID="$RID" "$ROOT/scripts/build.sh" "$ROOT/publish"
fi

if [ ! -x "$ROOT/publish/yt-dlp-gui" ]; then
  echo "Missing executable: publish/yt-dlp-gui" >&2
  exit 1
fi

STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT
APP="$STAGE/opt/yt-dlp-gui"
install -d "$STAGE/DEBIAN" "$APP" "$STAGE/usr/bin" \
  "$STAGE/usr/share/applications" "$STAGE/usr/share/icons/hicolor/256x256/apps" \
  "$STAGE/usr/share/doc/yt-dlp-gui"
cp -a "$ROOT/publish/." "$APP/"
chmod 755 "$APP/yt-dlp-gui"
ln -s /opt/yt-dlp-gui/yt-dlp-gui "$STAGE/usr/bin/yt-dlp-gui"
install -m 644 "$ROOT/packaging/yt-dlp-gui.desktop" "$STAGE/usr/share/applications/yt-dlp-gui.desktop"
install -m 644 "$ROOT/src/YtDlpUi/Assets/yt-dlp-gui.png" \
  "$STAGE/usr/share/icons/hicolor/256x256/apps/yt-dlp-gui.png"
install -m 644 "$ROOT/LICENSE" "$STAGE/usr/share/doc/yt-dlp-gui/copyright"

SIZE="$(du -sk "$STAGE/opt/yt-dlp-gui" | cut -f1)"
cat >"$STAGE/DEBIAN/control" <<EOF
Package: yt-dlp-gui
Version: $VERSION
Section: video
Priority: optional
Architecture: $ARCH
Maintainer: Afrowave contributors <noreply@github.com>
Installed-Size: $SIZE
Depends: libc6, libfontconfig1, libx11-6
Recommends: yt-dlp, ffmpeg
Homepage: https://github.com/afrowaveltd/yt-dlp-ui
Description: Friendly desktop GUI for yt-dlp
 A cross-platform Avalonia application for downloading video and audio.
 Provides a download queue, presets, subtitles and advanced yt-dlp options.
 yt-dlp and ffmpeg are external tools, not bundled in this package.
EOF

mkdir -p "$ROOT/artifacts"
OUTPUT="$ROOT/artifacts/yt-dlp-gui_${VERSION}_${ARCH}.deb"
dpkg-deb --root-owner-group --build "$STAGE" "$OUTPUT"
echo "Package created: $OUTPUT"
echo "Install: sudo apt install ./artifacts/$(basename "$OUTPUT")"
