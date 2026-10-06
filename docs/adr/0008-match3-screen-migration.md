# ADR-0008: Match3 屏幕切换迁移 (W3)

> **Status**: ✅ Accepted
> **Date**: 2026-10-04 (D+5)
> **Deciders**: Mark + Jacob

## Context

Match-3 game 在 W1-W2 是 `Match3Feature` 直接 `SwapScreen()` 私有方法切换 screen。W3 时要：
1. 迁到 `ScreenManager.ShowAsync()`（per ADR-0006）
2. 同时保留 EventBus 通信（per ADR-0005）
3. 拆分 algorithm + view（per ADR-0003）
4. 不破坏现有 unit tests（93/93 passing）

约束：
- W3 是接受 refactoring 窗口（D+5）
- 算法层 100% unit test 覆盖要保留
- 切换过程要可测试（不能依赖 Godot scene tree）

## Decision

Match-3 屏幕切换迁移（W3）：

| Before (W1-W2) | After (W3) |
|----------------|------------|
| `Match3Feature.SwapScreen()`（私有方法）| `ScreenManager.ShowAsync<T>(args)` |
| `Match3BoardView` 直接订阅 `MatchEngine.MatchFound`（action）| `Match3BoardView` 订阅 `EventBus.Instance.Subscribe<MatchFoundEvent>` |
| `GameState.PhaseChanged`（event Action）| `EventDispatcher<GameStateChangedEvent>` |
| `ScoreManager.ScoreChanged`（event Action）| `EventDispatcher<ScoreChangedEvent>` |
| Algorithm + View 在 Match3Feature.cs | Algorithm → `src/Features/Match3/Algorithm/`, View → `src/Features/Match3/UI/` |

**新增文件**：
- `src/Features/Match3/Screens/ScreenTypes.cs`（TitleScreenArgs, GameStartArgs, EndScreenArgs）
- `src/Features/Match3/Screens/TitleScreen.cs`
- `src/Features/Match3/Screens/GameScreen.cs`
- `src/Features/Match3/Screens/EndScreen.cs`
- `src/Features/Match3/Events/GameEvents.cs`（event types）
- `tests/Match3Tests/`（93 tests，不动）

**修改文件**：
- `src/Features/Match3/Match3Feature.cs`（重写为 thin orchestrator）
- `src/Features/Match3/Match3BoardView.cs`（event 订阅迁移）
- `src/Features/Match3/Algorithm/GameState.cs`（event Action → EventBus publish）

## Consequences

**Positive**:
- ✅ Match-3 屏幕切换用 type-safe ScreenManager（vs private SwapScreen）
- ✅ Algorithm + View 清晰分层（per ADR-0003）
- ✅ EventBus 跨 assembly 通信（addons + 主项目）
- ✅ 93/93 unit tests 保留（无 regression）
- ✅ Fork 用户 clear pattern（TitleScreen / GameScreen / EndScreen 模板）

**Negative**:
- ❌ W3 改动大（5 文件新增 + 5 文件修改）
- ❌ Match3Feature 完全重写（git blame 历史部分断裂）
- ❌ EventBus 静态单例（test 需要 reset）

**Neutral**:
- 🟢 Algorithm 层不动（继续 100% test 覆盖）
- 🟢 Match3SmokeTest 保留（headless smoke test）

## Alternatives Considered

### Alternative A: 保留 W2 SwapScreen 私有方法

- ❌ Rejected：与 ScreenManager 架构冲突（ADR-0006）
- ❌ Rejected：screen 切换无 type safety

### Alternative B: W3 拆分多个小 PR（per-screen 1 PR）

- ❌ Rejected：多个 PR 增加 review burden
- ❌ Rejected：每个 PR 中间状态 broken（main 不稳定）

### Alternative C: 完全重写（扔掉 W2 code）

- ❌ Rejected：93/93 unit tests 需要重写
- ❌ Rejected：算法层已经稳定，重写 = 风险

## Cross-references

- `feature/eventbus-migration` commit（W2 EventBus 迁移）
- `feature/w3-screen-migration` commit（W3 ScreenManager 迁移）
- `src/Features/Match3/Match3Feature.cs`（新 thin orchestrator）
- `src/Features/Match3/Screens/`（新目录）
- `src/Features/Match3/Events/GameEvents.cs`
- tests/Match3Tests/（93 tests 保留）
- ADR-0003: Algorithm/View split
- ADR-0005: EventBus
- ADR-0006: ScreenManager
