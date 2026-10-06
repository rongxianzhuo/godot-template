# ADR-0009: i18n strategy (v1.1 3 langs / v2.0 8 langs)

> **Status**: ✅ Accepted
> **Date**: 2026-10-06 (D+7)
> **Deciders**: Mark + Christine

## Context

Match-3 game 需要国际化：
- v1.0 ship：English only（per Mark D+10 decision）
- v1.1 ship：3 languages（en + es + pt-BR）—— 西方主要 casual mobile market
- v2.0 ship：8 languages（en + es + pt-BR + fr + de + it + ja + zh-CN）—— 全球 casual mobile market

约束：
- Godot 4.7 有 built-in `TranslationServer` + PO file 支持
- 但 9 string baseline（v1.0）+ 50-70 string（v1.1）+ 200+ string（v2.0）translation 成本高
- CJK 字体（Noto Sans SC/JP/KR）subset ~2.7 MB（大）
- 必须 support Latin（es / pt-BR / fr / de / it）+ CJK（zh-CN / ja）字符

## Decision

采用 **分层 i18n 策略**：

| Phase | Languages | Strings | Fonts | Bundle +size |
|-------|-----------|---------|-------|--------------|
| **v1.0 ship** | en only | 9 (hardcoded) | Godot default | 0 KB |
| **v1.1 ship** | en + es + pt-BR | 50-70 (PO files) | Fredoka + Nunito (Latin only) | +120 KB |
| **v2.0 ship** | 8 langs + CJK | 200+ (PO files) | + Noto Sans SC/JP/KR | +2.7 MB |

**Translation workflow**（v1.1+）：
- **Tool**：Weblate hosted free OSS + GitHub Actions CI auto-extract
- **Translation source**：DeepL Pro + human review
- **Format**：PO files (`locale/*.po`) + polib Python tooling

**Bundle size optimization**（v2.0）：
- Per-language subset (`cmap` filtering)
- Android App Bundle per-language split（节省 19-25%）

## Consequences

**Positive**:
- ✅ v1.1 3 languages 涵盖西方主要 casual market（Latin American Spanish + Brazilian Portuguese 是最大 2 个海外市场）
- ✅ Weblate hosted free OSS（per Mark D+12 decision）—— 零翻译工具成本
- ✅ DeepL Pro + human review $75 预算（per Mark D+12 approval）—— 翻译质量 industry-standard
- ✅ pyftsubset Latin（95 KB）+ CJK（2.7 MB）subset 字符集合理
- ✅ Android App Bundle per-language split 节省 19-25% 包大小

**Negative**:
- ❌ v1.1 PO files 需要 human translator（DeepL Pro 不够，需要 review）
- ❌ CJK 字体 +2.7 MB（v2.0 才有）—— 必须 App Bundle split 节省
- ❌ Weblate 需要 GitHub repo integration（setup 步骤）
- ❌ Translation 持续维护（每个新 string 需要翻译）

**Neutral**:
- 🟢 "Magic Match" brand name 保留（不翻译）
- 🟢 "Game Over" 西语保留英文（casual mobile game 标准）
- 🟢 Google Play 商店文案 v1.0 English only，v1.1+ 加翻译

## Alternatives Considered

### Alternative A: v1.0 直接 ship 3 languages

- ❌ Rejected：v1.0 时间压力（14 天 ship）
- ❌ Rejected：i18n 需要 LocaleManager.cs 实施（增加 v1.0 risk）
- ❌ Rejected：DeepL Pro + human review 时间不够

### Alternative B: v2.0 直接 ship 8 languages + CJK

- ❌ Rejected：v2.0 包大小爆炸（+2.7 MB CJK fonts + 8 PO files）
- ❌ Rejected：translation cost 过高（8 langs × 200 strings = 1600 translations）
- ❌ Rejected：v1.1 + v2.0 拆分 = 渐进式 ship（先验证西方市场）

### Alternative C: 用 Google Translate API（machine-only）

- ❌ Rejected：translation quality 不够（machine-only 翻 casual game 显得不专业）
- ❌ Rejected：Google Play 评论会投诉翻译质量
- ❌ Rejected：DeepL Pro 质量明显更好（西班牙语 + 葡萄牙语）

### Alternative D: Fork 用户翻译（community-driven）

- ❌ Rejected：casual game 上线后等 community 翻译 = 数月延迟
- ❌ Rejected：launch day 翻译缺失 = 流失海外用户

## Cross-references

- `docs/design/i18n-strategy.md`（完整 v1.1/v2.0 拆分 + 工作流）
- `scripts/extract_i18n_strings.py`（i18n string extraction tool, D+13）
- `locale/en.po` / `locale/es.po` / `locale/pt-BR.po`（v1.1 baseline）
- `docs/design/ttf-font-research.md`（Fredoka + Nunito Latin subset）
- `i18n/v1.1-translations` branch（D+13 DeepL Pro translations）
- `tools/i18n-extract-script` branch（D+13 extraction script）
- Mark D+12 决策（4 项：3 langs + Weblate + $75 + +120 KB）
