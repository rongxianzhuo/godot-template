#!/usr/bin/env bash
# install-android-templates.sh — Fetch Godot 4.7.2 Android export templates.
#
# In Godot 4.7+ the export_templates.tpz is the SINGLE SOURCE OF TRUTH for
# both build-time and export-time Android artifacts. Inside the .tpz, under
# the templates/ subdirectory, you'll find:
#
#   - android_source.zip      — the Godot Android build template. MUST be
#                               extracted to android/build/ (NOT android/ —
#                               README §Pitfalls #1). Used at export time
#                               by the legacy non-Gradle path.
#   - android_debug.apk       — debug APK template
#   - android_release.apk     — release APK template
#
# README §Pitfalls #4 still applies: the export_templates dir needs the
# `.mono` suffix → ~/.local/share/godot/export_templates/4.7.2.stable.mono/
#
# Total download: ~1.2 GB (the .tpz is much bigger than the bare editor
# because it bundles templates for every platform Godot supports).
#
# Usage:
#     bash scripts/install-android-templates.sh
#
# Override (optional):
#     GODOT_VERSION=4.7.2-stable      Different release tag
#     GODOT_EXPORT_DIR=/path/to/dir   Custom export_templates location

set -euo pipefail

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { printf "${GREEN}[%(%H:%M:%S)T]${NC} %s\n" -1 "$*"; }
warn() { printf "${YELLOW}[%(%H:%M:%S)T WARN]${NC} %s\n" -1 "$*" >&2; }
err()  { printf "${RED}[%(%H:%M:%S)T ERR]${NC} %s\n" -1 "$*" >&2; }
die()  { err "$*"; exit 1; }
section(){ printf "\n${GREEN}═══ $* ═══${NC}\n"; }

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

GODOT_VERSION="${GODOT_VERSION:-4.7.2-stable}"
# Convert "4.7.2-stable" → "4.7.2.stable" (export_templates path uses dots)
GODOT_VERSION_DOT="${GODOT_VERSION//-/.}"
ANDROID_TEMPLATES_DIR="${ANDROID_TEMPLATES_DIR:-$REPO_ROOT/android/build}"
GODOT_EXPORT_DIR="${GODOT_EXPORT_DIR:-$HOME/.local/share/godot/export_templates/$GODOT_VERSION_DOT.mono}"

cd "$REPO_ROOT"

# ─── URL ──────────────────────────────────────────────────────────────────────
# README §Pitfalls #5: GH release page renders "mono_linux.x86_64.zip" with
# a dot; export template URL uses underscores correctly here. Be paranoid.
EXPORT_TPL_URL="https://github.com/godotengine/godot-builds/releases/download/${GODOT_VERSION}/Godot_v${GODOT_VERSION}_export_templates.tpz"

log "Godot version: $GODOT_VERSION (export dir uses $GODOT_VERSION_DOT.mono)"

# ─── Preflight: how much of this is already installed? ────────────────────────
NEED_TPL=0
NEED_SRC=0
if [ ! -f "$GODOT_EXPORT_DIR/android_debug.apk" ] || [ ! -f "$GODOT_EXPORT_DIR/android_release.apk" ]; then
    NEED_TPL=1
fi
# AndroidManifest.xml lives at src/main/AndroidManifest.xml inside the
# extracted android_source.zip (it's a Gradle project layout, not a flat one).
if [ ! -f "$ANDROID_TEMPLATES_DIR/src/main/AndroidManifest.xml" ]; then
    NEED_SRC=1
fi

if [ "$NEED_TPL" -eq 0 ] && [ "$NEED_SRC" -eq 0 ]; then
    log "export templates + android/build/ already present, skipping download"
    section "✅ Android templates install complete (no work needed)"
    exit 0
fi

# ─── Download the .tpz once (it's the source of both) ────────────────────────
section "Step 1/3 — downloading export_templates.tpz"
log "URL: $EXPORT_TPL_URL (~1.2 GB)"
curl -fL --retry 3 --retry-delay 5 -o /tmp/godot-export-templates.tpz "$EXPORT_TPL_URL" \
    || die "export_templates.tpz download failed — check network or URL"
log "downloaded $(du -h /tmp/godot-export-templates.tpz | awk '{print $1}')"

# ─── Step 2/3: extract export templates to ~/.local/share/godot/... ───────────
section "Step 2/3 — export templates → $GODOT_EXPORT_DIR"

if [ "$NEED_TPL" -eq 1 ]; then
    mkdir -p "$GODOT_EXPORT_DIR"
    log "extracting .tpz to $GODOT_EXPORT_DIR (flattening templates/ subdir)"
    # .tpz is a zip whose root is "templates/" — but Godot 4.7+ looks for
    # android_debug.apk DIRECTLY under the version-named dir, not nested in
    # templates/. Use unzip -j to discard the inner directory structure.
    unzip -q -j -o /tmp/godot-export-templates.tpz -d "$GODOT_EXPORT_DIR"

    if [ ! -f "$GODOT_EXPORT_DIR/android_debug.apk" ]; then
        die "extracted templates don't include android_debug.apk — wrong URL?"
    fi
    log "✅ export templates ready at $GODOT_EXPORT_DIR/"
else
    log "export templates already installed, skipping"
fi

# ─── Step 3/3: extract android_source.zip → repo/android/build/ ───────────────
section "Step 3/3 — Android source template → $ANDROID_TEMPLATES_DIR"

if [ "$NEED_SRC" -eq 1 ]; then
    if [ ! -f "$GODOT_EXPORT_DIR/android_source.zip" ]; then
        die "android_source.zip missing from .tpz — Godot $GODOT_VERSION release broken?"
    fi

    # README §Pitfalls #1: extract to android/build/ (subdirectory), not android/
    rm -rf "$REPO_ROOT/android/build"
    mkdir -p "$REPO_ROOT/android"
    log "extracting android_source.zip → $REPO_ROOT/android/build/"
    unzip -q -o "$GODOT_EXPORT_DIR/android_source.zip" -d "$ANDROID_TEMPLATES_DIR"

    if [ ! -f "$ANDROID_TEMPLATES_DIR/src/main/AndroidManifest.xml" ]; then
        die "extracted android/build/ doesn't contain src/main/AndroidManifest.xml — wrong layout?"
    fi
    log "✅ android/build/ ready (Gradle project layout: src/main/AndroidManifest.xml)"
else
    log "android/build/ already populated, skipping"
fi

# ─── Cleanup ──────────────────────────────────────────────────────────────────
rm -f /tmp/godot-export-templates.tpz

section "✅ Android templates install complete"
log "next: bash scripts/build-debug.sh"
