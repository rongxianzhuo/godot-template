# godot-template Makefile
# Aggregates scripts/ for common workflows. Run `make help` for an overview.
#
# Phony targets (no files produced):
#   help        — print available targets
#   setup       — full toolchain install (~25 min, ~1.3 GB download incl. NDK)
#   setup-quick — same as setup but skips Android NDK (saves ~670 MB)
#   sync        — sync the godot-framework submodule
#   test        — run algorithm-layer unit tests (~5 sec)
#   smoke       — run in-Godot smoke test (~30 sec)
#   build       — export debug APK (~5 min for first build)
#   verify      — test + dotnet build (CI gate — no APK export)
#   all         — setup + test + build
#   clean       — remove local build artifacts (keeps builds/ APK)
#
# Override variables (passed as env vars to scripts):
#   SKIP_NDK=1                   — skip NDK install in install-toolchain.sh
#   GODOT_VERSION=4.7.2-stable   — Godot release tag
#   OUTPUT=builds/foo.apk        — output path for build-debug.sh
#   EXPECTED_SUBMODULE_SHA=<hex> — fail if submodule HEAD doesn't match

SHELL := /usr/bin/env bash

.PHONY: help setup setup-quick sync test smoke build verify all clean

help:
	@printf "%s\n" \
		"godot-template — Make targets:" \
		"" \
		"  make setup         Full toolchain install (~25 min, 1.3 GB)" \
		"  make setup-quick   Skip Android NDK (saves ~670 MB)" \
		"  make sync          Sync the godot-framework submodule" \
		"  make test          Run algorithm-layer tests (93 cases, ~5 sec)" \
		"  make smoke         Run in-Godot smoke test (needs android templates)" \
		"  make build         Export debug APK (~5 min first build)" \
		"  make verify        test + dotnet build (CI gate — no APK)" \
		"  make all           setup + test + build (full pipeline)" \
		"  make clean         Remove local build artifacts" \
		"" \
		"Override vars: SKIP_NDK=1, GODOT_VERSION=4.7.2-stable, OUTPUT=path.apk"

setup:
	bash scripts/install-toolchain.sh
	bash scripts/install-android-templates.sh
	bash scripts/sync-submodule.sh

setup-quick:
	SKIP_NDK=1 bash scripts/install-toolchain.sh
	bash scripts/install-android-templates.sh
	bash scripts/sync-submodule.sh

sync:
	bash scripts/sync-submodule.sh

test:
	bash scripts/test-algorithm.sh

smoke:
	bash scripts/smoke-test.sh

verify: test
	dotnet build GodotTemplate.csproj -c Debug

build:
	bash scripts/build-debug.sh

all: setup test build

clean:
	rm -rf .godot/mono/temp/bin .godot/mono/temp/obj
	rm -rf obj bin
	@echo "removed local build artifacts (builds/ APK retained)"
