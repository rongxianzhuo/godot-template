#!/usr/bin/env bash
# build-debug.sh — Export a debug APK of the Godot project.
#
# Output: builds/app-debug.apk (~110 MB with Mono runtime).
#
# Prereqs (run in order):
#   1. bash scripts/install-toolchain.sh         (provides godot + Android SDK/NDK)
#   2. bash scripts/install-android-templates.sh (provides android/build/ + export_templates)
#   3. bash scripts/sync-submodule.sh            (ensures framework source present)
#
# Usage:
#     bash scripts/build-debug.sh
#     # or with custom output path:
#     OUTPUT=builds/MagicMatch-debug.apk bash scripts/build-debug.sh

set -euo pipefail

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { printf "${GREEN}[%(%H:%M:%S)T]${NC} %s\n" -1 "$*"; }
warn() { printf "${YELLOW}[%(%H:%M:%S)T WARN]${NC} %s\n" -1 "$*" >&2; }
err()  { printf "${RED}[%(%H:%M:%S)T ERR]${NC} %s\n" -1 "$*" >&2; }
die()  { err "$*"; exit 1; }

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT="${OUTPUT:-$REPO_ROOT/builds/app-debug.apk}"

cd "$REPO_ROOT"

# ─── Preflight ────────────────────────────────────────────────────────────────
GODOT_BIN="${GODOT_BIN:-$(command -v godot || true)}"
if [ -z "$GODOT_BIN" ] || [ ! -x "$GODOT_BIN" ]; then
    die "godot not found in PATH. Run scripts/install-toolchain.sh and source /opt/godot-template-env.sh"
fi
log "using godot: $($GODOT_BIN --version 2>&1 | head -1)"

if [ ! -d "$REPO_ROOT/android/build" ]; then
    die "android/build/ not found. Run scripts/install-android-templates.sh first."
fi

# Required for non-Gradle C# + Android export path. README §Pitfalls #6.
if ! grep -q 'TargetFramework>net9.0' "$REPO_ROOT/GodotTemplate.csproj"; then
    die "GodotTemplate.csproj doesn't target net9.0 — non-Gradle export will fail (README §Pitfalls #6)."
fi

# Export templates presence check (Godot silently fails without them).
# After install-android-templates.sh extracts with `unzip -j` the templates
# are flattened, so android_debug.apk lives directly in the version dir.
TPL_DIR="$HOME/.local/share/godot/export_templates/4.7.2.stable.mono"
if [ ! -f "$TPL_DIR/android_debug.apk" ]; then
    die "export templates missing: $TPL_DIR/android_debug.apk not found.
         Did scripts/install-android-templates.sh complete successfully?"
fi

# ─── Run export ───────────────────────────────────────────────────────────────
mkdir -p "$(dirname "$OUTPUT")"

log "exporting debug APK to $OUTPUT"
log "(first build = 2-5 min; subsequent = 30-60 sec)"

# --export-debug expects: --export-debug "<preset_name>" "<output_path>"
# Preset name comes from export_presets.cfg [preset.0] name="Android"
"$GODOT_BIN" \
    --headless \
    --path "$REPO_ROOT" \
    --export-debug "Android" "$OUTPUT"

# ─── Verify ───────────────────────────────────────────────────────────────────
if [ ! -f "$OUTPUT" ]; then
    die "godot exited 0 but $OUTPUT not found — export silently failed?"
fi

SIZE_BYTES=$(stat -c%s "$OUTPUT" 2>/dev/null || stat -f%z "$OUTPUT")
SIZE_MB=$((SIZE_BYTES / 1024 / 1024))

log "✅ APK built: $OUTPUT ($SIZE_MB MB)"

# Sanity: APK should be at least 50 MB (Mono runtime is ~40 MB alone)
if [ "$SIZE_BYTES" -lt 50000000 ]; then
    warn "APK is suspiciously small ($SIZE_MB MB) — expected ~110 MB with Mono runtime"
    warn "check that export templates include the .mono variant (not bare 4.7.2.stable)"
fi
