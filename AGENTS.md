# Agent Working Contract

This file defines what AI agents may and may not modify in this Godot project.
Any agent working on this codebase should read this first.

## Architecture: Feature Bootstrap

The project uses a **feature bootstrap** pattern. All gameplay and UI logic lives
in C# classes decorated with `[GodotFeature]`. At runtime, `src/Core/Bootstrap.cs`
scans the loaded assembly, finds these classes, instantiates them, and adds them
as children of the root node.

Adding a new feature = dropping a new `.cs` file into `src/Features/<Name>/`.
No resource file changes needed.

## File Boundary Rules

### ❌ DO NOT EDIT

**`*.tscn`** and **`*.tscn.uid`** — Godot scene format. Editor manages UID
mappings, `ext_resource` references, anchor data, and node tree shape. Hand-editing
is fragile: the editor may rewrite or invalidate the file on next save, and UID
consistency breaks silently.

**`*.cs.uid`** — Godot 4.4+ source-generator sidecar. Each `*.cs` file under
`src/` has a matching `*.cs.uid` with a single line:

```
uid://<22-char-base32>
```

These UIDs are referenced from `.tscn` / `.tres` via `uid://...` syntax.
Hand-editing desyncs UID mappings and breaks resource loading. **Godot
regenerates `.cs.uid` automatically when the matching `.cs` changes — let it.**

If a `.cs.uid` is stale: re-run `godot --headless --import` to regenerate,
or delete the `.uid` and let Godot re-create it. Never commit a manually
edited `.cs.uid`.

The single `.tscn` file in this project (`src/Main.tscn`) is intentionally
minimal — 4 lines: one root node + the Bootstrap script. It should never need to
be edited by an agent. If you need new nodes, create them in C# via `new Node()`,
configure them, and `AddChild()`.

If you genuinely need to load a pre-authored scene (e.g. for a complex visual
asset the editor handled), do it via `GD.Load<PackedScene>("res://path.tscn")`
**at runtime** — don't write to the .tscn source.

### ✅ FREE TO EDIT

Everything else. Common categories:

| File type | Examples | Notes |
|---|---|---|
| C# source | `*.cs`, `*.csproj`, `*.sln` | Plain text, code-first |
| Project config | `project.godot` | INI-like. Hand-editable. |
| Export config | `export_presets.cfg` | INI-like. Hand-editable. |
| Other configs | `*.cfg`, `.gdignore`, `*.gradle`, `AndroidManifest.xml` | Plain text. |
| Images | `*.svg`, `*.png`, `*.jpg`, `*.webp`, `*.ktx` | Binary, no internal refs. |
| Audio | `*.ogg`, `*.mp3`, `*.wav` | Binary. |
| Non-scene resources | `*.tres`, `*.material`, `*.theme`, `*.font` | Hand-editable, but editor may rewrite. Prefer creating in C#. |
| Shaders | `*.gdshader`, `*.shader` | Plain text. |
| Docs | `*.md` | Plain text. |

## Adding a New Feature

1. Create `src/Features/<FeatureName>/<FeatureName>Feature.cs`
2. Decorate the class with `[GodotFeature(Order = N, Category = "...")]`
3. Inherit from the appropriate `Node` subclass (`Control` for UI, `Node2D` for 2D,
   `Node3D` for 3D, `Node` for headless logic).
4. Override `_Ready` to build your UI programmatically — no `.tscn` involved.
5. `Bootstrap.cs` auto-discovers and instantiates it at game start.

Example:

```csharp
using Godot;
using GodotTemplate.Core;

namespace GodotTemplate.Features.HealthBar;

[GodotFeature(Order = 200, Category = "ui")]
public partial class HealthBarFeature : Control
{
    private ProgressBar _bar = null!;

    public override void _Ready()
    {
        _bar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 100,
            Value = 75,
        };
        _bar.SetAnchorsPreset(LayoutPreset.CenterTop);
        _bar.CustomMinimumSize = new Vector2(200, 20);
        AddChild(_bar);
    }

    public void SetHealth(float pct) => _bar.Value = pct;
}
```

## Loading Order

Features load in ascending `Order` value. Suggested conventions:

| Range | Use |
|---|---|
| 0–99 | Infrastructure: input, save/load, config |
| 100–199 | UI (HUD, dialogs, menus) |
| 200–299 | Gameplay systems |
| 300–899 | Game-specific features |
| 900+ | Debug / dev-only tools (strip from release if needed) |

