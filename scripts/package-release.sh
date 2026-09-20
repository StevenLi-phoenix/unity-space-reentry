#!/usr/bin/env bash
set -euo pipefail
platform="${1:?platform required}"
source_root="$(cd "${2:?build directory required}" && pwd)"
mkdir -p "${3:?archive directory required}"
archive_root="$(cd "$3" && pwd)"
case "$platform" in
  macos)
    test -s "$source_root/Ember.app/Contents/Info.plist"
    test -s "$source_root/Ember.app/Contents/MacOS/EMBER - Return to Earth"
    chmod +x "$source_root/Ember.app/Contents/MacOS/EMBER - Return to Earth"
    COPYFILE_DISABLE=1 tar -czf "$archive_root/Ember-macOS.tar.gz" -C "$source_root" 'Ember.app'
    ;;
  windows)
    test -s "$source_root/Ember.exe"
    test -s "$source_root/UnityPlayer.dll"
    test -d "$source_root/Ember_Data"
    (cd "$source_root" && zip -qr "$archive_root/Ember-Windows-x64.zip" .)
    ;;
  linux)
    test -s "$source_root/Ember.x86_64"
    test -s "$source_root/UnityPlayer.so"
    test -d "$source_root/Ember_Data"
    chmod +x "$source_root/Ember.x86_64"
    tar -czf "$archive_root/Ember-Linux-x64.tar.gz" -C "$source_root" .
    ;;
  webgl)
    node "$(dirname "$0")/check-web.mjs" "$source_root"
    (cd "$source_root" && zip -qr "$archive_root/Ember-WebGL.zip" .)
    ;;
  *) echo "Unsupported platform: $platform" >&2; exit 1 ;;
esac
echo "RELEASE_PACKAGE_OK: $platform"
