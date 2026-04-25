#!/usr/bin/env sh
set -eu

REPOSITORY="AlexsanderCallou/TermBullet"
VERSION="${VERSION:-latest}"
INSTALL_DIR="${INSTALL_DIR:-$HOME/.local/bin}"
ASSET_PATTERN="linux_x64.tar.gz"

log() {
    printf '==> %s\n' "$1"
}

fail() {
    printf 'Error: %s\n' "$1" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || fail "Missing required command: $1"
}

download() {
    curl -fsSL "$1"
}

case "$(uname -s)" in
    Linux)
        ;;
    *)
        fail "This installer currently supports Linux only."
        ;;
esac

case "$(uname -m)" in
    x86_64|amd64)
        ;;
    *)
        fail "This installer currently supports Linux x64 only."
        ;;
esac

require_command curl
require_command tar

if command -v sha256sum >/dev/null 2>&1; then
    HASH_COMMAND="sha256sum"
elif command -v shasum >/dev/null 2>&1; then
    HASH_COMMAND="shasum -a 256"
else
    fail "Missing required command: sha256sum or shasum"
fi

if [ "$VERSION" = "latest" ]; then
    release_url="https://api.github.com/repos/$REPOSITORY/releases/latest"
else
    release_url="https://api.github.com/repos/$REPOSITORY/releases/tags/$VERSION"
fi

log "Resolving TermBullet release ($VERSION)"
release_json="$(download "$release_url")"

archive_url="$(printf '%s\n' "$release_json" | tr -d '\r' | grep '"browser_download_url":' | sed -n "s/.*\"browser_download_url\": \"\\(.*$ASSET_PATTERN\\)\".*/\\1/p" | head -n 1)"
checksum_url="$(printf '%s\n' "$release_json" | tr -d '\r' | grep '"browser_download_url":' | sed -n 's/.*"browser_download_url": "\(.*checksums\.txt\)".*/\1/p' | head -n 1)"

[ -n "$archive_url" ] || fail "Could not find a Linux x64 asset in the release."
[ -n "$checksum_url" ] || fail "Could not find checksum asset in the release."

archive_name="${archive_url##*/}"
checksum_name="${checksum_url##*/}"

temp_root="$(mktemp -d "${TMPDIR:-/tmp}/termbullet-install.XXXXXX")"
extract_dir="$temp_root/extract"
archive_path="$temp_root/$archive_name"
checksum_path="$temp_root/$checksum_name"

cleanup() {
    rm -rf "$temp_root"
}
trap cleanup EXIT INT TERM

mkdir -p "$extract_dir"

log "Downloading $archive_name"
download "$archive_url" > "$archive_path"

log "Downloading $checksum_name"
download "$checksum_url" > "$checksum_path"

log "Verifying SHA256 checksum"
expected_hash="$(grep "  $archive_name\$" "$checksum_path" | awk '{print $1}' | head -n 1)"
[ -n "$expected_hash" ] || fail "Could not find checksum entry for $archive_name."

actual_hash="$($HASH_COMMAND "$archive_path" | awk '{print $1}')"
if [ "$(printf '%s' "$actual_hash" | tr '[:upper:]' '[:lower:]')" != "$(printf '%s' "$expected_hash" | tr '[:upper:]' '[:lower:]')" ]; then
    fail "Checksum mismatch. Expected $expected_hash but got $actual_hash."
fi

log "Extracting archive"
tar -xzf "$archive_path" -C "$extract_dir"

executable="$(find "$extract_dir" -type f -name termbullet | head -n 1)"
[ -n "$executable" ] || fail "Archive does not contain termbullet."

log "Installing to $INSTALL_DIR"
mkdir -p "$INSTALL_DIR"
cp "$executable" "$INSTALL_DIR/termbullet"
chmod +x "$INSTALL_DIR/termbullet"

log "Installation complete"
printf '\n'
printf 'Run: termbullet --help\n'

case ":${PATH:-}:" in
    *":$INSTALL_DIR:"*)
        ;;
    *)
        printf '\n'
        printf 'Add this directory to your PATH if termbullet is not found:\n'
        printf '  %s\n' "$INSTALL_DIR"
        printf '\n'
        printf 'For many shells, add this line to your profile:\n'
        printf '  export PATH="$PATH:%s"\n' "$INSTALL_DIR"
        ;;
esac