Ties broken by full class name (ordinal).

## Feature Communication

Features should not reference each other directly. Use one of:

- **`EventBus.Instance.Publish<TEvent>(evt)` + `Subscribe<TEvent>(handler)`** (canonical,
  recommended — GodotFramework v0.4-alpha; see `src/Features/Match3/Events/GameEvents.cs`
  for the record-struct pattern)
- **Godot Groups / `GetTree().GetNodesInGroup()`** (for runtime queries)
- **Public methods** exposed via the Bootstrap (TBD — add when needed)

> **Old pattern (deprecated, removed in v1.1)**: bare C# `event Action<T>` /
> `event Action<T1, T2>` on algorithm classes (e.g. `GameState.PhaseChanged`,
> `ScoreManager.ScoreChanged`). Migrated to EventBus in W2 — keep old events
> `[Obsolete]` for one release window so forks can transition. See
> `feature/eventbus-migration` commit history for the migration recipe.

## When You Might Be Tempted to Edit a .tscn

| Temptation | Better alternative |
|---|---|
| "I'll add this UI element to Main.tscn" | Create a `Feature` class. Build the element in `_Ready`. |
| "I'll position this node with anchors" | Use `SetAnchorsPreset()` and `SetOffsetsPreset()` in C#. |
| "I'll wire signals in the editor" | Connect signals in C# with `myNode.Connect("signal", callable)`. |
| "I'll instance this complex visual asset" | Load it at runtime via `GD.Load<PackedScene>(...)`. Don't edit the source. |

## Dual-purpose project: Product + Template

The `master` branch of this repository serves two roles simultaneously.
Before making changes, identify which role your change serves, and follow
the rules below.

| Role | Audience | What they want |
|---|---|---|
| **Product** | End users, app store reviewers | A polished, shippable Match-3 game |
| **Template** | New game projects | A starting scaffold they can fork into a new game |

### When the role is "Product"

You're shipping the Match-3 game. Treat every commit as something that will
go to end users. Run `make verify && make build` before pushing.

- **Don't delete product code** to "clean up the template" — the template
  cleanup lives in `FORK_CHECKLIST.md`.
- **Don't rename public APIs** (`GemType` enum, `Match3Feature`,
  `SpritePaths.GemTypeToSprite`) without a migration plan. The Match-3
  product depends on them.
- **Performance, polish, content** all welcome. New gems, new animations,
  new screens — go.

### When the role is "Template"

Someone wants to fork this repo as the start of a new game. Your changes
should make their life easier, not harder.

- **Don't bake Match-3-specific assumptions into shared infrastructure.**
  If `GodotFeatureAttribute` learns something that's only useful for
  Match-3, you've broken the abstraction.
- **Add to `FORK_CHECKLIST.md`** when you introduce a new "this MUST be
  changed on fork" item (new hardcoded path, new env var, etc.).
- **Keep demos discoverable** but small. `CounterFeature` is the
  minimum-viable Feature — keep it alive as a "hello world" reference.

### File / directory classification

#### 📦 Product (keep, maintain, ship)

These files are the Match-3 game. They live in master and go to users.

| Path | Why |
|---|---|
| `src/Features/Match3/` | The game itself — Board, MatchEngine, Match3Feature, etc. |
| `tests/Match3Tests/` | Algorithm regression suite — runs in CI |
| `assets/gems/`, `assets/ui/`, `assets/backgrounds/` | Match-3 art assets (Bev domain) |
| `assets/icons/icon.svg` | Launcher icon — replace with real app icon before release |
| `docs/ART_SPEC.md` | Match-3 art specification (Bev) |
| `docs/ASSET_INTEGRATION.md` | Match-3 art integration guide (Bev/Robin) |
| `src/Features/Match3/Match3SmokeTest.cs` | In-Godot smoke test (release gate) |
| `src/Core/Bootstrap.cs`, `src/Core/GodotFeature.cs` | Bootstrap machinery — both product and template need it |

#### 🪓 Fork 必删 (delete when forking)

These are demo / scaffolding / personal-laptop residue. They MUST be
removed before shipping a fork as a new game. See `FORK_CHECKLIST.md`
for the exact commands.

