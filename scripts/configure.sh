#!/usr/bin/env bash
# configure.sh — Inject app identity into the godot-template project files.
#
# After forking godot-template, run this once to replace the upstream
# "Magic Match" / "MagicStudio" identity with your own. The script is
# idempotent: re-running with the same args is a no-op.
#
# What it changes:
#   - export_presets.cfg — export_path, package/unique_name, package/name,
#                          keystore/debug path
#   - project.godot      — config/name, config/description,
#                          [android] package/unique_name, package/name
#   - LICENSE            — copyright year + name
#
# What it does NOT change (manual work — see FORK_CHECKLIST.md):
#   - icon.svg (replace with your own)
#   - src/Features/Counter/ (delete if you don't need the demo)
#   - src/Features/Match3/ (delete if you're not building Match-3)
#   - assets/icons/icon.svg
#
# Usage:
#     bash scripts/configure.sh \
#         --name "My Game" \
#         --package "com.example.mygame" \
#         [--author "Your Name"] \
#         [--keystore "$HOME/.android/debug.keystore"] \
#         [--description "One-line description"]
#
# All args can also come from env vars (CI / non-interactive):
#     APP_NAME, APP_PACKAGE, APP_AUTHOR, APP_KEYSTORE, APP_DESCRIPTION
#
# Fail-fast: if --name / --package are missing, the script prints a clear
# error and exits 1. We deliberately provide NO defaults — shipping a fork
# with "Magic Match" as the app name by accident is exactly the bug class
# this script exists to prevent.

set -euo pipefail

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { printf "${GREEN}[%(%H:%M:%S)T]${NC} %s\n" -1 "$*"; }
warn() { printf "${YELLOW}[%(%H:%M:%S)T WARN]${NC} %s\n" -1 "$*" >&2; }
err()  { printf "${RED}[%(%H:%M:%S)T ERR]${NC} %s\n" -1 "$*" >&2; }
die()  { err "$*"; exit 1; }

print_usage() {
    sed -n '/^# configure.sh/,/^set -euo pipefail/p' "$0" | head -35 | sed 's/^# \{0,1\}//'
}

# ─── Parse args ───────────────────────────────────────────────────────────────
APP_NAME=""
APP_PACKAGE=""
APP_AUTHOR=""
APP_KEYSTORE=""
APP_DESCRIPTION=""

while [[ $# -gt 0 ]]; do
    case "$1" in
        --name)        APP_NAME="$2";        shift 2 ;;
        --package)     APP_PACKAGE="$2";     shift 2 ;;
        --author)      APP_AUTHOR="$2";      shift 2 ;;
        --keystore)    APP_KEYSTORE="$2";    shift 2 ;;
        --description) APP_DESCRIPTION="$2"; shift 2 ;;
        -h|--help)     print_usage; exit 0 ;;
        *) die "unknown argument: $1 (try --help)" ;;
    esac
done

# Allow env-var fallback (CI / non-interactive use)
APP_NAME="${APP_NAME:-${APP_NAME_ENV:-}}"
APP_PACKAGE="${APP_PACKAGE:-${APP_PACKAGE_ENV:-}}"
APP_AUTHOR="${APP_AUTHOR:-${APP_AUTHOR_ENV:-}}"
APP_KEYSTORE="${APP_KEYSTORE:-${APP_KEYSTORE_ENV:-}}"
APP_DESCRIPTION="${APP_DESCRIPTION:-${APP_DESCRIPTION_ENV:-}}"

# ─── Validation ──────────────────────────────────────────────────────────────
[ -n "$APP_NAME" ]    || { err "ERROR: --name is required (or set APP_NAME env var)"; exit 1; }
[ -n "$APP_PACKAGE" ] || { err "ERROR: --package is required (or set APP_PACKAGE env var)"; exit 1; }

# Sanity: package must look like a Java package
if ! echo "$APP_PACKAGE" | grep -qE '^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$'; then
    die "--package '$APP_PACKAGE' is not a valid Java package: lowercase, dot-separated (e.g. com.example.foo)"
fi

# Defaults for optional fields
APP_AUTHOR="${APP_AUTHOR:-$(git config user.name 2>/dev/null || echo "Unknown")}"
APP_DESCRIPTION="${APP_DESCRIPTION:-$APP_NAME}"
APP_KEYSTORE="${APP_KEYSTORE:-$HOME/.android/debug.keystore}"

