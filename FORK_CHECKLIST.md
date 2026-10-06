# Fork Checklist — godot-template → Your Game

Run this list **before your first commit** on a forked godot-template.
Every unchecked item is a leak of "Magic Match" / "MagicStudio" identity
into your new game.

## 0. Inject your app identity (1 min)

```bash
bash scripts/configure.sh \
    --name "My Game" \
    --package "com.example.mygame" \
    --author "Your Name" \
    --description "One-line description"
```

This rewrites `export_presets.cfg`, `project.godot`, and `LICENSE` to
match your game. Re-run with the same args to verify (idempotent).

If you prefer env vars (CI / non-interactive):

```bash
export APP_NAME="My Game"
export APP_PACKAGE="com.example.mygame"
export APP_AUTHOR="Your Name"
bash scripts/configure.sh
```

**Verify**:
```bash
grep -E 'MagicMatch|magicstudio|emptygodot|EmptyGodot' export_presets.cfg project.godot
# should print nothing
```

## 0.5. Migrate off deprecated event pattern (only if forked < v1.0)

The pre-v1.0 event pattern `event Action<T>?` on algorithm classes
(`GameState.PhaseChanged`, `ScoreManager.ScoreChanged` / `MovesChanged`)
was deprecated in W2 in favor of `EventBus.Instance.Publish<TEvent>(evt)`.
After v1.1 ships, those `[Obsolete]` fields will be **removed**.

**If your fork subscribes to those events** (compile-time CS0618 warning):

```bash
# Find your subscriptions:
grep -rn 'PhaseChanged\|\.ScoreChanged\|\.MovesChanged' src/
```

Replace each `+= handler` with `EventBus.Instance.Subscribe<TEvent>(handler)`
(store the `IDisposable` token as a field; dispose in `_ExitTree`). See
`AGENTS.md` §"EventBus Migration Recipe" for the full pattern + the
`feature/eventbus-migration` commit for the reference migration.

Forks that don't use the legacy events can skip this step.

## 1. Delete demo residue (5 min)

```bash
git rm -r src/Features/Counter/                          # v0.0 trivial demo
git rm src/Features/Match3/Match3SmokeTest.cs           # Match-3 smoke test (specific to this game)
git rm src/Features/Match3/Match3SmokeTest.cs.uid       # match the .uid sidecar
```

> Re-run `godot --headless --import` after deletion to clean the import cache.
> Verify with `make test && make smoke` — algorithm tests still pass, smoke
> test exits cleanly.

**Optional but recommended** (delete the entire Match-3 game if you're
forging for a different genre):

```bash
git rm -r src/Features/Match3/
git rm -r tests/Match3Tests/
git rm -r assets/gems/ assets/backgrounds/
git rm docs/ART_SPEC.md docs/ASSET_INTEGRATION.md
```

## 2. Replace hardcoded app identity (2 min)

Edit `export_presets.cfg`:

| Section | Field | Change from | Change to |
|---|---|---|---|
| `[preset.0]` | `export_path` | `builds/MagicMatch-debug.apk` | `builds/<your-app>-debug.apk` |
| `[preset.0.options]` | `package/unique_name` | `org.magicstudio.match3` | `com.<your-org>.<your-app>` |
| `[preset.0.options]` | `package/name` | `Magic Match` | `<Your App Name>` |
| `[preset.0.options]` | `keystore/debug` | `/root/build-godot/keystore/debug.keystore` | `${HOME}/.android/debug.keystore` (the standard location) |

Edit `project.godot`:

| Section | Field | Change from | Change to |
|---|---|---|---|
| `[application]` | `config/name` | `GodotTemplate` | `<Your App Name>` |
| `[application]` | `config/description` | `A Godot 4 + C# + Android project template.` | `<Your one-line description>` |
| `[android]` | `package/unique_name` | `com.example.emptygodot` | `com.<your-org>.<your-app>` (must match export_presets.cfg) |
| `[android]` | `package/name` | `EmptyGodot` | `<Your App Name>` (must match) |

**Verification**:
```bash
grep -rn 'MagicMatch\|magicstudio\|emptygodot\|EmptyGodot\|build-godot' . \
    --include='*.cfg' --include='*.godot' --include='*.tres' \
    --include='AGENTS.md' --include='README.md'
# should return zero hits
```

## 3. Debug keystore (auto — usually no work)

