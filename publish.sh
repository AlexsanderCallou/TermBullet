#!/usr/bin/env bash
# Builds release assets consumed by install.ps1 and install.sh.
set -euo pipefail

PROJECT="src/TermBullet/TermBullet.csproj"
VERSION="${VERSION:-1.1.1}"
OUT_BASE="publish"
DIST_DIR="$OUT_BASE/dist"

require_command() {
  command -v "$1" >/dev/null 2>&1 || {
    echo "Missing required command: $1" >&2
    exit 1
  }
}

checksum() {
  if command -v sha256sum >/dev/null 2>&1; then
    sha256sum "$1"
  else
    shasum -a 256 "$1"
  fi
}

require_command dotnet
require_command tar
require_command zip

if ! command -v sha256sum >/dev/null 2>&1; then
  require_command shasum
fi

rm -rf "$OUT_BASE/win-x64" "$OUT_BASE/linux-x64" "$DIST_DIR"
mkdir -p "$DIST_DIR"

echo ">>> Publishing Windows x64..."
dotnet publish "$PROJECT" \
  -c Release \
  -r win-x64 \
  -o "$OUT_BASE/win-x64" \
  --self-contained true

echo ">>> Publishing Linux x64..."
dotnet publish "$PROJECT" \
  -c Release \
  -r linux-x64 \
  -o "$OUT_BASE/linux-x64" \
  --self-contained true

echo ">>> Packaging release assets..."
(
  cd "$OUT_BASE/win-x64"
  zip -q -r "../dist/termbullet_${VERSION}_windows_x64.zip" .
)

(
  cd "$OUT_BASE/linux-x64"
  tar -czf "../dist/termbullet_${VERSION}_linux_x64.tar.gz" .
)

echo ">>> Writing checksums..."
(
  cd "$DIST_DIR"
  checksum "termbullet_${VERSION}_windows_x64.zip" > "termbullet_${VERSION}_checksums.txt"
  checksum "termbullet_${VERSION}_linux_x64.tar.gz" >> "termbullet_${VERSION}_checksums.txt"
)

echo ""
echo "Done. Release assets:"
ls -1 "$DIST_DIR"
