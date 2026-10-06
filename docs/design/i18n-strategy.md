# i18n Strategy — v1.1 + v2.0 Implementation Plan

> **作者**：Christine (Artist) + Mark (decision)
> **Date**：2026-10-06 (D+12 周五)
> **状态**：v2.0 prep（v1.1 ship i18n infrastructure；v2.0 full multi-language）
> **关联**：TTF font research (`docs/design/ttf-font-research.md`)

---

## 0. TL;DR

| 决策 | 值 |
|------|-----|
| **v1.0 baseline** | English only, hardcoded strings |
| **v1.1 minimum i18n** | English (default) + 西班牙语 (es) + 葡萄牙语巴西 (pt-BR) = 3 语言 |
| **v1.1 翻译策略** | Godot PO 文件 + GitHub Actions 自动化 + Weblate 社区翻译（推荐 free OSS） |
| **v1.1 字体** | Latin only（Fredoka + Nunito subset，per TTF research） |
| **v2.0 full i18n** | + 中文简体 (zh-CN) + 日文 (ja) + 韩文 (ko) + 德语 (de) + 法语 (fr) = 8 语言 |
| **v2.0 字体** | + Noto Sans SC / Noto Sans JP / Noto Sans KR subset（per language） |
| **打包策略** | Android App Bundle (`.aab`) with per-language split，节省 ~30-50% 下载大小 |
| **实施工作量** | v1.1 基础设施 0.5 天 + 1 语言翻译 0.3 天；v2.0 完整 +5 语言 + CJK 字体 1.5-2 周 |
| **包大小影响** | v1.1 +~120 KB (5 fonts 95 KB + PO files 25 KB)；v2.0 +~1.5 MB (CJK fonts + 翻译) |

---

## 1. v1.1 vs v2.0 文案拆分（哪些 hardcode / 哪些 extract）

### 1.1 文案分类（v1.0 状态 vs v1.1 + v2.0）

| 类别 | 示例 | v1.0 | v1.1 extract | v2.0 extract | i18n key 命名建议 |
|------|------|------|--------------|--------------|----------|
| **Splash title** | "Magic Match" | hardcode | ✅ | ✅ | `splash.title` |
| **Splash subtitle** | "60 Levels · 3 Themes" | hardcode | ✅ | ✅ | `splash.subtitle` |
| **Title screen** | "Play" / "Continue" / "Settings" | hardcode | ✅ | ✅ | `title.play` / `title.continue` / `title.settings` |
| **Level select** | "Level {0}" / "Forest" / "Desert" / "Ocean" | hardcode | ✅ | ✅ | `level.select.label` / `theme.forest` / `theme.desert` / `theme.ocean` |
| **HUD** | "Score" / "Moves" / "Time" | hardcode | ✅ | ✅ | `hud.score` / `hud.moves` / `hud.time` |
| **End screen win** | "You Win!" / "Score: {0}" / "Stars: {0}/3" | hardcode | ✅ | ✅ | `endscreen.win.title` / `endscreen.score` / `endscreen.stars` |
| **End screen lose** | "Out of Moves" / "Try Again" | hardcode | ✅ | ✅ | `endscreen.lose.title` / `endscreen.retry` |
| **Lives system** | "Out of Lives" / "Watch Ad for Heart" | hardcode | ✅ | ✅ | `lives.empty` / `lives.refill_ad` |
| **Settings** | "Music" / "Sound" / "Language" | hardcode | ✅ | ✅ | `settings.music` / `settings.sound` / `settings.language` |
| **AdMob CTA** | "Continue" / "Watch Video" | hardcode | ✅ | ✅ | `ad.continue` / `ad.watch` |
| **Theme names** | "Forest" / "Desert" / "Ocean" | hardcode | ✅ | ✅ | `theme.forest` / `theme.desert` / `theme.ocean` |
| **Theme banner** | "Welcome to {Theme}" | hardcode | ✅ | ✅ | `themebanner.welcome` |
| **Level intros** | "Match 3 gems!" / "Reach 1000 points!" | hardcode | ✅ | ✅ | `level.{N}.intro` |
| **Error messages** | "No internet" / "Try again" | hardcode | ✅ | ✅ | `error.no_internet` / `error.retry` |
| **GameScenes 内 log** | internal log messages | hardcode | ❌ 不 extract | ❌ | (developer only) |
| **Asset filenames** | `gem_purple.png` / `bg_title.png` | hardcode | ❌ 不 extract | ❌ | (filesystem, not user-facing) |
| **数字常量** | Score 1000 / Moves 20 | hardcode | ❌ 不 extract | ❌ | (game logic) |
| **Brand name** | "Magic Match" | hardcode | ⚠️ 部分 | ⚠️ 部分 | `brand.name` (extract for UI, not for splash title — brand identity 跨语言) |

