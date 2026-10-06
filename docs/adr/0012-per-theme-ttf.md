# ADR-0012: Per-Theme TTF Overrides (v1.2 Phase 1 — Weight Remap)

> **Status**: ✅ Accepted
> **Date**: 2026-10-08 (D+23)
> **Deciders**: Mark (Leader) + Christine (Artist)

## Context

v1.1 ship 时所有主题（Forest / Desert / Ocean）共用同一套字体（Fredoka + Nunito 5 weights），没有 per-theme 字体差异化。3 个主题（Forest 1-20 / Desert 21-40 / Ocean 41-60）虽然 color palette 不同，但 typography 一致，缺少视觉差异化。

约束：
- v1.2 polish cycle 9 天（D+22-D+31 ship），不能引入大字体变化
- 必须复用 v1.1 已 subset 的 5 字体（避免重新 download + subset）
- ThemeBuilder.cs（D+18 Jacob 已 done）需要扩展 `GetThemeFonts(theme)` 方法
- Bundle 预算 12 MB，v1.1 已用 8.42 MB，留 ~3.5 MB 空间

## Decision

采用 **Per-Theme TTF Overrides** v1.2 phase 1（**weight remap**）：

| Theme | Primary (display) | Secondary (body) |
|-------|-------------------|------------------|
| Forest | Fredoka Bold (700) | Nunito Regular (400) |
| Desert | Fredoka SemiBold (600) | Nunito Medium (500) |
| Ocean | Fredoka Bold (700) | Nunito Bold (700) |

**Rationale**：
- **v1.2 phase 1**：per-theme infrastructure（weight remap, 0 new fonts）
- **v1.3+ phase 2**：per-theme **families**（Forest=Lora serif, Desert=Pacifico italic, Ocean=Fredoka display）+80 KB

## Consequences

**Positive**:
- ✅ 3 主题 typography 差异化（weight 变化, Bold vs SemiBold vs Bold + Bold body）
- ✅ 0 new fonts（reuse v1.1 已 subset 5 fonts）
- ✅ Bundle delta 仅 +33 KB（Fredoka-Bold × 2 duplication）
- ✅ ThemeBuilder.cs `GetThemeFonts(theme)` 基础设施 ready
- ✅ v1.3+ phase 2 扩展空间预留（replace FontPair with new family）

**Negative**:
- ❌ Visual differentiation 仍 subtle（weight 变化 < family 变化）
- ❌ Fredoka-Bold × 2 duplication（Forest + Ocean share same file）
- ❌ Phase 1 + phase 2 是 2 个 polish cycles（不是 1 个）

**Neutral**:
- 🟢 Per-theme colors 仍 ThemeColors.cs 控制（D+3 已 done）
- 🟢 Per-theme font 与 colors 解耦（容易 future 修改）

## Alternatives Considered

### Alternative A: per-theme font families (full spec compliance)

- ❌ Rejected for v1.2：scope 大（download Lora + Pacifico + subset）
- ❌ Rejected for v1.2：bundle +80 KB（vs Mark D+23 拍板 +33 KB）
- ✅ **Deferred to v1.3+ phase 2**（per ADR appendix）

### Alternative B: no per-theme typography（uniform across themes）

- ❌ Rejected：失去 v1.2 polish #2 scope（per Mark D+22 spec）
- ❌ Rejected：3 主题 visual consistency > homorphic

### Alternative C: per-theme CSS-style override file

- ❌ Rejected：增加 complexity（每个 theme 一个 .json config）
- ❌ Rejected：运行时 parsing overhead

### Alternative D: dynamic font weights（runtime resolution）

- ❌ Rejected：runtime font loading 慢（200-500ms per font）
- ❌ Rejected：增加 complexity（vs static assets）

## Decision Rationale (per Mark D+23)

Mark D+23 message override v1.2 spec §2：
> "per-theme TTF subsets: Forest=Fredoka Bold+Nunito Regular; Desert=Fredoka SemiBold+Nunito Medium; Ocean=Fredoka Bold+Nunito Bold"

vs v1.2 spec §2 original 写:
> "Per-theme differentiation: Forest = serif; Desert = italic; Ocean = sans"

**Mark's rationale (inferred)**：
- 0 new fonts → fastest implementation
- 0 new bundle cost → ship-friendly
- 5 existing fonts already subset → reuse tooling
- per-theme weight 给更多文化差异化 ✓

## Cross-references

- `docs/design/v1.2-polish-backlog-spec.md` §"Per-theme TTF overrides"（source spec）
- `docs/design/font-per-theme-spec.md` (D+23 Christine handoff spec)
- `src/Core/ThemeBuilder.cs` (Jacob D+18 done, extended D+24-D+25)
- `docs/adr/0009-i18n-strategy.md` (related: 3 langs same Latin chars)
- `docs/design/font-integration-spec.md` (D+17 v1.1 font subset spec)
- Mark D+23 per-theme weight 拍板

## Future Work

### v1.3+ phase 2: per-theme font families

**Trigger**: Founder Google Play 反馈 "3 themes look similar" OR v1.2 ship 后 1-2 周

**Implementation**:
- Download new fonts:
  - Lora Regular + Bold (forest, 50 KB subset)
  - Pacifico Regular (desert, 30 KB subset)
- Update ThemeBuilder.cs:
  - Forest → Lora family (serif aesthetic)
  - Desert → Pacifico family (italic aesthetic)
  - Ocean → Fredoka family (display, unchanged)
- Bundle delta: +80 KB

**Rationale**: True per-theme aesthetic differentiation (serif vs italic vs display)

### v2.0+: full theme redesign

- Each theme gets unique font family + colors + UI elements
- Bundle delta: +150 KB