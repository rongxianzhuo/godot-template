#!/usr/bin/env bash
# sync-submodule.sh — Initialize and sync the godot-framework submodule.
#
# The framework lives at addons/godot-framework/ as a git submodule.
# After a fresh clone the directory is empty until this runs.
#
# ⚠️  Submodule checkouts are typically on DETACHED HEAD by default. That's
#     fine for reproducible builds but means `git pull` won't work directly
#     inside the submodule. To upgrade:
#
#         cd addons/godot-framework
#         git fetch
#         git checkout <branch-or-sha>
#         cd ../..
#         git add addons/godot-framework
#         git commit -m "submodule: bump godot-framework to <sha>"
#
#     Per Mark 2026-10-05: we DO NOT permanently pin the submodule. Each PR
#     gets a human review of the submodule HEAD diff. This script just
#     materializes whatever SHA is currently recorded in the parent repo.
#
# Optional env var:
#     EXPECTED_SUBMODULE_SHA=<40-char-hex>
#         Will fail if the checked-out HEAD doesn't match. Use this in CI
#         to pin the framework version for a particular release.
#
# Usage:
#     bash scripts/sync-submodule.sh

set -euo pipefail

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()  { printf "${GREEN}[%(%H:%M:%S)T]${NC} %s\n" -1 "$*"; }
warn() { printf "${YELLOW}[%(%H:%M:%S)T WARN]${NC} %s\n" -1 "$*" >&2; }
err()  { printf "${RED}[%(%H:%M:%S)T ERR]${NC} %s\n" -1 "$*" >&2; }
die()  { err "$*"; exit 1; }

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
SUBMODULE_DIR="$REPO_ROOT/addons/godot-framework"

cd "$REPO_ROOT"

# ─── Preflight ────────────────────────────────────────────────────────────────
log "syncing submodule at $SUBMODULE_DIR"

if [ ! -d "$SUBMODULE_DIR/.git" ] && [ ! -f "$SUBMODULE_DIR/.git" ]; then
    log "submodule not initialized, running update --init --recursive"
    git submodule update --init --recursive
else
    log "submodule already initialized, syncing"
    git submodule update --recursive
fi

if [ ! -d "$SUBMODULE_DIR/src/GameFramework" ]; then
    die "submodule synced but src/GameFramework/ missing — submodule URL broken?"
fi

# ─── Verify HEAD state ────────────────────────────────────────────────────────
cd "$SUBMODULE_DIR"

ACTUAL_SHA="$(git rev-parse HEAD)"
SHORT_SHA="$(git rev-parse --short HEAD)"
BRANCH="$(git symbolic-ref --short HEAD 2>/dev/null || echo "(detached HEAD)")"
COMMIT_DATE="$(git log -1 --format=%ci)"
COMMIT_SUBJECT="$(git log -1 --format=%s)"

log "submodule HEAD: $SHORT_SHA ($BRANCH)"
log "  date:   $COMMIT_DATE"
log "  commit: $COMMIT_SUBJECT"

# Warn on detached HEAD unless explicitly allowed
if [ "$BRANCH" = "(detached HEAD)" ]; then
    warn "submodule is on detached HEAD (default after 'git submodule update')"
    warn "to upgrade: cd addons/godot-framework && git checkout main && git pull"
    warn "           cd ../.. && git add addons/godot-framework && git commit"
fi

# Optional: enforce a specific SHA (for CI pinning)
if [ -n "${EXPECTED_SUBMODULE_SHA:-}" ]; then
    EXPECTED="$(echo "$EXPECTED_SUBMODULE_SHA" | tr -d '[:space:]')"
    if [ "$ACTUAL_SHA" != "$EXPECTED" ]; then
        die "submodule HEAD $ACTUAL_SHA does not match EXPECTED_SUBMODULE_SHA=$EXPECTED"
    fi
    log "✅ submodule HEAD matches EXPECTED_SUBMODULE_SHA"
fi

log "done"