| Path | Why |
|---|---|
| `src/Features/Counter/` | v0.0 trivial "frame counter" demo. The "hello world" example already lives in this file's `HealthBarFeature` snippet. |
| `src/Features/Match3/Match3SmokeTest.cs` (+ `.uid`) | Match-3 specific smoke test. Keep in master as release gate, but delete on fork unless you're keeping Match-3. |

#### ✏️ Fork 必改 (replace when forking)

These contain hardcoded values that name *this* game, not the fork's
game. Run `bash scripts/configure.sh --name <AppName> --package <pkg>`
to replace them all at once. See `FORK_CHECKLIST.md` for the full list.

| Path | Field | Currently |
|---|---|---|
| `export_presets.cfg` | `export_path` | `builds/MagicMatch-debug.apk` |
| `export_presets.cfg` | `package/unique_name` | `org.magicstudio.match3` |
| `export_presets.cfg` | `package/name` | `Magic Match` |
| `export_presets.cfg` | `keystore/debug` | `/root/build-godot/keystore/debug.keystore` (absolute, hardcoded) |
| `project.godot` | `config/name`, `config/description` | `GodotTemplate` / generic |
| `project.godot` | `[android] package/unique_name`, `package/name` | `com.example.emptygodot` / `EmptyGodot` |
| `LICENSE` | copyright | `(c) 2026 rongxianzhuo` |

### Feature Communication — don't hold direct refs

The existing rule lists signals / public methods / groups, but doesn't
forbid the obvious anti-pattern: `featureA._featureB.DoX()`. Codify it:

> **Anti-pattern**: feature `A` holds a typed reference to feature `B` and
> calls its methods directly. This couples the two features' lifecycles,
> breaks Bootstrap's reflection-based loading, and makes refactoring
> impossible.
>
> **Correct**: communicate via framework `EventBus.Instance.Publish<TEvent>(evt)`
> + `Subscribe<TEvent>(handler)` (v0.4-alpha in `addons/godot-framework`),
> services registered in `GameFramework.Instance.Services`, or Godot
> signals/groups.

#### EventBus Migration Recipe (W2+)

For features that previously held typed refs OR subscribed to legacy
`event Action<T>` fields:

```csharp
// 1. Define the event type (prefer record struct, in your feature's Events folder):
public readonly record struct MyEvent(int SomeValue);

// 2. Publisher: where the event source lives (algorithm class or feature):
using GameFramework;
using MyFeature.Events;
EventBus.Instance.Publish(new MyEvent(value: 42));

// 3. Subscriber: where the event consumer lives (usually another feature):
using var sub = EventBus.Instance.Subscribe<MyEvent>(evt =>
    GD.Print($"got MyEvent: {evt.SomeValue}"));
// sub auto-unsubscribes when scope exits — no leak even on reload
```

> **Note**: `EventBus` is autoloaded via `project.godot`
> (`EventBus="*res://addons/godot-framework/src/GameFramework/EventBus.cs"`).
> If you remove it from autoload, `EventBus.Instance` is null and publish
> will throw NullReferenceException. Keep the autoload.

### Dev-only features — `Match3SmokeTest` pattern

`Match3SmokeTest` shows the convention. Codify it:

```csharp
[GodotFeature(Order = 900+, Category = "debug")]
public partial class MyDevFeature : Node
{
    public override void _Ready()
    {
        if (OS.GetEnvironment("MY_DEV_FEATURE") != "1") return;
        // dev-only logic
    }
}
```

- `Order = 900+` keeps it last in the load sequence (see Loading Order table)
- `Category = "debug"` for grouping / log filtering
- Env var gate (`MY_DEV_FEATURE=1`) means it's a no-op in production

### Submodule update flow

The framework lives at `addons/godot-framework/` as a git submodule on
**detached HEAD** by default. To upgrade:

```bash
bash scripts/sync-submodule.sh          # check current SHA, verify state
cd addons/godot-framework
git fetch
git checkout main                       # or any branch/sha
git pull                                # if you checked out a branch
cd ../..
git add addons/godot-framework
git commit -m "submodule: bump godot-framework to <sha>"
```

**Per Mark 2026-10-05: the submodule is NOT permanently pinned.** Each
PR gets human review of the submodule HEAD diff. If Francisco pushes a
breaking change to framework `main`, Jacob is expected to review the diff
in the PR and either bump or stay.

For CI reproducibility, pin with `EXPECTED_SUBMODULE_SHA=<hex>` env var
when calling `sync-submodule.sh`.