### 1.2 i18n key 命名规范

**规则**：
- lowercase + dot-separated (Godot 4 tr() 推荐)
- screen.subtype.field
- 不带空格或特殊字符
- 不带变体 / 上下文后缀（用 plural forms 而非 key variant）

**示例**：
```csharp
// ✅ good
GetNode<Label>("Title").Text = Tr("title.play");
GetNode<Label>("Score").Text = Tr("hud.score");
Tr("endscreen.stars", starsEarned);  // 占位符 {0}

// ❌ bad
Tr("PLAY_BUTTON");
Tr("titlePlay");
Tr("title.play.button.label");
```

### 1.3 占位符 + plural forms

**占位符**（用 `{0}` `{1}` `{2}`）：
```po
# en.po
msgid "endscreen.score"
msgstr "Score: {0}"

# zh-CN.po
msgid "endscreen.score"
msgstr "得分: {0}"

# ja.po
msgid "endscreen.score"
msgstr "スコア: {0}"
```

**Plural forms**（PO 标准）：
```po
# en.po
msgid "endscreen.stars"
msgid_plural "endscreen.stars_plural"
msgstr[0] "{0} star"
msgstr[1] "{0} stars"

# zh-CN.po (no plural)
msgid "endscreen.stars"
msgstr "{0} 颗星"

# ja.po (no plural)
msgid "endscreen.stars"
msgstr "{0} 個の星"
```

**Godot tr() 调用**：
```csharp
// 单数
Tr("endscreen.stars", count);
// plural
TrPlural("endscreen.stars", "{0} stars", count);
```

### 1.4 v1.1 必须 extract 的 strings

按 §1.1 表格，v1.1 extract 所有 ✅ 类目。预计 **~50-70 strings**。

PO 文件预期大小：
- en.po baseline: ~10 KB
- es.po + pt-BR.po: each ~10-15 KB（含 plural forms 扩展）
- 总计: ~40 KB（v1.1 3 语言 PO files）

---

## 2. 翻译工作流

### 2.1 三种工作流对比

| 方式 | 优点 | 缺点 | 适合 |
|------|------|------|------|
| **GitHub PR-based** | 无第三方依赖 / 简单 / 透明 | 翻译者需 git 技能 / 难 review | 1-2 语言 + 小团队 |
| **Weblate (hosted)** | Web UI / 翻译者无 git 技能 / 自动 commit / glossary / quality check | 免费 tier 限制 1 语言 | 5+ 语言 + 社区翻译 ⭐ **推荐** |
| **Crowdin (SaaS)** | 多平台同步 / 截图上下文 / translator marketplace / TM | 收费 / 适合大项目 | 10+ 语言 + 大型 release |

