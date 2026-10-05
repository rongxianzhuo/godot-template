#!/usr/bin/env bash
# install-toolchain.sh — One-shot toolchain installer for godot-template.
#
# Installs everything needed to build the project on a fresh Ubuntu machine:
#   1. apt deps (git, ssh, JDK 17, libicu, fontconfig, unzip, ...)
#   2. .NET 9 SDK (LTS channel) to /opt/dotnet
#   3. Godot 4.7.2 Mono editor + symlink to /usr/local/bin/godot
#   4. Android SDK (cmdline-tools + platforms;android-36 + build-tools;36.1.0
#      + platform-tools) + NDK r26d to $HOME/android-sdk
#
# Idempotent: re-runnable. Each step checks before installing.
#
# After running, source the env file:
#     source /opt/godot-template-env.sh
# (or copy the exports into your ~/.bashrc / ~/.zshrc)
#
# Tested on: Ubuntu 24.04 (Noble). Ubuntu 22.04 should also work.
#
# ⚠️  Read this before running — these are the foot-guns you can't script away:
#
#   - Godot 4.7.2 GH release page renders the Linux Mono artifact as
#     "mono_linux.x86_64.zip" (with a DOT). The real URL uses UNDERSCORES:
#     "mono_linux_x86_64.zip". This script uses the correct URL.
#
#   - .NET 9 runtime requires libicu ≥ 70 (Debian/Ubuntu package name pattern
#     "libicuNN"). On Ubuntu 24.04 this is libicu78; on 22.04 it's libicu74.
#     We let apt-cache pick the newest available.
#
#   - C# / Android non-Gradle export path REQUIRES TFM = net9.0. Don't change
#     GodotTemplate.csproj's TargetFramework without re-validating the APK build.
#
#   - 512 MB memory + Gradle build = OOM. The project uses Gradle-less legacy
#     export (use_gradle_build=false). If you switch to Gradle, give the box
#     ≥ 2 GB RAM or prepare for crash loops.
#
# Usage:
#   sudo bash scripts/install-toolchain.sh
#
# Environment overrides (optional):
#   PREFIX=/opt                     Where dotnet and godot go (default /opt)
#   ANDROID_HOME=$HOME/android-sdk  Where Android SDK goes
#   DOTNET_VERSION=9.0              .NET SDK channel (default 9.0)
#   GODOT_VERSION=4.7.2-stable      Godot release tag
#   ANDROID_NDK_VERSION=r26d        Android NDK tag
#   SKIP_NDK=1                      Skip NDK download (~670 MB, 5–10 min)

set -euo pipefail

# ─── Config ───────────────────────────────────────────────────────────────────
PREFIX="${PREFIX:-/opt}"
ANDROID_HOME="${ANDROID_HOME:-$HOME/android-sdk}"
DOTNET_VERSION="${DOTNET_VERSION:-9.0}"
GODOT_VERSION="${GODOT_VERSION:-4.7.2-stable}"
ANDROID_NDK_VERSION="${ANDROID_NDK_VERSION:-r26d}"
ANDROID_PLATFORM="android-36"
ANDROID_BUILD_TOOLS="36.1.0"
SKIP_NDK="${SKIP_NDK:-0}"

# ─── Pretty output ────────────────────────────────────────────────────────────
GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log()    { printf "${GREEN}[%(%H:%M:%S)T]${NC} %s\n" -1 "$*"; }
warn()   { printf "${YELLOW}[%(%H:%M:%S)T WARN]${NC} %s\n" -1 "$*" >&2; }
err()    { printf "${RED}[%(%H:%M:%S)T ERR]${NC} %s\n" -1 "$*" >&2; }
die()    { err "$*"; exit 1; }
section(){ printf "\n${GREEN}═══ $* ═══${NC}\n"; }

# ─── Preflight ────────────────────────────────────────────────────────────────
section "Preflight"

if [ "$(id -u)" -ne 0 ]; then
    if command -v sudo >/dev/null; then
        die "this script must run as root. Re-run with: sudo bash $0"
    else
        die "this script must run as root (no sudo found)."
    fi
fi
log "running as root, ok"

# Check we're on a Debian/Ubuntu-like system
if ! command -v apt-get >/dev/null; then
    die "apt-get not found. This script targets Debian/Ubuntu. Adapt it for your distro."
fi
log "apt-get found, ok"

# Ensure we have what we'll need to do downloads and extractions
log "installing bootstrap packages"
apt-get update -qq
DEBIAN_FRONTEND=noninteractive apt-get install -y -qq \
    ca-certificates apt-transport-https \
    curl wget unzip zip \
    >/dev/null

