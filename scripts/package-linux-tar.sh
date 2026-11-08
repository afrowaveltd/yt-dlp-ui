#!/usr/bin/env bash
# Portable, self-contained Linux archive for distributions without .deb/.rpm.
# Usage: ./scripts/package-linux-tar.sh [--from-publish]
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
STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT
mkdir -p "$STAGE/yt-dlp-gui"
cp -a "$ROOT/publish/." "$STAGE/yt-dlp-gui/"
cp -f "$ROOT/LICENSE" "$STAGE/yt-dlp-gui/LICENSE"
mkdir -p "$ROOT/artifacts"
OUTPUT="$ROOT/artifacts/yt-dlp-gui_$VERSION_linux-$ARCH.tar.gz"
tar -C "$STAGE" -czf "$OUTPUT" yt-dlp-gui
echo "Portable archive: $OUTPUT"
echo "Unpack it, then run ./yt-dlp-gui/yt-dlp-gui."