# Convert spaces in APP_NAME to dashes for filesystem-safe filenames
APP_NAME_SLUG="$(echo "$APP_NAME" | tr '[:upper:] ' '[:lower:]-')"

log "configuring app identity:"
log "  name:        $APP_NAME"
log "  package:     $APP_PACKAGE"
log "  author:      $APP_AUTHOR"
log "  description: $APP_DESCRIPTION"
log "  keystore:    $APP_KEYSTORE"
log "  filename:    $APP_NAME_SLUG"
log ""

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

# ─── 1/3 export_presets.cfg ─────────────────────────────────────────────────
EXPORT_PRESETS="$REPO_ROOT/export_presets.cfg"
[ -f "$EXPORT_PRESETS" ] || die "export_presets.cfg not found in repo root"

log "patching $EXPORT_PRESETS"
sed -i \
    -e "s|^export_path=.*|export_path=\"builds/${APP_NAME_SLUG}-debug.apk\"|" \
    -e "s|^package/unique_name=.*|package/unique_name=\"${APP_PACKAGE}\"|" \
    -e "s|^package/name=.*|package/name=\"${APP_NAME}\"|" \
    -e "s|^keystore/debug=.*|keystore/debug=\"${APP_KEYSTORE}\"|" \
    "$EXPORT_PRESETS"

# ─── 2/3 project.godot ──────────────────────────────────────────────────────
PROJECT_GODOT="$REPO_ROOT/project.godot"
[ -f "$PROJECT_GODOT" ] || die "project.godot not found in repo root"

log "patching $PROJECT_GODOT"
sed -i \
    -e "s|^config/name=.*|config/name=\"${APP_NAME}\"|" \
    -e "s|^config/description=.*|config/description=\"${APP_DESCRIPTION}\"|" \
    -e "s|^package/unique_name=.*|package/unique_name=\"${APP_PACKAGE}\"|" \
    -e "s|^package/name=.*|package/name=\"${APP_NAME}\"|" \
    "$PROJECT_GODOT"

# ─── 3/3 LICENSE ────────────────────────────────────────────────────────────
LICENSE="$REPO_ROOT/LICENSE"
if [ -f "$LICENSE" ]; then
    YEAR="$(date +%Y)"
    log "patching $LICENSE"
    sed -i "s|^Copyright (c) [0-9]\{4\}.*|Copyright (c) ${YEAR} ${APP_AUTHOR}|" "$LICENSE"
fi

# ─── 4/4 debug keystore ────────────────────────────────────────────────────
# If the keystore file at $APP_KEYSTORE doesn't exist yet, generate the
# standard Android debug keystore so the user can immediately run
# `bash scripts/build-debug.sh` without manual keytool work.
if [ -f "$APP_KEYSTORE" ]; then
    log "keystore already exists at $APP_KEYSTORE, skipping generation"
else
    if ! command -v keytool >/dev/null; then
        warn "keytool not found in PATH — install a JDK (e.g. apt install openjdk-17-jdk-headless)"
        warn "then manually generate a debug keystore at $APP_KEYSTORE"
    else
        log "generating debug keystore at $APP_KEYSTORE"
        mkdir -p "$(dirname "$APP_KEYSTORE")"
        # Standard Android debug keystore: alias=androiddebugkey, storepass=android,
        # keypass=android. dname fields can be anything for debug builds.
        keytool -genkeypair \
                -keystore "$APP_KEYSTORE" \
                -storepass android \
                -alias androiddebugkey \
                -keypass android \
                -dname "CN=Android Debug,O=Android,C=US" \
                -keyalg RSA -keysize 2048 \
                -validity 10000 \
                || die "keytool generation failed — create keystore manually"
        log "✅ keystore ready (alias=androiddebugkey, password=android)"
    fi
fi

# ─── Verify ─────────────────────────────────────────────────────────────────
log ""
log "✅ configuration complete"
log ""
log "verify the changes with:"
log "    grep -E '^(export_path|package|config/name|config/description|keystore)' \\"
log "        export_presets.cfg project.godot"
log ""
log "remaining manual cleanup (see FORK_CHECKLIST.md):"
log "    - Replace icon.svg with your launcher icon"
log "    - Delete src/Features/Counter/ if you don't need the demo"
log "    - (Optional) Delete src/Features/Match3/ if you're not building Match-3"
log ""
log "test the result:"
log "    bash scripts/test-algorithm.sh   # 93/93 tests should still pass"
log "    bash scripts/build-debug.sh      # builds/${APP_NAME_SLUG}-debug.apk"