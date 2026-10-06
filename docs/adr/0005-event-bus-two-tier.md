# ADR-0005: EventBus 两层架构 (EventDispatcher + EventBus)

> **Status**: ✅ Accepted
> **Date**: 2026-10-01 (D+2)
> **Deciders**: Mark + Francisco + Jacob

## Context

Match-3 game 需要 cross-feature 通信：
- `GameState.PhaseChanged` → `Match3BoardView` 触发 update
- `ScoreManager.ScoreChanged` → `HUD` 显示新分数
- `MatchEngine.MatchFound` → `Match3BoardView` 播放消除动画

约束：
- 直接订阅 `event Action<T>?` 容易 memory leak（忘 unsubscribe）
- C# event 强耦合 publisher/subscriber（同 assembly）
- Fork 用户添加 listener 时 boilerplate 多
- 需要跨 assembly 通信（addons/godot-framework + 主项目）

## Decision

采用 **EventBus 两层架构**：
- **底层**：`EventDispatcher<TEvent>`（generic, type-safe, 单实例）
- **高层**：`EventBus.Instance`（singleton wrapper，便于 fork 友好 API）

```csharp
// 底层（addons/godot-framework）
public class EventDispatcher<TEvent> where TEvent : class
{
    private readonly List<Func<TEvent, Task>> _handlers = new();
    public IDisposable Subscribe(Func<TEvent, Task> handler) { ... }
    public async Task PublishAsync(TEvent evt) { ... }
}

// 高层（单例 wrapper）
public static class EventBus
{
    public static EventDispatcher<TEvent> For<TEvent>() where TEvent : class
        => ServiceRegistry.Get<EventDispatcher<TEvent>>();
}
```

**Subscribe pattern**（return IDisposable token，subscriber 必须 dispose）：

```csharp
_token = EventBus.Instance.Subscribe<GameStateChangedEvent>(OnPhaseChanged);

public override void _ExitTree()
{
    _token?.Dispose();  // 防止 memory leak
}
```

## Consequences

**Positive**:
- ✅ Type-safe（generic `TEvent`）
- ✅ Cross-assembly 通信（addons + 主项目）
- ✅ IDisposable token 防止 memory leak
- ✅ Async-friendly（`PublishAsync` 支持 await handler）
- ✅ Fork 用户添加 listener boilerplate 减少

**Negative**:
- ❌ 静态单例（test 时需要 reset）
- ❌ Subscribe 必须 dispose（fork 用户可能忘）
- ❌ Debug 时 event flow 不直观（需要查 dispatcher 注册表）

**Neutral**:
- 🟢 Event 类型用 `record class`（immutable）
- 🟢 ServiceRegistry 注册 dispatcher 单例

## Alternatives Considered

### Alternative A: C# native event (`event Action<T>?`)

- ❌ Rejected：易 memory leak（忘 `-=`）
- ❌ Rejected：不能 cross-assembly（除非用 weak event pattern）
- ❌ Rejected：W2 deprecated（per `feature/eventbus-migration` commit）

### Alternative B: WeakReference + polling

- ❌ Rejected：performance overhead
- ❌ Rejected：AI agent 不熟

### Alternative C: MediatR / message bus library

- ❌ Rejected：增加 nuget 依赖
- ❌ Rejected：fork 用户增加 setup 步骤

## Cross-references

- `addons/godot-framework/Events/EventDispatcher.cs`
- `addons/godot-framework/Events/EventBus.cs`
- `src/Features/Match3/Events/GameEvents.cs`
- `feature/eventbus-migration` commit（W2 迁移 PR）
- `docs/fork-guide-examples.md` §8（提到 `EventBus.Instance.Subscribe` pattern）
- FORK_CHECKLIST.md §0.5（迁移指南）
- ADR-0008: Match3 screen migration
