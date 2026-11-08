#!/usr/bin/env bash
# Builds a self-contained (no .NET needed) release of yt-dlp GUI.
#
# Usage:
#   ./scripts/build.sh                 # app folder in ./publish  (default, most robust)
#   ./scripts/build.sh --single-file   # one portable executable in ./publish
#   ./scripts/build.sh /custom/dir     # custom output folder
#
# The folder build is the default on purpose: a single-file build has to unpack its native
# libraries into a cache directory at startup, which fails on systems with a read-only or
# missing home directory. Use --single-file only when you want one file to carry around.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SINGLE_FILE=0
OUT=""

for arg in "$@"; do
  case "$arg" in
    --single-file) SINGLE_FILE=1 ;;
    -h|--help) sed -n '2,11p' "${BASH_SOURCE[0]}"; exit 0 ;;
    *) OUT="$arg" ;;
  esac
done

OUT="${OUT:-$ROOT/publish}"

# --- runtime identifier for this machine --------------------------------------
detect_rid() {
  local os arch
  case "$(uname -s)" in
    Linux*)  os=linux ;;
    Darwin*) os=osx ;;
    MINGW*|MSYS*|CYGWIN*) os=win ;;
    *) echo "Nepodporovany system: $(uname -s)" >&2; exit 1 ;;
  esac

  case "$(uname -m)" in
    x86_64|amd64) arch=x64 ;;
    aarch64|arm64) arch=arm64 ;;
    armv7l) arch=arm ;;
    *) echo "Nepodporovana architektura: $(uname -m)" >&2; exit 1 ;;
  esac

  echo "$os-$arch"
}

RID="${RID:-$(detect_rid)}"

PUBLISH_ARGS=(
  --configuration Release
  --runtime "$RID"
  --self-contained true
  -p:PublishTrimmed=false
  -p:DebugType=none
  --output "$OUT"
)

if [ "$SINGLE_FILE" -eq 1 ]; then
  PUBLISH_ARGS+=(
    -p:PublishSingleFile=true
    -p:IncludeNativeLibrariesForSelfExtract=true
    -p:EnableCompressionInSingleFile=true
  )
  echo "==> Sestavuji yt-dlp GUI pro $RID (jeden soubor)"
else
  PUBLISH_ARGS+=(-p:PublishSingleFile=false)
  echo "==> Sestavuji yt-dlp GUI pro $RID (slozka aplikace)"
fi

cd "$ROOT"
rm -rf "$OUT"
dotnet publish src/YtDlpUi/YtDlpUi.csproj "${PUBLISH_ARGS[@]}"

rm -f "$OUT"/*.pdb

echo
if [ "$SINGLE_FILE" -eq 1 ]; then
  echo "==> Hotovo: $OUT/yt-dlp-gui"
else
  echo "==> Hotovo: $OUT/yt-dlp-gui  (spoustejte z teto slozky)"
fi
echo "    Instalace do systemu: $ROOT/scripts/install.sh"