# ─── Step 1/5: apt deps ───────────────────────────────────────────────────────
section "Step 1/5 — apt dependencies (JDK 17, libicu, fontconfig, ...)"

DEBIAN_FRONTEND=noninteractive apt-get install -y -qq \
    git openssh-client \
    openjdk-17-jdk-headless \
    libfontconfig1 \
    libfreetype6 libxinerama1 libxcursor1 libxrandr2 libxi6 \
    libgl1 libegl1 libasound2t64 \
    >/dev/null || DEBIAN_FRONTEND=noninteractive apt-get install -y -qq \
        git openssh-client \
        openjdk-17-jdk-headless \
        libfontconfig1 \
        libfreetype6 libxinerama1 libxcursor1 libxrandr2 libxi6 \
        libgl1 libegl1 libasound2 \
        >/dev/null

# libicu: pick the highest-numbered one available (74, 76, 78, ...).
ICU_PKG="$(apt-cache search '^libicu[0-9]+$' | awk '{print $1}' | sort -V | tail -1)"
if [ -z "$ICU_PKG" ]; then
    die "no libicu package found in apt — install one manually (libicu74, libicu78, ...)"
fi
log "installing $ICU_PKG (.NET 9 runtime dep)"
DEBIAN_FRONTEND=noninteractive apt-get install -y -qq "$ICU_PKG" >/dev/null

# ─── Step 2/5: .NET 9 SDK ─────────────────────────────────────────────────────
section "Step 2/5 — .NET ${DOTNET_VERSION} SDK"

DOTNET_ROOT="${DOTNET_ROOT:-$PREFIX/dotnet}"
mkdir -p "$DOTNET_ROOT"

if [ -x "$DOTNET_ROOT/dotnet" ] && "$DOTNET_ROOT/dotnet" --list-sdks 2>/dev/null | grep -q "^${DOTNET_VERSION}"; then
    log ".NET ${DOTNET_VERSION} SDK already installed at $DOTNET_ROOT, skipping"
else
    log "downloading dotnet-install.sh (official MS script)"
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh

    log "running dotnet-install.sh (channel ${DOTNET_VERSION})"
    # NOTE: --channel accepts version strings (e.g. "9.0") or aliases ("LTS", "STS").
    #       --quality is a separate axis with values: daily, preview, ga. LTS is
    #       NOT a valid quality — use --channel LTS or omit --quality entirely.
    /tmp/dotnet-install.sh \
        --channel "${DOTNET_VERSION}" \
        --install-dir "$DOTNET_ROOT" \
        --no-path \
        || die ".NET install failed — see output above"

    rm -f /tmp/dotnet-install.sh
fi
log ".NET SDK installed:"
"$DOTNET_ROOT/dotnet" --list-sdks | sed 's/^/    /'

# ─── Step 3/5: Godot 4.7.2 Mono ───────────────────────────────────────────────
section "Step 3/5 — Godot ${GODOT_VERSION} Mono"

GODOT_HOME="$PREFIX/godot"
mkdir -p "$GODOT_HOME"

# ⚠️  URL uses UNDERSCORES between "linux" and "x86_64" — GH release page
#     misleadingly displays dots. Don't trust the rendered link; trust this string.
GODOT_URL="https://github.com/godotengine/godot-builds/releases/download/${GODOT_VERSION}/Godot_v${GODOT_VERSION}_mono_linux_x86_64.zip"

if [ -x "$GODOT_HOME/Godot_v${GODOT_VERSION}_mono_linux_x86_64/Godot_v${GODOT_VERSION}_mono_linux.x86_64" ]; then
    log "Godot already extracted at $GODOT_HOME, skipping"
else
    log "downloading from $GODOT_URL (~103 MB)"
    curl -fL --retry 3 --retry-delay 5 -o /tmp/godot-mono.zip "$GODOT_URL" \
        || die "Godot download failed — check network or URL"

    log "extracting to $GODOT_HOME"
    unzip -q -o /tmp/godot-mono.zip -d "$GODOT_HOME"
    rm -f /tmp/godot-mono.zip
fi

