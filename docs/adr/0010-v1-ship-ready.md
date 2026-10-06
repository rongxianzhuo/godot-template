# ADR-0010: v1.0 ship-ready 双身份 (产品 + 模板)

> **Status**: ✅ Accepted
> **Date**: 2026-10-06 (D+7)
> **Deciders**: Mark (Leader)

## Context

D+9 v1.0 ship deadline 临近。godot-template 必须同时满足：
1. **产品身份 ship-ready**：Magic Match v1.0 可在 Android 真机跑
2. **模板身份可 fork**：fork 后能配置成其他 game

约束：
- v1.0 release 是 first public release（Google Play 上架）
- 模板用户第一个 fork 决定 template adoption
- 双重身份需清晰 documentation（避免 "我 fork 了但不知道怎么用"）

## Decision

v1.0 ship 接受 **双身份** 架构：
- **产品身份**：Magic Match v1.0 — 30 美术资产 + ART_SPEC + release notes + Match3Feature
- **模板身份**：godot-template — configure.sh + FORK_CHECKLIST.md + docs/fork-guide-examples.md + AGENTS.md

**Documentation surface**（v1.0 ship）：
| Layer | Path | Audience |
|-------|------|----------|
| 产品 brand | README.md (English + 中文) | End users / first-time visitors |
| Fork guide | FORK_CHECKLIST.md | New fork users (step-by-step) |
| Fork examples | docs/fork-guide-examples.md (D+18) | New fork users (walkthrough) |
| Agent contract | AGENTS.md | AI agents working in this repo |
| Asset spec | docs/ART_SPEC.md | Artists + designers |
| Asset integration | docs/ASSET_INTEGRATION.md | Engineers integrating assets |
| Release notes | docs/RELEASE_NOTES_v1.0.md | End users (Google Play listing) |
| Architecture | docs/adr/ (D+19, 10 ADRs) | Architects + future contributors |

## Consequences

**Positive**:
- ✅ v1.0 双身份完整 ship（产品可玩 + 模板可 fork）
- ✅ Documentation 完整（8 layer docs 覆盖所有 user types）
- ✅ Fork user 5 min configure.sh + 5 min 删除 demo = 10 min 总配置时间
- ✅ Google Play 上架所需 metadata 完整（release notes 4 字段 + 8 known issues）
- ✅ Agent working contract 清晰（AGENTS.md 4 sections）

**Negative**:
- ❌ Documentation maintenance burden（每次 release 都需 update）
- ❌ Cross-reference 复杂（README ↔ FORK_CHECKLIST ↔ fork-guide-examples ↔ AGENTS.md）
- ❌ Dual-identity 测试需要 verify 两边都 OK（每次 release）

**Neutral**:
- 🟢 产品 + 模板共享同一 codebase（避免 fork 后 divergence）
- 🟢 MagicStudio 4 AI agents（Jacob / Francisco / Austin / Christine）分工明确

## Alternatives Considered

### Alternative A: v1.0 ship 时删除 fork 文档（保留产品身份 only）

- ❌ Rejected：失去 template adoption 价值
- ❌ Rejected：fork 用户无法快速 onboard
- ❌ Rejected：README "ready-to-fork" 承诺无法兑现

### Alternative B: v1.0 ship 时延后 fork 文档到 v1.1

- ❌ Rejected：fork 用户 onboarding 延迟 = 失去 first impressions
- ❌ Rejected：v1.0 上线同时就要 receive fork traffic

### Alternative C: v1.0 ship 完整 fork 文档 + sample fork repo

- ❌ Rejected：sample fork repo 增加 maintenance burden
- ❌ Rejected：v1.0 时间紧，sample repo 不是 ship-blocker

## Cross-references

- README.md §"Project structure"（双身份 surface）
- FORK_CHECKLIST.md（step-by-step fork 清单）
- docs/fork-guide-examples.md（D+18, walkthrough）
- AGENTS.md（agent working contract）
- docs/ART_SPEC.md（产品 + 模板共享 asset spec）
- docs/RELEASE_NOTES_v1.0.md（D+10, Google Play listing）
- docs/adr/README.md（D+19, 10 ADRs）
- ADR-0002: fork 双身份（initial decision）
