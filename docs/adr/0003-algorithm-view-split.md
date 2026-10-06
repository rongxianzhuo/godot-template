# ADR-0003: Match3 算法层 + 视图层 分层

> **Status**: ✅ Accepted
> **Date**: 2026-09-30 (D+1)
> **Deciders**: Mark + Jacob

## Context

Match-3 game 核心逻辑（match detection, scoring, swap validation）需要：
1. **可测试**：用 unit test 覆盖所有 match / score / state 转换
2. **可重用**：未来 port 到 iOS / Web 时核心逻辑不动
3. **可独立 evolution**：算法升级（4-match → 5-match detection）不需要 view 改动

约束：
- Godot 4.7 C# 项目结构鼓励 view + logic 混合（per `.tscn` scene tree）
- 但 unit test framework 不喜欢 Godot Node（需要 `partial class` + `Node` 才能 attach scene）
- AI agent 写混合 code 容易出现 subtle bugs（view state leak 到 algorithm）

## Decision

Match3 采用 **算法层 / 视图层分层** 架构：

| 层 | 路径 | 类型 | 测试 |
|----|------|------|------|
| 算法层 | `src/Features/Match3/Algorithm/` | 普通 C# class | `tests/Match3Tests/`（xUnit）|
| 视图层 | `src/Features/Match3/UI/` | `partial class : Node` | `Match3SmokeTest`（headless Godot）|

**Algorithm 例子**：`Board.cs`, `MatchEngine.cs`, `ScoreManager.cs`, `GameState.cs`（plain C# class）

**View 例子**：`Match3BoardView.cs`, `GameScreen.cs`（Godot Node + scene）

## Consequences

**Positive**:
- ✅ 算法层 100% unit test 覆盖（93/93 tests passing per `make test`）
- ✅ 算法层 port 到其他引擎 / platform 时只需写 view
- ✅ AI agent 写算法层时不需要考虑 Godot scene tree
- ✅ 算法层无 side effect（deterministic），debug 简单

**Negative**:
- ❌ 分层 boilerplate（view 要包装 algorithm call）
- ❌ Algorithm + View 之间 state sync 需要 explicit message passing
- ❌ 简单游戏会觉得分层 overkill（match-3 不算超大）

**Neutral**:
- 🟢 EventBus（ADR-0005）用于 algorithm → view 通知
- 🟢 GameState.cs 是 single source of truth（avoid 算法 + view 状态 duplicated）

## Alternatives Considered

### Alternative A: 全混合（无分层）

- ❌ Rejected：unit test 覆盖率低（algorithm 嵌入 Node 难 test）
- ❌ Rejected：AI agent 难调试（algorithm state leak 到 scene tree）

### Alternative B: 全 ECS（Entity-Component-System）

- ❌ Rejected：match-3 不是 ECS-friendly game（entity 数量小）
- ❌ Rejected：ECS overhead + AI agent 不熟

### Alternative C: MVP (Model-View-Presenter)

- ❌ Rejected：MVP pattern 对 Godot scene tree 不友好
- ❌ Rejected：Presenter 类增加 complexity（无明显 gain for match-3）

## Cross-references

- `src/Features/Match3/Algorithm/Board.cs`
- `src/Features/Match3/UI/Match3BoardView.cs`
- `tests/Match3Tests/BoardTests.cs`
- ADR-0004: FeatureBootstrap
- ADR-0005: EventBus