`scripts/configure.sh` (step 0) auto-generates a standard Android debug
keystore at `$HOME/.android/debug.keystore` if one isn't already there.
The credentials are the standard debug pair (alias=`androiddebugkey`,
passwords=`android`), which matches `export_presets.cfg`'s
`keystore/debug_user` and `keystore/debug_password`.

If you used a non-default keystore path via `--keystore`, configure.sh
generates there instead.

**If you see "Unable to open keystore" during `make build`**:
- Run `bash scripts/configure.sh` again (it checks + generates on each run)
- Or generate manually:
  ```bash
  mkdir -p ~/.android
  keytool -genkeypair \
      -keystore ~/.android/debug.keystore \
      -storepass android \
      -alias androiddebugkey \
      -keypass android \
      -dname "CN=Android Debug,O=Android,C=US" \
      -keyalg RSA -keysize 2048 \
      -validity 10000
  ```

(Founder decision pending for **release** keystore strategy — see
`docs/release-keystore.md` once that lands. configure.sh only handles
the debug keystore.)

## 4. Replace project icon (1 min)

```bash
# Replace the launcher icon
cp path/to/your/icon.svg assets/icons/icon.svg
# Replace the in-game icon (root-level, used by export_presets.cfg)
cp path/to/your/icon.svg icon.svg
```

Format: 1024×1024 SVG preferred (Godot will rasterize to all Android
density buckets). See Bev's icon_alpha guidance if your source is a
raster PNG with alpha issues.

## 5. Update LICENSE copyright (30 sec)

Edit `LICENSE`:

```diff
- Copyright (c) 2026 rongxianzhuo
+ Copyright (c) <year> <your name or org>
```

Decide on license: MIT (recommended, matches upstream) or your own.

## 6. Update submodule if needed (optional)

The default submodule HEAD is `0edd419` (godot-framework v0.2.3). To
use a newer or older version:

```bash
cd addons/godot-framework
git fetch
git checkout <branch-or-sha>          # e.g. main, v0.3.0, abc1234
cd ../..
git add addons/godot-framework
git commit -m "submodule: bump godot-framework to <sha>"
```

**Warning**: the submodule is unpinned by default. Each PR gets human
review of the submodule HEAD diff. If you need CI reproducibility, set
`EXPECTED_SUBMODULE_SHA=<sha>` in your CI env.

## 7. Replace your fork's app/builds output naming (1 min)

The `OUTPUT` env var on `make build` lets you name the APK explicitly:

```bash
OUTPUT=builds/<your-app>-debug.apk make build
```

Or edit `scripts/build-debug.sh` default `OUTPUT` value.

## 8. Verify (5 min)

```bash
# 1. Toolchain present
make setup-quick         # SKIP_NDK=1 if you don't need APK builds locally

# 2. Algorithm tests pass (if you kept tests/)
make test                # should print N/N passed

# 3. In-Godot smoke test passes (if you kept Match3SmokeTest)
make smoke               # should print [Match3SmokeTest] === END (PASS) ===

# 4. APK builds
make build               # should produce builds/<your-app>-debug.apk
```

All four should exit 0. If any fail, your fork has lingering demo
code or missing config.

## 9. (Optional) Update README

The repo's `README.md` describes Match-3 specifically. After fork:

1. Replace the "Project structure" section with your game's structure
2. Replace the "踩过的坑" (pitfalls) section if you hit new ones
3. Keep the "toolchain install" instructions — those are generic

---

## Appendix: what NOT to delete (the "safe to keep" list)

These work for any game built on godot-template:

- `src/Core/Bootstrap.cs`, `src/Core/GodotFeature.cs` — feature bootstrap machinery
- `scripts/` (after replacing MagicMatch names) — build pipeline
- `tests/Match3Tests/` — your own algorithm tests should follow the same pattern (rename to `tests/YourGameTests/`)
- `addons/godot-framework/` (submodule) — UI panel stack, service registry
- `AGENTS.md` — agent working contract, generic to any project on this template
- `.gitignore` — generic patterns

## Appendix: what the framework submodule requires on fork

Per Francisco (Mark relayed 2026-10-05):

- `addons/godot-framework/project.godot` (submodule's own project.godot) has
  `assembly_name="GodotFramework"` — your fork's csproj must NOT collide
  with this name.
- `addons/godot-framework/samples/Demo/` — make sure your fork's
  `GodotTemplate.csproj` keeps `<Compile Remove="addons/godot-framework/samples/**/*.cs" />`
  or you'll pollute your assembly with `GameFramework.Demo.*` classes.
