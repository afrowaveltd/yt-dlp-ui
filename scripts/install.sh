#!/usr/bin/env bash
# Installs yt-dlp GUI into the user's home directory (no root needed).
# Usage: ./scripts/install.sh [--uninstall] [--rebuild]
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PREFIX="${PREFIX:-$HOME/.local}"
APP_DIR="$PREFIX/opt/yt-dlp-gui"
BIN_DIR="$PREFIX/bin"
APPS_DIR="$PREFIX/share/applications"
ICON_DIR="$PREFIX/share/icons/hicolor/256x256/apps"
DESKTOP_FILE="$APPS_DIR/yt-dlp-gui.desktop"
LAUNCHER="$BIN_DIR/yt-dlp-gui"

UNINSTALL=0
REBUILD=0
for arg in "$@"; do
  case "$arg" in
    --uninstall) UNINSTALL=1 ;;
    --rebuild)   REBUILD=1 ;;
    -h|--help)
      sed -n '2,3p' "${BASH_SOURCE[0]}"
      exit 0
      ;;
    *)
      echo "Neznamy parametr: $arg" >&2
      exit 1
      ;;
  esac
done

if [ "$UNINSTALL" -eq 1 ]; then
  echo "==> Odinstalovavam yt-dlp GUI"
  rm -f "$LAUNCHER" "$DESKTOP_FILE" "$ICON_DIR/yt-dlp-gui.png"
  rm -rf "$APP_DIR"
  command -v update-desktop-database >/dev/null 2>&1 && update-desktop-database "$APPS_DIR" 2>/dev/null || true
  echo "==> Hotovo. Uzivatelska nastaveni zustala v ~/.config/yt-dlp-ui"
  exit 0
fi

if [ ! -x "$ROOT/publish/yt-dlp-gui" ] || [ "$REBUILD" -eq 1 ]; then
  "$ROOT/scripts/build.sh"
fi

echo "==> Instaluji do $APP_DIR"
rm -rf "$APP_DIR"
install -d "$APP_DIR"
cp -a "$ROOT/publish/." "$APP_DIR/"
chmod +x "$APP_DIR/yt-dlp-gui" 2>/dev/null || true

echo "==> Vytvarim prikaz $LAUNCHER"
install -d "$BIN_DIR"
ln -sf "$APP_DIR/yt-dlp-gui" "$LAUNCHER"

echo "==> Instaluji ikonu a polozku nabidky aplikaci"
install -d "$ICON_DIR" "$APPS_DIR"
cp -f "$ROOT/src/YtDlpUi/Assets/yt-dlp-gui.png" "$ICON_DIR/yt-dlp-gui.png"
cp -f "$ROOT/packaging/yt-dlp-gui.desktop" "$DESKTOP_FILE"
# Some mounts (network shares, FUSE) refuse chmod; the files are still usable without it.
chmod 644 "$ICON_DIR/yt-dlp-gui.png" "$DESKTOP_FILE" 2>/dev/null || true

command -v update-desktop-database >/dev/null 2>&1 && update-desktop-database "$APPS_DIR" 2>/dev/null || true
command -v gtk-update-icon-cache >/dev/null 2>&1 && gtk-update-icon-cache -q "$PREFIX/share/icons/hicolor" 2>/dev/null || true

echo
echo "==> Hotovo."
echo "    Spusteni z terminalu: yt-dlp-gui"
echo "    Nebo v nabidce aplikaci pod nazvem 'yt-dlp GUI'."

case ":$PATH:" in
  *":$BIN_DIR:"*) ;;
  *)
    echo
    echo "    Pozor: $BIN_DIR neni ve vase PATH. Pridejte do ~/.profile:"
    echo "        export PATH=\"\$HOME/.local/bin:\$PATH\""
    ;;
esac
