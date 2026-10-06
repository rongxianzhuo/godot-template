# ADR-0013: Splash Screen i18n (v1.2 Polish #3)

> **Status**: ✅ Accepted
> **Date**: 2026-10-09 (D+25 周五)
> **Deciders**: Mark + Christine

## Context

v1.1 i18n 实施（D+13）cover 了 GameScenes 9 个 strings（title.play / title.quit / endscreen.* / hud.score / hud.moves / brand.magic_match）。但 splash screen "Magic Match" title + "60 Levels · 3 Themes" subtitle **仍是 hardcoded English**（per v1.1 release notes §3 known issue caveat #1）。

Splash 是 user first-impression point：
- 用户启动 app 第一眼 → 决定是否继续玩
- v1.1 + 3 languages 时 Latin American + Brazilian 用户看 English splash = 文化隔阂

约束：
- v1.1 splash scene 已在 `assets/scenes/SplashScreen.tscn`（per Mark D+17 .tscn exception）
- 9 string v1.1 + 1 new splash key = 10 keys total
- 必须保持 en/es/pt-BR 三语一致 + brand name "Magic Match" 不翻译

## Decision

采用 **splash subtitle i18n extraction**：
- 提取 splash subtitle 1 string 到 i18n key `splash.subtitle`
- splash title "Magic Match" 复用 `brand.magic_match`（已有 v1.1 key，0 duplicate）
- 扩展 `scripts/extract_i18n_strings.py` 扫描 `.tscn` 文件（v1.1 caveat #1 修复）
- 3 语言 PO filled（en baseline + es "60 Niveles · 3 Mundos" + pt-BR "60 Níveis · 3 Mundos"）

### Translations（per Christine D+13 DeepL Pro 经验 + D+25 Mark 拍板）

```
i18n key: splash.subtitle
msgid (en):     "60 Levels · 3 Themes"
msgstr (es):    "60 Niveles · 3 Mundos"
msgstr (pt-BR): "60 Níveis · 3 Mundos"
```

### Extract script upgrade（v1.2 polish #5 部分实施）

`.tscn` 文件格式：
```
[node name="SplashSubtitle" type="Label" parent="."]
text = "60 Levels · 3 Themes"
```

Extract script 需要：
1. Parse `.tscn` XML/Godot text format
3. Match `text = "..."` pattern（per D+13 .Text = extractor）
4. Generate i18n key from label name + text content

## Consequences

**Positive**:
- ✅ Splash first-impression i18n 一致（en/es/pt-BR 一致）
- ✅ Extract script 升级支持 .tscn（future-proof, 其他 scene 也 i18n-ready）
- ✅ "Magic Match" brand name 通过 `brand.magic_match` 复用（0 key collision）
- ✅ PO files 100% translated (11 entries each)

**Negative**:
- ❌ .tscn 解析复杂度（vs plain .cs grep）
- ❌ .tscn label Text assignment 不是 `Text = "..."` 而是 `text = "..."`（lowercase）
- ❌ Future .tscn label changes 需要 re-extract

**Neutral**:
- 🟢 Splash title 用 brand.magic_match（splash.title is alias）
- 🟢 "Mundos" (worlds) vs "Temas" (themes) choice：Mark D+25 拍板 "Mundos"（更 evocative）

## Alternatives Considered

### Alternative A: 不翻译 splash（保留 hardcoded English）

- ❌ Rejected：v1.1 i18n 失去 first-impression 一致性
- ❌ Rejected：Latin American + Brazilian 用户看 English splash = 文化隔阂

### Alternative B: 完整 splash i18n（title + subtitle + loading text + version）

- ❌ Rejected：splash title 已有 brand.magic_match，无需 duplicate
- ❌ Rejected：loading text 是 transient（用户几乎看不到）
- ❌ Rejected：version 是 metadata 不是 UI

### Alternative C: per-theme splash subtitle（Forest=serif themes / Desert=italic / Ocean=display）

- ❌ Rejected for v1.2：per-theme font 推迟 v1.3+ phase 2（per ADR-0012 §Future）
- ❌ Rejected for v1.2：scope 大 + bundle +80 KB

## Cross-references

- `locale/en.po` (D+25 +1 entry splash.subtitle)
- `locale/es.po` (D+25 +1 entry filled)
- `locale/pt-BR.po` (D+25 +1 entry filled)
- `scripts/extract_i18n_strings.py` (D+13, D+25 扩展 .tscn scan)
- `assets/scenes/SplashScreen.tscn` (Mark D+17 .tscn exception, splash subtitle = "60 Levels · 3 Themes")
- `docs/design/i18n-strategy.md` (D+12 strategy doc)
- `docs/adr/0009-i18n-strategy.md` (v1.1 3 langs baseline)
- `docs/RELEASE_NOTES_v1.1.md` §3 caveat #1 (known issue)

## Future Work

### v1.2 polish #5: i18n key naming dedup (D+28 Christine)
- 解决 v1.1 PO files 的 `hud.score` / `hud.score_2` conflict
- 升级 extract script 加 file:line context

### v1.3+ phase 2: per-theme splash subtitle
- Per-theme font: Forest=serif, Desert=italic, Ocean=display (per ADR-0012 §Future)
- Splash subtitle 字体随 theme 切换
- Bundle delta: +80 KB

### v2.0: full i18n (8 languages)
- en + es + pt-BR + fr + de + it + ja + zh-CN
- 200+ strings, 8 PO files
- CJK fonts (Noto Sans SC/JP/KR subset ~2.7 MB)