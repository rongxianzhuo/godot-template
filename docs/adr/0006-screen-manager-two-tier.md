# ADR-0006: ScreenManager 两层架构 (ScreenRouter + ScreenManager)

> **Status**: ✅ Accepted
> **Date**: 2026-10-02 (D+3)
> **Deciders**: Mark + Francisco + Jacob

## Context

Match-3 game 需要 screen 切换：
- `TitleScreen`（Play） → `GameScreen`（开始游戏）
- `GameScreen`（won/oom） → `EndScreen`（结束界面）
- `EndScreen`（Play Again） → `GameScreen`（重玩同关）
- `EndScreen`（Main Menu） → `TitleScreen`（回首页）

约束：
- Godot 4.7 scene 切换用 `SceneTree.ChangeSceneToPacked()`，但：
  - 需要 PackedScene 资源
  - 切换时状态丢失（per-scene state 不跨屏）
  - 切换动画 hard（无 cross-fade 支持）
- 直接 `GetTree().ChangeSceneToFile()` 太简单（无 type safety）
- Match-3 屏幕切换需要 arguments（`GameStartArgs { Level, Theme }`）

## Decision

采用 **ScreenManager 两层架构**：
- **底层**：`ScreenRouter`（generic, type-safe, route definition）
- **高层**：`ScreenManager`（state management + async show pattern）

```csharp
// 底层（addons/godot-framework）
public abstract class ScreenRouter { ... }
public abstract class ScreenRouter<TScreen, TArgs> : ScreenRouter
    where TScreen : Node where TArgs : class { ... }

// 高层
public class ScreenManager
{
    public async Task ShowAsync<TScreen>(TArgs args) { ... }
    public async Task PopAsync() { ... }
}
```

**Match3 usage**：

```csharp
// Match3Feature.cs 启动 loop
await ScreenManager.ShowAsync(TitleScreen);
await ScreenManager.ShowAsync(GameScreen, new GameStartArgs { Level = 1 });
await ScreenManager.ShowAsync(EndScreen, new EndScreenArgs { Won = true });
```

## Consequences

**Positive**:
- ✅ Type-safe（generic `TScreen, TArgs`）
- ✅ Cross-fade 动画 built-in（per `ScreenRouter.TransitionStyle`）
- ✅ State preservation（per-screen state 不跨屏 leak）
- ✅ Async-friendly（`ShowAsync` 支持 await transition）
- ✅ 测试容易（Mock ScreenRouter）

**Negative**:
- ❌ ScreenRouter abstraction 增加 complexity（vs 直接 ChangeSceneToFile）
- ❌ 每个 screen 要写对应的 Router class（boilerplate）
- ❌ Debug 时 transition state 不直观

**Neutral**:
- 🟢 ScreenRouter 生命周期 = screen 生命周期
- 🟢 Async transition 默认 SineInOut 200ms

## Alternatives Considered

### Alternative A: `SceneTree.ChangeSceneToFile()`

- ❌ Rejected：无 type safety
- ❌ Rejected：无 transition 动画
- ❌ Rejected：无 arguments passing

### Alternative B: `GetTree().ChangeSceneToPacked(PackedScene)`

- ❌ Rejected：需要 PackedScene 资源（每个 screen 都要 build）
- ❌ Rejected：arguments passing hacky（global singleton）

### Alternative C: 自己实现 finite state machine

- ❌ Rejected：增加 complexity
- ❌ Rejected：每个 fork 用户重新实现

## Cross-references

- `addons/godot-framework/Screens/ScreenRouter.cs`
- `addons/godot-framework/Screens/ScreenManager.cs`
- `src/Features/Match3/Match3Feature.cs`（screen loop）
- `src/Features/Match3/Screens/TitleScreen.cs`
- `src/Features/Match3/Screens/GameScreen.cs`
- `src/Features/Match3/Screens/EndScreen.cs`
- `docs/design/splash-screen-build-spec.md` §3（splash 也用 ScreenRouter）
- ADR-0008: Match3 screen migration
