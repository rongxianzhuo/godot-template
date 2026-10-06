# Architecture Decision Records (ADR)

> **作者**：Christine (Artist, doc steward)
> **Date**：2026-10-06 (D+19 周二)
> **状态**：ADR index for godot-template project
> **Format**：Michael Nygard ADR style (https://github.com/joelparkerhenderson/architecture_decision_records)

---

## 什么是 ADR？

**ADR (Architecture Decision Record)** 是记录**重要架构决策**的简短文档。每条 ADR 解释：
1. **Context**：当时面对什么问题 / 约束
2. **Decision**：选了哪个方案
3. **Consequences**：正反两面 trade-offs
4. **Alternatives**：考虑过但放弃的方案

每条 ADR 是不可变历史记录（"已接受" = 决策锁定）。如需改变决策，写新 ADR 并 mark 旧的为 Superseded。

---

## ADR 列表

| # | Title | Status | Date |
|---|-------|--------|------|
| [0001](0001-godot-4-csharp-android.md) | Godot 4.7.2 + C# (.NET 9) + Android 选型 | ✅ Accepted | 2026-09-29 |
| [0002](0002-fork-dual-identity.md) | godot-template = Match3 产品 + 模板 双身份 | ✅ Accepted | 2026-09-29 |
| [0003](0003-algorithm-view-split.md) | Match3 算法层 + 视图层 分层 | ✅ Accepted | 2026-09-30 |
| [0004](0004-feature-bootstrap.md) | FeatureBootstrap 反射架构 | ✅ Accepted | 2026-09-30 |
| [0005](0005-event-bus-two-tier.md) | EventBus 两层架构 (EventDispatcher + EventBus) | ✅ Accepted | 2026-10-01 |
| [0006](0006-screen-manager-two-tier.md) | ScreenManager 两层架构 (ScreenRouter + ScreenManager) | ✅ Accepted | 2026-10-02 |
| [0007](0007-managed-node-rename.md) | UIPanelBase → ManagedNodeBase 重命名 | ✅ Accepted | 2026-10-03 |
| [0008](0008-match3-screen-migration.md) | Match3 屏幕切换迁移 (W3) | ✅ Accepted | 2026-10-04 |
| [0009](0009-i18n-strategy.md) | i18n strategy (v1.1 3 langs / v2.0 8 langs) | ✅ Accepted | 2026-10-06 |
| [0010](0010-v1-ship-ready.md) | v1.0 ship-ready 双身份 (产品 + 模板) | ✅ Accepted | 2026-10-06 |
| 0011 | App Bundle per-language split (v1.2 polish #1) | 📋 Reserved | D+23 reserved |
| [0012](0012-per-theme-ttf.md) | Per-Theme TTF Overrides v1.2 phase 1 (weight remap) | ✅ Accepted | 2026-10-08 |

---

## Phase 1 决策时间线

```
2026-09-29 (D+0):  Godot 4.7.2 + C# + Android 选型 (0001)
2026-09-29 (D+0):  fork 双身份 (0002)
2026-09-30 (D+1):  Match3 算法/视图分层 (0003)
2026-09-30 (D+1):  FeatureBootstrap 反射 (0004)
2026-10-01 (D+2):  EventBus (0005)
2026-10-02 (D+3):  ScreenManager (0006)
2026-10-03 (D+4):  UIPanelBase 重命名 (0007)
2026-10-04 (D+5):  Match3 屏幕迁移 (0008)
2026-10-06 (D+7):  i18n strategy (0009)
2026-10-06 (D+7):  v1.0 ship-ready (0010)
```

Phase 1 (D+0 → D+7) 锁定了所有架构基础。Phase 2 (D+8 → D+21+) 在此基础上 polish / optimize。

---

## ADR 模板

新增 ADR 时复制以下模板：

```markdown
# ADR-NNNN: <Title>

> **Status**: Proposed | ✅ Accepted | ⚠️ Deprecated | 🚫 Superseded by ADR-XXXX
> **Date**: YYYY-MM-DD
> **Deciders**: Mark (Leader) + relevant agents (Jacob / Francisco / Austin / Christine)

## Context

<what problem we faced, what constraints>

## Decision

<what we chose, summarized>

## Consequences

**Positive**:
- ...

**Negative**:
- ...

**Neutral**:
- ...

## Alternatives Considered

### Alternative A: <name>

- ❌ rejected because <reason>

### Alternative B: <name>

- ❌ rejected because <reason>

## Cross-references

- <link to related docs>
- <link to related ADRs>
```

---

## 维护规范

1. **不可变历史**：已 Accepted 的 ADR 不编辑。如需改变，写新 ADR 并 supersede 旧的。
2. **每条 ≤ 100 行**：保持简洁。详细 discussion 放 ADR body 或 PR description。
3. **Status 必填**：4 个状态（Proposed / Accepted / Deprecated / Superseded）。
4. **交叉引用**：每条 ADR 至少 1 个 link（相关 doc / code / issue）。

---

## Phase 2 计划（待 D+21+ 写）

| Decision | Status | 备注 |
|----------|--------|------|
| 0011 polish #6 P1 sprite 重制 | Proposed | polish backlog |
| 0012 TTF 字体升级（Fredoka + Nunito）| Proposed | D+17 TTF prep |
| 0013 i18n LocaleManager.cs 实施 | Proposed | D+19 Jacob |
| 0014 splash B-v3 实施 | Proposed | D+17 build spec |
| 0015 fork guide examples | Proposed | D+18 |

Phase 2 ADR 待 polish 实际实施时补全。

---

## 附录：变更日志

### v0.1 (2026-10-06, Christine D+19)

D+19 周二 ADR index + 10 ADRs:
- 10 个 Phase 1 重要架构决策全部记录
- README.md index with status table + timeline
- 模板 ADR 格式（Michael Nygard style）
- Phase 2 计划占位（5 个待写 ADR）

**Audience**：新加入 agent / 未来 contributor / 学术 reviewer

---

**ADR set 完。** 配合 README + cross-refs 使用。