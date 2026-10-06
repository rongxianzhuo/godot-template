# ADR-0002: godot-template = Match3 产品 + 模板 双身份

> **Status**: ✅ Accepted
> **Date**: 2026-09-29 (D+0)
> **Deciders**: Mark (Leader)

## Context

godot-template repo 同时承担两个角色：
1. **产品**：Magic Match v1.0（match-3 game，要 ship 到 Google Play）
2. **模板**：可 fork 给其他 game developer 用作 starting point

约束：
- 14 天 ship + 模板可 forkability 需同时满足
- 模板身份需要"干净"（无 hardcoded MagicMatch identity），但产品身份需要 ship-ready
- 双重身份需清晰分离，避免 fork 后用户看到 "Magic Match" 字样残留

## Decision

采用 **双身份（dual-identity）** 架构：
- **产品身份**：通过 `scripts/configure.sh` 注入 app identity
- **模板身份**：通过 `FORK_CHECKLIST.md` + `docs/fork-guide-examples.md`（D+18）指导 fork
- **共享部分**：所有 generic infrastructure（toolchain, scripts, FeatureBootstrap）

## Consequences

**Positive**:
- ✅ Magic Match v1.0 可以 ship（产品身份完整）
- ✅ godot-template 仍是 useful fork starting point（模板身份清晰）
- ✅ `configure.sh` 自动化 identity injection（5 min vs 30 min 手动）
- ✅ fork 指南 + checklist 降低 fork 学习曲线
- ✅ 双身份共享同一 codebase（避免 fork 后 divergence）

**Negative**:
- ❌ 配置两层（产品 + 模板）增加 complexity
- ❌ 每次 release 都要 verify 产品身份 + 模板身份都 OK
- ❌ fork checklist 需要 maintain（产品 evolution 时）

**Neutral**:
- 🟢 AGENTS.md / README.md / FORK_CHECKLIST.md 都需 cross-reference
- 🟢 所有 design doc（ART_SPEC / splash / i18n）写给产品，但模板用户也可参考

## Alternatives Considered

### Alternative A: 单身份（只做产品）

- ❌ Rejected：失去 forkability，失去 community contribution 价值
- ❌ Rejected：其他 developer 不愿参考产品 repo（"不是干净的 template"）

### Alternative B: 双 repo（产品 repo + 模板 repo）

- ❌ Rejected：两个 repo 同步维护成本高
- ❌ Rejected：fork 流程需 2 个 step（先 fork 模板，再 cherry-pick 产品改动）

### Alternative C: 模板剥离（产品 release 完转纯模板）

- ❌ Rejected：失去产品案例（template 抽象没有产品 anchor）
- ❌ Rejected：用户 fork 后不知怎么 fill template

## Cross-references

- README.md §"Project structure"
- FORK_CHECKLIST.md（root-level, step-by-step fork 清单）
- docs/fork-guide-examples.md（D+18, concrete walkthrough）
- scripts/configure.sh（自动化 identity injection）
- ADR-0010: v1.0 ship-ready 双身份