# The zip's internal layout is:
#   Godot_v4.7.2-stable_mono_linux_x86_64/         <- outer dir name uses UNDERSCORES
#   └── Godot_v4.7.2-stable_mono_linux.x86_64      <- binary name uses DOT
# Mind the asymmetry — don't "fix" one without the other.
GODOT_BIN="$GODOT_HOME/Godot_v${GODOT_VERSION}_mono_linux_x86_64/Godot_v${GODOT_VERSION}_mono_linux.x86_64"
if [ ! -x "$GODOT_BIN" ]; then
    die "expected Godot binary at $GODOT_BIN but it's missing — zip layout changed?"
fi
ln -sf "$GODOT_BIN" /usr/local/bin/godot
log "Godot installed: $(godot --version 2>&1 | head -1)"

# ─── Step 4/5: Android SDK + NDK ──────────────────────────────────────────────
section "Step 4/5 — Android SDK + NDK ${ANDROID_NDK_VERSION}"

mkdir -p "$ANDROID_HOME/cmdline-tools"

if [ ! -x "$ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager" ]; then
    log "downloading Android cmdline-tools (~153 MB)"
    curl -fL --retry 3 --retry-delay 5 -o /tmp/cmdline-tools.zip \
        https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip \
        || die "cmdline-tools download failed"

    log "extracting"
    unzip -q -o /tmp/cmdline-tools.zip -d /tmp/cmdline-tools-extract
    # The zip extracts to cmdline-tools/bin/... — move it to cmdline-tools/latest/
    rm -rf "$ANDROID_HOME/cmdline-tools/latest"
    mv /tmp/cmdline-tools-extract/cmdline-tools "$ANDROID_HOME/cmdline-tools/latest"
    rm -rf /tmp/cmdline-tools-extract /tmp/cmdline-tools.zip
fi

export ANDROID_HOME
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export PATH="$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$PATH"

# Pre-accept SDK licenses so non-interactive installs work
log "accepting SDK licenses"
yes 2>/dev/null | sdkmanager --licenses >/dev/null 2>&1 || warn "license accept had non-fatal errors — re-run interactively if a later step complains"

# Core SDK components (small)
NEEDED_PACKAGES=(
    "platform-tools"
    "platforms;${ANDROID_PLATFORM}"
    "build-tools;${ANDROID_BUILD_TOOLS}"
)
if [ "$SKIP_NDK" != "1" ]; then
    NEEDED_PACKAGES+=("ndk;${ANDROID_NDK_VERSION}")
else
    warn "SKIP_NDK=1 — NDK ${ANDROID_NDK_VERSION} will NOT be installed (no Gradle build support)"
fi

log "installing SDK packages: ${NEEDED_PACKAGES[*]}"
sdkmanager "${NEEDED_PACKAGES[@]}" >/dev/null \
    || die "sdkmanager install failed — check licenses and try again"

log "SDK install verified:"
sdkmanager --list_installed 2>/dev/null | grep -E 'platform-tools|platforms;android|build-tools|ndk;' | sed 's/^/    /'

# ─── Step 5/5: env vars file ──────────────────────────────────────────────────
section "Step 5/5 — writing /opt/godot-template-env.sh"

JAVA_HOME_REAL="$(dirname "$(dirname "$(readlink -f "$(command -v java)")")")"

cat > "$PREFIX/godot-template-env.sh" <<EOF
# godot-template toolchain env. Source me:
#     source $PREFIX/godot-template-env.sh
# Or copy the lines below into your ~/.bashrc / ~/.zshrc.

export DOTNET_ROOT="$DOTNET_ROOT"
export PATH="$DOTNET_ROOT:\$PATH"

export JAVA_HOME="$JAVA_HOME_REAL"
export PATH="\$JAVA_HOME/bin:\$PATH"

export ANDROID_HOME="$ANDROID_HOME"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export ANDROID_NDK_HOME="$ANDROID_HOME/ndk/${ANDROID_NDK_VERSION}"
export PATH="\$ANDROID_HOME/cmdline-tools/latest/bin:\$ANDROID_HOME/platform-tools:\$PATH"
EOF

log "wrote $PREFIX/godot-template-env.sh"

# ─── Summary ──────────────────────────────────────────────────────────────────
section "✅ Toolchain installation complete"

cat <<EOF

Next steps:

    1. Load the env vars:
           source $PREFIX/godot-template-env.sh

    2. Run scripts/install-android-templates.sh to fetch the Godot Android
       export templates (~580 MB total — android_source + export_templates).

    3. Verify everything works:
           godot --version
           dotnet --list-sdks
           sdkmanager --list_installed

    4. From the repo root:
           bash scripts/sync-submodule.sh
           bash scripts/test-algorithm.sh
           bash scripts/build-debug.sh   # produces builds/*.apk

EOF
