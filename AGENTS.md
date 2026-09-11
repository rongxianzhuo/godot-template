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

- **Signals** on a shared `EventBus` autoload (recommended for cross-cutting events)
- **Public methods** exposed via the Bootstrap (TBD — add when needed)
- **Godot Groups / `GetTree().GetNodesInGroup()`** (for runtime queries)

## When You Might Be Tempted to Edit a .tscn

| Temptation | Better alternative |
|---|---|
| "I'll add this UI element to Main.tscn" | Create a `Feature` class. Build the element in `_Ready`. |
| "I'll position this node with anchors" | Use `SetAnchorsPreset()` and `SetOffsetsPreset()` in C#. |
| "I'll wire signals in the editor" | Connect signals in C# with `myNode.Connect("signal", callable)`. |
| "I'll instance this complex visual asset" | Load it at runtime via `GD.Load<PackedScene>(...)`. Don't edit the source. |
