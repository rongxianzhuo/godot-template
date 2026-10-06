#!/usr/bin/env bash
# smoke-test.sh — Run Match-3's in-Godot smoke test headlessly.
#
# The smoke test lives in src/Features/Match3/Match3SmokeTest.cs. It gates on
# the MATCH3_SMOKE_TEST env var: when unset, the feature is a no-op; when set
# to "1" or "true", it builds the algorithm layer + GameState inside a real
# Godot runtime, runs 12 assertions, prints PASS/FAIL, and quits.
#
# Why this exists alongside tests/Match3Tests/ (which is plain .NET):
#   - Catches class-load + reflection issues that `dotnet run` cannot see.
#   - Verifies Godot source generators ran (partial class : Node binding).
#   - Cheap sanity check before burning 5 minutes on a full APK export.
#
# Exit code: 0 if smoke test passes, 1 otherwise.
#
# Usage:
#     bash scripts/smoke-test.sh

set -euo pipefail

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { printf "${GREEN}[%(%H:%M:%S)T]${NC} %s\n" -1 "$*"; }
warn() { printf "${YELLOW}[%(%H:%M:%S)T WARN]${NC} %s\n" -1 "$*" >&2; }
err()  { printf "${RED}[%(%H:%M:%S)T ERR]${NC} %s\n" -1 "$*" >&2; }
die()  { err "$*"; exit 1; }

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$REPO_ROOT"

# ─── Preflight ────────────────────────────────────────────────────────────────
GODOT_BIN="${GODOT_BIN:-$(command -v godot || true)}"
if [ -z "$GODOT_BIN" ] || [ ! -x "$GODOT_BIN" ]; then
    die "godot not found in PATH. Run scripts/install-toolchain.sh and source /opt/godot-template-env.sh"
fi
log "using godot: $($GODOT_BIN --version 2>&1 | head -1)"

# ─── Run smoke test ───────────────────────────────────────────────────────────
# Note: --quit-after takes main-loop ITERATIONS (frames), NOT seconds. At 60 FPS,
# 1800 frames = ~30 seconds wall-clock — enough headroom for the W3+ splash
# (2s on SplashScreen.tscn) + bootstrap + 12-assertion smoke test.
# Pre-W3 (Main.tscn as run/main_scene) used --quit-after 30 because bootstrap
# was instant. After SplashScreen was added in W3, 30 frames (0.5s) wasn't
# enough — splash's SceneTreeTimer.Timeout would never fire.
GODOT_ARGS=(
    --headless
    --path "$REPO_ROOT"
    --quit-after 1800
)

log "running MATCH3_SMOKE_TEST=1 godot --headless --quit-after 1800 (1800 frames ≈ 30s @ 60fps)"
log "(Godot will import assets on first run — may take 30-60 sec)"

# Capture all output. The smoke test prints PASS/FAIL to stdout; any stderr
# noise from Godot import is normal first-run chatter.
set +e
OUTPUT="$(MATCH3_SMOKE_TEST=1 "$GODOT_BIN" "${GODOT_ARGS[@]}" 2>&1)"
EXIT_CODE=$?
set -e

# Surface the relevant slice of output
echo "$OUTPUT" | grep -E '\[Match3SmokeTest\]|\[Bootstrap\]|ERROR|FAIL' | tail -30
echo ""
echo "(truncated; full log available by re-running without grep)"

# ─── Verdict ──────────────────────────────────────────────────────────────────
if [ "$EXIT_CODE" -eq 0 ] && echo "$OUTPUT" | grep -q 'END (PASS)'; then
    log "✅ smoke test passed"
    exit 0
fi

err "❌ smoke test failed (exit code $EXIT_CODE)"
if echo "$OUTPUT" | grep -q 'FAIL:'; then
    echo "$OUTPUT" | grep 'FAIL:' | head -5 | sed 's/^/    /'
fi
exit 1
