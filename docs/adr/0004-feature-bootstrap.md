# ADR-0004: FeatureBootstrap 反射架构

> **Status**: ✅ Accepted
> **Date**: 2026-09-30 (D+1)
> **Deciders**: Mark + Jacob

## Context

Godot 4.7 C# 项目需要管理多个 features（Match3 / Counter / Splash / Future）。每个 feature 需要：
1. 在 startup 时 instantiate
2. add to scene tree
3. 决定启动顺序
4. 不需要修改 `project.godot` 主 scene（让 fork 简单）

约束：
- Godot 默认需要 `.tscn` scene + `run/main_scene` config
- Fork 用户想加新 feature 要 copy-paste `.tscn`（容易出错）
- AI agent 加新 feature 时不希望 boilerplate（focus 在 logic）

## Decision

采用 **FeatureBootstrap 反射架构**：
- 所有 feature class 用 `[GodotFeature(Order=N, Category="...")]` attribute 装饰
- `src/Core/Bootstrap.cs` 在 `_Ready` 时用反射扫所有 assembly 的 `[GodotFeature]` 类
- 自动 instantiate + add child + 按 Order 排序

```csharp
[GodotFeature(Order = 200, Category = "gameplay")]
public partial class Match3Feature : Node { ... }
```

```csharp
// src/Core/Bootstrap.cs
foreach (var type in DiscoverFeatureTypes())
{
    var instance = Activator.CreateInstance(type) as Node;
    AddChild(instance);
}
```

## Consequences

**Positive**:
- ✅ 加新 feature 只需 1 个 `.cs` 文件（无需 `.tscn` + `project.godot` 改动）
- ✅ `Order` 控制启动顺序（Counter 100 < Match3 200，Counter 先）
- ✅ Fork 用户可以 copy-paste feature class 而不动 infrastructure
- ✅ AI agent 写 feature boilerplate 减少（focus 在 logic）
- ✅ 测试容易（feature 是 plain Node + `[GodotFeature]`）

**Negative**:
- ❌ Reflection 有小 performance overhead（negligible for 5-10 features）
- ❌ Debug 时 stack trace 多一层（feature → Bootstrap）
- ❌ IDE "Find References" 不显示反射调用（需要查 `[GodotFeature]` usages）

**Neutral**:
- 🟢 `Order` 默认 1000（如果没指定）
- 🟢 `Category` 仅用于文档（无 functional effect）

## Alternatives Considered

### Alternative A: 手动 register

- ❌ Rejected：每个 feature 都要改 `Bootstrap.cs` 的 register list（boilerplate）
- ❌ Rejected：fork 用户 merge 冲突多

### Alternative B: `.tscn` 主 scene + manual add_child

- ❌ Rejected：fork 用户加 feature 要 copy `.tscn` XML
- ❌ Rejected：AI agent 不擅长编辑 XML scene tree

### Alternative C: Service Locator pattern

- ❌ Rejected：增加 complexity（每个 feature 要 register + lookup）
- ❌ Rejected：Godot Node 已经 implicit service（add_child tree）

## Cross-references

- `src/Core/Bootstrap.cs`（60 行反射 + instantiate 逻辑）
- `src/Core/GodotFeature.cs`（attribute 定义）
- `src/Features/Counter/CounterFeature.cs`（最简 demo）
- `src/Features/Match3/Match3Feature.cs`（production example）
- docs/fork-guide-examples.md §8（FeatureBootstrap walkthrough）
- ADR-0003: Algorithm/View split
