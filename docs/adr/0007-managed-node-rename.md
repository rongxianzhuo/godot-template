# ADR-0007: UIPanelBase → ManagedNodeBase 重命名

> **Status**: ✅ Accepted
> **Date**: 2026-10-03 (D+4)
> **Deciders**: Mark + Francisco + Jacob

## Context

`addons/godot-framework` 早期版本有 `UIPanelBase` 类（继承 `Panel`）。问题是：
- 名字暗示"只用于 UI panel"，但实际上所有 `Node` 都用它（gameplay, UI, manager）
- 导致 fork 用户误以为 `UIPanelBase` 是 UI-only base class

约束：
- Framework API 名字影响 fork 用户理解
- 重命名是 breaking change（需要 update 所有 call sites）
- Phase 1 (D+0-D+7) 是 accepting breaking changes 的窗口

## Decision

**Rename** `UIPanelBase` → `ManagedNodeBase`（per W3 commit `feature/ui-panel-rename`）：

| Before | After |
|--------|-------|
| `UIPanelBase : Panel` | `ManagedNodeBase : Node` |
| `UIPanel : UIPanelBase` | `ManagedNode : ManagedNodeBase` |

**Rationale**：
- "ManagedNode" 强调"由 framework 管理的 Node"（vs 裸 Node）
- 不再限制 `Panel`（可以是任何 `Node` subclass）
- API 名字与 actual usage 一致

## Consequences

**Positive**:
- ✅ API 名字与 actual usage 一致（不再误导）
- ✅ Fork 用户 understanding 更清晰
- ✅ 未来扩展（如 `ManagedNode2D`, `ManagedNode3D`）可基于此
- ✅ Search "ManagedNodeBase" → 找到所有 framework-managed nodes

**Negative**:
- ❌ Breaking change（所有 fork 用户需要 update）
- ❌ 文档 + tutorial 需要 update
- ❌ git blame 历史断裂（rename 后看不出原始 "UIPanel" 上下文）

**Neutral**:
- 🟢 Phase 1 是 accepting breaking changes 窗口（v1.0 锁定前）
- 🟢 rename commit 用了 `git mv` 保留历史

## Alternatives Considered

### Alternative A: 保留 `UIPanelBase`（不改名）

- ❌ Rejected：API 误导持续
- ❌ Rejected：未来 fork 用户认知负担

### Alternative B: 新增 `ManagedNodeBase`，保留 `UIPanelBase`（向后兼容）

- ❌ Rejected：增加 API surface（两个 base class）
- ❌ Rejected：fork 用户困惑（用哪个？）

### Alternative C: 删 `UIPanelBase`，所有 Node 直接继承 Godot `Node`

- ❌ Rejected：失去 framework 提供的 lifecycle hook（`_Ready`, `_ExitTree` 等扩展）

## Cross-references

- `addons/godot-framework/ManagedNodeBase.cs`
- `feature/ui-panel-rename` commit（W3 breaking change）
- AGENTS.md §"Framework Conventions"（提到 `ManagedNodeBase` 命名约定）
- docs/fork-guide-examples.md §8（FeatureBootstrap 演示继承 `Control`，不是 `ManagedNodeBase`）
- ADR-0004: FeatureBootstrap
