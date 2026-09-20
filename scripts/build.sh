#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.0f1-arm64/Unity.app/Contents/MacOS/Unity}"
mode="${1:-Web}"
case "$mode" in Web|Desktop|Test) ;; *) echo 'Use Web, Desktop or Test'; exit 2;; esac
"$editor" -batchmode -nographics -projectPath "$root/Reentry" -executeMethod "BuildReentry.$mode" -quit -logFile /tmp/ember-build.log
if [[ "$mode" == Web ]]; then node "$root/scripts/check-web.mjs" "$root/Reentry/Build/WebGL"; fi