**推荐**：**Weblate hosted** (free for OSS, https://weblate.org) for Magic Match v1.1 3 语言 → v2.0 8 语言。

### 2.2 v1.1 Weblate workflow

```
1. Christine 创建 weblate 项目 (magic-match)
2. Mark 添加 3 语言 component: en (source) / es / pt-BR
3. Weblate 自动从 GitHub pull locale/en.po
4. 翻译者 (内部 + 社区) 在 Weblate UI 翻译
5. Weblate 自动 push translation commit 到 GitHub PR
6. CI (GitHub Actions) 自动 validate .po files
7. Christine + Mark merge PR
8. en.po master 永远在 main branch
```

### 2.3 v1.1 翻译源策略

**首批翻译**（v1.1 ship）：
- 🇺🇸 **English (en)** — 默认 baseline（已写）
- 🇪🇸 **Spanish (es)** — 美国 2nd language / Latin America 大市场
- 🇧🇷 **Portuguese-Brazil (pt-BR)** — Brazil mobile market 大

**翻译者**：
- **es**: Mark 推荐专业 translator OR DeepL Pro + human review（$0.05/word × ~50 strings × ~5 words = ~$12 total）
- **pt-BR**: 同上

**v1.1 不做**：
- ❌ 中文 / 日文 / 韩文（CJK v2.0）
- ❌ 德语 / 法语 / 意大利语（v2.0）
- ❌ 阿拉伯语 / 印地语（right-to-left + Indic script 复杂，v2.0+）

### 2.4 自动化 CI（GitHub Actions）

**Workflow 1**: extract_strings.yml（on PR）
```yaml
name: Extract Strings
on: [pull_request]
jobs:
  extract:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Install godot
        run: |
          wget https://github.com/godotengine/godot-builds/releases/download/4.7.2-stable/Godot_v4.7.2-stable_linux.x86_64.zip
          unzip Godot_v4.7.2-stable_linux.x86_64.zip
      - name: Extract translatable strings
        run: |
          ./Godot_v4.7.2-stable_linux.x86_64 --headless --quit-after 1 || true
          # 扫描 src/ 所有 .cs 文件中的 Tr("...") 调用
          python scripts/extract_tr_strings.py src/ > locale/en.po
      - name: Check diff
        run: |
          if [[ $(git diff locale/en.po | wc -l) -gt 10 ]]; then
            echo "::warning::locale/en.po changed — please review new strings"
            git diff locale/en.po
          fi
```

**Workflow 2**: validate_translations.yml（on push）
```yaml
name: Validate Translations
on: [push]
jobs:
  validate:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        lang: [en, es, pt-BR]
    steps:
      - uses: actions/checkout@v4
      - name: Validate .po file
        run: |
          # 检查 msgfmt / msgmerge
          msgfmt --check locale/${{ matrix.lang }}.po -o /dev/null
          # 检查所有 msgid 在 en.po 都存在
          for lang in es pt-BR; do
            python scripts/validate_po.py locale/${{ matrix.lang }}.po locale/en.po
          done
```

### 2.5 翻译质量保证

**3-tier review process**：
1. **自动化 check** (CI) — msgfmt + msgmerge + completeness (100% msgid coverage)
2. **Translator self-review** — 翻译者提交前 review 自己的 work
3. **Native speaker review** — Mark + 团队中 native speaker 抽查 10-20% strings

**质量指标**：
- 100% msgid coverage (no missing translations)
- 90%+ glossary compliance (use app's terminology)
- 0 truncation / overflow issues（字号 + 容器测试）

---

## 3. 字体集成（CJK Noto Sans SC + Subset + 包大小）

### 3.1 v1.1 字体决策（per TTF research）

v1.1 字体 = Fredoka Bold + Nunito Regular/Medium/Bold + Nunito SemiBold（5 fonts subset ~95 KB）。

Latin + Latin Extended-A only。**CJK v1.1 不做**。

### 3.2 v2.0 CJK 字体集成策略

**字体选择**：Google Noto Sans 系列（与 Fredoka / Nunito OFL 一致）：
- 🇨🇳 中文简体：Noto Sans SC（Source Han Sans CN）
- 🇯🇵 日文：Noto Sans JP
- 🇰🇷 韩文：Noto Sans KR
- 🌏 CJK 统一：Noto Sans CJK SC（包含简体 + 日文 + 韩文 + 繁体）

**字符子集策略**（per language）：
```bash
# v2.0 简体中文 (zh-CN)
pyftsubset NotoSansSC-Regular.otf \
  --unicodes="U+0020-007E,U+00A0-00FF,U+0100-017F,U+3000-303F,U+4E00-9FFF" \
  --text-file=scripts/chinese-common-3500-chars.txt \
  --output-file=NotoSansSC-Regular-subset.otf

# text-file: 3500 常用简体汉字 list (from 现代汉语常用字表)
```

**CJK 子集大小估算**：

| 字体 | 子集字符数 | 子集大小 |
|------|------------|----------|
| Noto Sans SC Regular | 3500 chars | ~700 KB |
| Noto Sans SC Bold | 3500 chars | ~700 KB |
| Noto Sans JP Regular | 3500 chars | ~700 KB |
| Noto Sans KR Regular | 3500 chars | ~600 KB |
| **CJK 总计** | 3 languages | **~2.7 MB** |

**v2.0 包大小影响**：
- v1.1 baseline: ~8.0 MB assets
- v2.0 CJK fonts: +2.7 MB（**大幅增加**）
- 但用 Android App Bundle per-language split，实际下载 100 KB - 700 KB per language

### 3.3 CJK 字体分发策略

**方案 A：打包进 APK**（简单）
```
Android APK size: 8.0 MB (v1.1) → 10.7 MB (v2.0 CJK)
所有用户下载全量
```

**方案 B：Android App Bundle (`.aab`) per-language split** ⭐ **推荐**
```
App Bundle size: 10.7 MB total
用户下载: 仅自己语言 + Latin
en-only: 8.0 MB
en + es + pt-BR: 8.2 MB
en + zh-CN: 8.7 MB (Latin 95 KB + Noto SC 700 KB)
```

**方案 C：运行时下载字体**（激进）
- 玩家首次切换语言时下载 CJK 字体
- 节省 APK 大小，但需要 internet
- 风险：首次中文玩家断网体验差

**v2.0 推荐**：**方案 B（App Bundle per-language split）** — Google Play 自动按用户设备语言分包。

### 3.4 ⚠️ 字体 fallback 策略

**问题**：v2.0 8 语言，CJK 字体与 Latin 字体分离。运行时按玩家语言切换字体。

**Godot 4 Theme multi-font**：
```csharp
// 玩家设置语言 = zh-CN → 用 Noto Sans SC + 仍保留 Latin font（数字 / 英文 fallback）
public static Theme BuildChineseTheme() {
    var theme = new Theme();
    
    var latinRegular = new FontFile();
    latinRegular.LoadDynamicFont("res://assets/fonts/Nunito-Regular-subset.ttf");
    
    var chineseRegular = new FontFile();
    chineseRegular.LoadDynamicFont("res://assets/fonts/NotoSansSC-Regular-subset.otf");
    
    // 中文为主，Latin 作为 fallback（数字 / 英文 / 标点用 Nunito）
    theme.SetFont("font", "Label", chineseRegular);
    theme.SetFont("fallback", "Label", latinRegular);
    
    return theme;
}
```

---

## 4. 多语言打包策略

### 4.1 Android App Bundle + per-language split ⭐ 推荐

**实施**：
1. 在 `export_presets.cfg` 加 App Bundle 输出（`.aab`）
2. 把每语言资源（PO + 字体 subset）分目录存放
3. Godot 4.7 export 引擎自动按 locale 生成 split

**目录结构**：
```
assets/
├── fonts/  (Latin fonts — 所有语言共享)
│   ├── Fredoka-Bold-subset.ttf
│   ├── Nunito-Regular-subset.ttf
│   └── ...
├── fonts-cjk/
│   ├── zh-CN/
│   │   ├── NotoSansSC-Regular-subset.otf
│   │   └── NotoSansSC-Bold-subset.otf
│   ├── ja/
│   │   ├── NotoSansJP-Regular-subset.otf
│   │   └── ...
│   └── ko/
│       └── ...
├── locale/
│   ├── en.po
│   ├── es.po
│   ├── pt-BR.po
│   ├── zh-CN.po
│   ├── ja.po
│   ├── ko.po
│   ├── de.po
│   └── fr.po
```

**Android `.aab` per-locale filter**（`build.gradle`）：
```gradle
android {
    bundle {
        language {
            enableSplit = true
        }
    }
}
```

### 4.2 包大小估算（v2.0 完整 i18n）

| 玩家语言 | 下载大小 | 节省（vs 全量） |
|---------|----------|-----------------|
| en (Latin only) | ~8.0 MB | 0% |
| en + es + pt-BR (Latin + 3 翻译 PO) | ~8.05 MB | 25% |
| en + zh-CN (Latin + 1 CJK font) | ~8.7 MB | 19% |
| en + ja (Latin + 1 CJK font) | ~8.7 MB | 19% |
| en + 8 语言 (Latin + 4 CJK fonts) | ~10.7 MB | 0% |
| 全语言（最坏情况） | ~10.7 MB | 0% |

### 4.3 iOS App Store localization（v2.0 决策）

**iOS App Store 多语言 metadata**：
- App name 多语言
- Description 多语言
- Screenshots per language（v1.1+）

**v1.1 不做**（Magic Match v1.0 ship Android-first；v2.0 看 Austin iOS 计划）。

---

## 5. 风险 + 实施工作量估算

### 5.1 风险评估

| # | 风险 | 概率 | 影响 | 缓解 |
|---|------|------|------|------|
| 1 | **翻译质量差**（机器翻译 vs 人工） | 🟡 Medium | UX 下降 / 玩家流失 | DeepL Pro + human review；专业 translator；术语表 |
| 2 | **字符串遗漏**（UI 新增未 extract） | 🟡 Medium | 部分语言 fallback 到英文 | CI auto-extract + Weblate 同步 |
| 3 | **CJK 字体大小爆炸** | 🟡 Medium（per §3.2 +2.7 MB） | 用户下载慢 | App Bundle split + 按需下载 |
| 4 | **RTL 语言**（阿拉伯语 / 希伯来语） | 🟢 Low（v2.0 不做） | UI 布局反向 | v2.0+ 加 RTL 支持，需 Godot `LayoutDirection` |
| 5 | **Cultural adaptation**（颜色 / 图标 / 数字） | 🟢 Low | 个别市场 cultural miss | v2.0+ 区域化图标包 |
| 6 | **翻译冻结**（release 前最后翻译确认） | 🟡 Medium | release 延期 | release 前 1 周冻结，所有翻译 lock |
| 7 | **运行时语言切换** | 🟢 Low | 已支持 | Godot `TranslationServer.SetLocale()` |
| 8 | **字体 fallback 字符缺失** | 🟢 Low | 中文 + Latin 字符都能渲染 | 验证 Latin / CJK 字符集 |
| 9 | **App Bundle split 兼容旧设备** | 🟢 Low | 旧 Android 不支持 split | min SDK 24 已支持 App Bundle |

**综合风险等级**：🟢 **低-中**（成熟技术栈，可分阶段实施）

### 5.2 v1.1 工作量估算（Jacob 主要工作 + Christine 协调）

| 任务 | 工时 | 责任 |
|------|------|------|
| 创建 `locale/` 目录结构 + en.po baseline | 0.5h | Christine |
| 提取所有 v1.0 hardcode strings → en.po | 1h | Christine + Jacob |
| 在 src/Features/Match3/ 替换 `Label.Text = "..."` → `Tr("...")` | 2h | Jacob |
| Godot TranslationServer setup + load `locale/*.po` | 0.5h | Jacob |
| 翻译 es + pt-BR（DeepL Pro + human review） | 6h | Christine + Mark |
| Weblate 项目 setup + 3 语言 component | 1h | Christine |
| GitHub Actions extract_strings.yml + validate_translations.yml | 1h | Jacob |
| 真机 / Android emulator 切换语言 verify | 0.5h | Jacob |
| 视觉验收：Christine + Mark | 0.5h | All |
| **v1.1 i18n 总计** | **0.5-0.7 天** | — |

### 5.3 v2.0 工作量估算（CJK + 8 语言）

| 任务 | 工时 | 责任 |
|------|------|------|
| CJK font subset（Noto Sans SC/JP/KR）+ Latin + multi-font Theme | 1 day | Christine |
| 翻译 zh-CN / ja / ko / de / fr（5 语言） | 5-7 days | Translator team + Mark review |
| App Bundle per-language split setup | 0.5 day | Jacob |
| RTL 调研（v2.0 不做，记 backlog）| 0 | — |
| Godot i18n advanced（plural / RTL fallback） | 0.5 day | Jacob |
| 完整真机 × 3 设备 verify | 1 day | Jacob + Christine |
| **v2.0 完整 i18n 总计** | **2-3 周** | — |

### 5.4 v1.1 ship blocker？

❌ **不是**。v1.0 ship English only OK；v1.1 加 3 语言（en + es + pt-BR）。

---

## 6. v1.1 实施 checklist (Jacob + Christine)

### Phase A: 基础设施（D+12-13）

- [ ] 创建 `locale/` 目录（en.po / es.po / pt-BR.po 占位）
- [ ] 提取所有 v1.0 hardcode strings（按 §1.1 表格）→ en.po
- [ ] 在 `src/Features/Match3/*.cs` 替换 `Label.Text = "..."` → `Tr("...")`
- [ ] 创建 `src/Core/LocaleManager.cs`（load locale/*.po + TranslationServer.SetLocale）
- [ ] Godot Project Settings → Localization → 添加 en / es / pt-BR
- [ ] smoke test：英文显示正常（regression check）

### Phase B: 翻译（D+13-14）

- [ ] Weblate project setup（Christine）
- [ ] 添加 es / pt-BR components
- [ ] DeepL Pro 翻译 50-70 strings × 2 语言
- [ ] 人类 review + 术语统一
- [ ] 导出 → commit 到 main

### Phase C: 自动化（D+14）

- [ ] GitHub Actions `extract_strings.yml`（PR trigger）
- [ ] GitHub Actions `validate_translations.yml`（push trigger）
- [ ] msgfmt / msgmerge setup

### Phase D: 验证（D+14-15）

- [ ] Android emulator verify：切换 es / pt-BR，所有 UI 翻译显示
- [ ] 真机 verify ×1
- [ ] Settings 屏幕：Language picker（让玩家切语言）
- [ ] 视觉验收：Christine + Mark
- [ ] commit + push to `docs/i18n-strategy` → `art/v1.1-i18n-infrastructure`

### Phase E: 资源准备（v2.0 prep，optional D+12+）

- [ ] 调研 Noto Sans SC/JP/KR subset 工具链（Christine）
- [ ] 估算 v2.0 CJK 字体大小 + Android App Bundle split
- [ ] 创建 `docs/CJK_FONT_PLAN.md`（v2.0 prep）

---

## 7. 与 D+11 TTF research 关系

**v1.1 字体集成 = Latin only**（per TTF research §8.1）：

| 元素 | 字体 | 子集 | 来源 |
|------|------|------|------|
| Title (Latin) | Fredoka Bold | ~10 KB | Google Fonts |
| Body (Latin) | Nunito Regular/Medium/Bold | ~75 KB | Google Fonts |
| 数字 / 标点 (Latin) | Nunito Regular fallback | — | 同上 |

**CJK 字体 v2.0 prep**（per TTF research §3.2）：

| 元素 | 字体 | 子集 | 大小估算 |
|------|------|------|----------|
| 中文 SC | Noto Sans SC Regular/Bold | 3500 chars | ~1.4 MB |
| 日文 | Noto Sans JP Regular | 3500 chars | ~700 KB |
| 韩文 | Noto Sans KR Regular | 3500 chars | ~600 KB |
| CJK 总计 | 3 languages | — | ~2.7 MB |

**i18n + TTF 协调**：
- v1.1: i18n infrastructure + Latin fonts subset（5 fonts）
- v2.0: CJK fonts subset（4 fonts）+ App Bundle split + 8 语言完整翻译

---

## 8. 决策点（Mark 拍板）

### 8.1 v1.1 必拍板

| # | 拍板项 | 决策 |
|---|--------|------|
| 1 | **v1.1 翻译源** | ✅ en + es + pt-BR（推荐） |
| 2 | **v1.1 翻译方式** | ✅ Weblate hosted free OSS（推荐） |
| 3 | **v1.1 翻译资金** | DeepL Pro $25/月 + 人工 review，预算批准？ |
| 4 | **v1.1 包大小** | 接受 +120 KB（8.0 → 8.12 MB）✅ |

### 8.2 v2.0 必拍板（future）

| # | 拍板项 | 决策 |
|---|--------|------|
| 5 | **v2.0 CJK 字体** | Noto Sans SC/JP/KR subset（per §3.2）✅ |
| 6 | **v2.0 完整语言** | 8 语言（en/es/pt-BR/zh-CN/ja/ko/de/fr）✅ |
| 7 | **v2.0 包大小** | 接受 +2.7 MB（8.0 → 10.7 MB）+ App Bundle split 优化 ✅ |
| 8 | **v2.0 翻译预算** | 8 语言 × ~$50/lang = ~$400 + 翻译者协调成本 |

### 8.3 v1.1 视觉验收标准

- [ ] 语言切换器在 Settings 屏幕正常显示 3 选项
- [ ] 切换 es 后：所有 UI 文字显示西语（无英文残留）
- [ ] 切换 pt-BR 后：所有 UI 文字显示葡语
- [ ] 数字 / 标点仍用 Nunito Latin font（v1.1 不引入 CJK）
- [ ] 字号不溢出容器（西语 / 葡语可能比英文长 10-20%）

---

## 附录 A：变更日志

### v0.1 (2026-10-06, Christine)

D+12 周五上午 i18n strategy doc。

**新增**：
- §0 TL;DR
- §1 v1.1 vs v2.0 文案拆分（哪些 hardcode / 哪些 extract，~50-70 strings）
- §2 翻译工作流（GitHub PR vs Weblate vs Crowdin）→ **推荐 Weblate**
- §3 CJK 字体集成（Noto Sans SC/JP/KR subset ~2.7 MB + App Bundle split）
- §4 多语言打包策略（per-language split 节省 19-25% 下载）
- §5 9 项风险评估
- §6 v1.1 + v2.0 实施工作量估算（v1.1 0.5-0.7 天 / v2.0 2-3 周）
- §7 与 TTF research 协调
- §8 决策点 for Mark

**决策**：
- ✅ v1.1 = en + es + pt-BR 3 语言
- ✅ 推荐 Weblate hosted free OSS
- ✅ Godot PO + GitHub Actions 自动化
- ✅ v2.0 CJK 字体 = Noto Sans SC/JP/KR subset
- ✅ Android App Bundle per-language split

**未决**：
- v2.0 RTL 语言支持（ar / he）—— v2.0+ backlog
- iOS App Store localization 时间（v2.0+ if iOS 计划）

---

## 附录 B：参考链接

- [Godot 4 i18n docs](https://docs.godotengine.org/en/stable/tutorials/i18n/index.html)
- [Godot TranslationServer API](https://docs.godotengine.org/en/stable/classes/class_translationserver.html)
- [GNU gettext .po format](https://www.gnu.org/software/gettext/manual/html_node/PO-Files.html)
- [Weblate OSS](https://weblate.org)
- [Noto Sans fonts](https://fonts.google.com/noto/specimen/Noto+Sans+SC)
- [Android App Bundle + per-language split](https://developer.android.com/guide/app-bundle)

---

**i18n strategy 完。** Mark verify 后 commit 到 main。v1.1 实施前需要 Mark 拍板 §8.1 + v1.1 翻译预算批准。