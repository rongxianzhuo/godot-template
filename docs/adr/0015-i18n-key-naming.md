# ADR-0015: i18n Key Naming Convention + Dedup (v1.2 Polish #5)

> **Status**: ✅ Accepted
> **Date**: 2026-10-11 (D+27 周日)
> **Deciders**: Mark + Christine (designer) + Jacob (tooling integration)

## Context

v1.1 i18n 实施（D+13 Christine + D+13.5 Jacob Tr() migration）有以下问题：

### Issue 1: msgid 形式不一致（v1.1 caveat #1 + v1.2 ADR-0013）

`extract_i18n_strings.py`（D+13）扫描结果混用 2 种 msgid：
- **i18n key** 作 msgid（早期版本）：`msgid "title.play"` → "Play"
- **English text** 作 msgid（v1.1 修订）：`msgid "Play"` → "Play"

混淆：extract script 的 dedup 逻辑 + tcomment 信息冗余。

### Issue 2: v1.1 caveat #2 — `hud.score` / `hud.score_2` 冲突

`hud.score_2` 不是 PO dedup 冲突（msgid 不同），而是 i18n key naming conflict：
- 同一段代码 2 个 score-related string → 一个建议 `hud.score`，另一个 append `_2`
- 实际语义不同（"Score: {0}" vs "Score: {0} — {1} of {2} moves used"），应该不同 key
- 但 `_2` suffix 不传达语义

### Issue 3: v1.2+ 扩展 conflict

v1.2 polish #3 splash subtitle + #4 settings.title/settings.language 加 4 个新 string。如果沿用 v1.1 逻辑，可能产生 `splash.subtitle_2` 或 `settings.title_2` 等无意义 suffix。

约束：
- 必须与 v1.1 PO files 兼容（不要重新翻译已有 9 个 string）
- 必须 support v1.2+ 50-70 string 扩展
- 不应该 require 手动 rename 每次 extract

## Decision

采用 **msgid = canonical English text + smart key generation** 模式：

### Rule 1: msgid 标准化

**msgid** = English text（canonical, includes placeholders as `{0}`, `{1}`, etc.）
**tcomment** = `i18n key: <key>`（informational only）

Example:
```
# i18n key: title.play
msgid "Play"
msgstr "Jugar"
```

### Rule 2: Smart key generation (no auto-_2)

新 dedup 逻辑：
1. Group entries by **exact** `(msgid, placeholder_count)` pair
2. Same pair → merge occurrences (file:line append)
3. Different pair → DIFFERENT keys (no auto-suffix)
4. Truly identical msgid appearing multiple times → suffix `_v2`, `_v3` (file context)

```python
def dedup_entries(entries: list) -> dict:
    """v1.2 polish #5: group by msgid + placeholder count, not just key."""
    by_signature = {}  # (msgid, placeholder_count) -> list[entries]
    for entry in entries:
        sig = (entry['msgid'], entry.get('placeholders', 0))
        by_signature.setdefault(sig, []).append(entry)
    
    by_key = {}
    for sig, entries_list in by_signature.items():
        msgid, ph_count = sig
        key = suggest_key(msgid, Path(entries_list[0]['file']))
        
        if len(entries_list) == 1:
            by_key[key] = entries_list[0]
        else:
            # Multiple occurrences of truly identical msgid (rare)
            # Use _v2, _v3 suffix for traceability
            for i, entry in enumerate(entries_list, start=1):
                if i == 1:
                    by_key[key] = entry
                else:
                    by_key[f'{key}_v{i}'] = entry
    return by_key
```

### Rule 3: Per-feature file:line traceability

每个 PO entry tcomment 加 file:line source:
```
#. Text assignment
#: src/Features/Match3/Screens/EndScreen.cs:102
# i18n key: endscreen.you_won
msgid "You Won!"
msgstr "¡Has ganado!"
```

### Rule 4: `suggest_key()` 增强

新 heuristic:
1. Specific keywords (existing, D+13) — keep
2. Add: 多 scene name → 用 filename 作为 namespace
3. Add: placeholder count suffix if generic (e.g., `hud.score_simple`, `hud.score_detailed`)

## Consequences

**Positive**:
- ✅ msgid 单一 canonical 形式（English text）
- ✅ 0 `_2` 无意义 suffix（v1.2+ 无 hud.score_2 类似冲突）
- ✅ PO dedup by msgid 自动准确
- ✅ tcomment i18n key 仍可读（人类维护）
- ✅ Extract script 自包含 dedup 逻辑（无需手动）

**Negative**:
- ❌ v1.1 PO files 需要小调整（re-extract + re-translate affected keys）
- ❌ `hud.score_2` 需要 rename（语义：Score: {0} — {1} of {2} moves used）
- ❌ Suggest_key heuristic 加 complexity

**Neutral**:
- 🟢 v1.1 已有 9 个 string 不变（除 hud.score_2）
- 🟢 Weblate 等工具兼容（msgid is canonical）
- 🟢 Godot TranslationServer 不依赖 i18n key

## Alternatives Considered

### Alternative A: msgid = i18n key, tcomment = English (Christine v1.1 旧)

- ❌ Rejected：tooling 难友好（Weblate 编辑 i18n key 比 English text 难）
- ❌ Rejected：grep "Play" 找不到（msgid 是 "title.play"）

### Alternative B: msgid = hash + tcomment = i18n key

- ❌ Rejected：hash 不友好（人类 review 难）
- ❌ Rejected：diff 难以跟踪

### Alternative C: 完全自动化（AI suggest keys）

- ❌ Rejected for v1.2：scope 大（需要训练 AI on Magic Match corpus）
- ✅ Deferred to v2.0（AI-assisted i18n management）

### Alternative E: 保留 v1.1 logic + 手动 rename 每次

- ❌ Rejected：手动错误 + 时间成本
- ❌ Rejected：v1.2+ string 扩展 conflict 风险

## Cross-references

- `scripts/extract_i18n_strings.py` (D+13, enhanced D+27 — new dedup + smart key)
- `locale/en.po` (re-extract D+27, +1 cleanup: hud.score_2 → hud.score_detailed)
- `locale/es.po` (re-translate 1 affected entry)
- `locale/pt-BR.po` (re-translate 1 affected entry)
- `docs/design/i18n-strategy.md` §1.2 (D+12 strategy)
- `docs/adr/0009-i18n-strategy.md` (v1.1 3 langs baseline)
- `docs/adr/0013-splash-i18n.md` (parallel ADR, splash i18n)
- `docs/adr/0014-language-picker.md` (parallel ADR, language picker)
- `docs/RELEASE_NOTES_v1.1.md` §3 caveat #2 (hud.score_2 conflict)

## Future Work

### v1.3+: scan fallback styles

- Current script 提取 4 patterns (Text / Title/Hint/MakeButton / Tr / .tscn text)
- v1.3+ 加 TooltipText, HelpTooltip, etc.
- 减少 manual re-extract

### v2.0: msgid + i18n key 双向 lookup table

- Generate `locale/i18n_keys.json` mapping (msgid -> key)
- Tools 可双向查找 (i18n key → msgid for code review, msgid → key for new strings)

### v2.0: AI-assisted i18n key suggestion

- Train AI on Magic Match + similar games' i18n keys
- Suggest keys at extract time based on context (file, surrounding code)
- Human review + edit before merging