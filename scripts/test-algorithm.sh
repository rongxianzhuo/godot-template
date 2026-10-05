#!/usr/bin/env bash
# test-algorithm.sh — Run the Match-3 algorithm-layer unit tests.
#
# The tests live in tests/Match3Tests/ as a standalone .NET 9 console app
# that <Compile Include>-pulls the pure-C# algorithm files (Board,
# MatchEngine, ScoreManager, GameState, Gem) — NO Godot dependency.
# This is the fastest, most reliable regression net we have: catches
# algorithm bugs in <5 seconds without spinning up a Godot runtime.
#
# Exit code: 0 if all tests pass, 1 otherwise.
#
# Usage:
#   bash scripts/test-algorithm.sh

set -euo pipefail

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { printf "${GREEN}[%(%H:%M:%S)T]${NC} %s\n" -1 "$*"; }
warn() { printf "${YELLOW}[%(%H:%M:%S)T WARN]${NC} %s\n" -1 "$*" >&2; }
err()  { printf "${RED}[%(%H:%M:%S)T ERR]${NC} %s\n" -1 "$*" >&2; }
die()  { err "$*"; exit 1; }
section(){ printf "\n${GREEN}═══ $* ═══${NC}\n"; }

# ─── Preflight ────────────────────────────────────────────────────────────────
section "Preflight"

# Locate repo root (script may be invoked from anywhere)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
TESTS_DIR="$REPO_ROOT/tests/Match3Tests"

if [ ! -d "$TESTS_DIR" ]; then
    die "tests directory not found: $TESTS_DIR — are you running this from inside the godot-template repo?"
fi

# Find dotnet: prefer DOTNET_ROOT/dotnet, fall back to PATH
DOTNET_BIN="${DOTNET_ROOT:-}/dotnet"
if [ ! -x "$DOTNET_BIN" ]; then
    DOTNET_BIN="$(command -v dotnet || true)"
fi
if [ -z "$DOTNET_BIN" ] || [ ! -x "$DOTNET_BIN" ]; then
    die "dotnet not found. Run scripts/install-toolchain.sh first, or:
         export DOTNET_ROOT=/opt/dotnet
         export PATH=\$DOTNET_ROOT:\$PATH"
fi
log "using dotnet: $($DOTNET_BIN --version)"

# Verify the test csproj exists and uses net9.0 (the only TFM our build supports)
if ! grep -q "TargetFramework>net9.0" "$TESTS_DIR/Match3Tests.csproj"; then
    die "tests/Match3Tests/Match3Tests.csproj doesn't target net9.0 — fix it before running."
fi

# ─── Run tests ────────────────────────────────────────────────────────────────
section "Building & running tests/Match3Tests"

# `dotnet run` will build then run. --nologo quiets MSBuild chatter.
# We capture both stdout and stderr; the test runner uses Console.WriteLine
# for the ✓/✗ lines, so they're in stdout.
set +e
OUTPUT="$($DOTNET_BIN run --project "$TESTS_DIR" --nologo --verbosity quiet 2>&1)"
EXIT_CODE=$?
set -e

echo "$OUTPUT"

# ─── Summary ──────────────────────────────────────────────────────────────────
section "Summary"

if [ "$EXIT_CODE" -eq 0 ]; then
    PASS_LINE="$(echo "$OUTPUT" | grep -E 'Result: [0-9]+/[0-9]+ passed' | tail -1)"
    if [ -n "$PASS_LINE" ]; then
        log "✅ $PASS_LINE"
    else
        log "✅ all tests passed"
    fi
    exit 0
else
    err "❌ tests failed (exit code $EXIT_CODE)"
    err "see output above for failing assertions"
    exit 1
fi
