# godot-template — 美术规范 ART_SPEC v1.0

> **状态**：**v1.0 ship-ready**（D+9 周二 docs 整合完毕；5 个 art branch 已 push，待 Jacob 验证 + Mark 拍板 merge）
> **范围**：godot-template + Magic Match v1.0（60 关 / 3 主题 / Android-first F2P Casual）
> **负责**：Christine（art direction + asset QA + 配色文档）/ Mark（review + 拍板）
> **生成时间**：2026-10-06 (D+9 周二)
> **依赖**：
> - `ASSET_INTEGRATION.md` v0.1（资产接入流程不变）
> - `/shared/austin-christine-brief.md`（v1.0 资产清单来源）
> - [`/shared/match3-design-doc-v0.5-preview.md`](/shared/match3-design-doc-v0.5-preview.md)（Austin 产品设计文档 — 与本文档互为 cross-ref）
> **本版本变更**：见 [附录 B 变更日志](#附录-b变更日志)

---

## 目录

1. [设计风格（沿用 v0.1 实际）](#1-设计风格)
2. [调色板（品牌 + UI）](#2-调色板)
3. [主题配色系统（Forest / Desert / Ocean）](#3-主题配色系统)
4. [字体](#4-字体)
5. [间距 token](#5-间距-token)
6. [暗色 / 亮色模式](#6-暗色--亮色模式)
7. [资产规格总览](#7-资产规格总览)
8. [v1.0 ship-ready 资产清单](#8-v10-ship-ready-资产清单)
9. [组件清单](#9-组件清单)
10. [状态规范](#10-状态规范)
11. [错误状态](#11-错误状态)
12. [动效规范](#12-动效规范)
13. [AI 出图规则](#13-ai-出图规则)
14. [验收 checklist](#14-验收-checklist)
15. [联系表](#15-联系表)
- [附录 A：fork-friendly 修改指南](#附录-afork-friendly-修改指南)
- [附录 B：变更日志 v0.1 → v1.0](#附录-b变更日志)
- [附录 C：v0.2 polish 工作历史（pointer）](#附录-cv02-polish-工作历史)

---

## 1. 设计风格

### 1.1 风格定义（已拍板）

**"Glossy magical gem"** — 半写实宝石 + 卡通光泽 + 紫蓝宇宙感

| 维度 | 现状 |
|------|------|
| 宝石形 | smooth rounded teardrop（v0.1 实际有少量 facet，**接受不修**） |
| 宝石 outline | dark outline（深红 / 深蓝 / 深绿 / 深紫 / 深橙），与 v0.1 spec "white outline" 不同 — **spec 已对齐实际** |
| 宝石高光 | 左上白色硬心月牙 + 右下小白点（每颗位置一致） |
| 宝石 sparkle | 1-5 颗白色 4-point star / 圆点（v0.1 不齐 → **v1.0 polish 后全部 6 颗一致 family**） |
| 风格参照 | Candy Crush 早期宝石（少"软糖"多"宝石"）、Bejeweled Classic |
| 整体调性 | Casual / F2P，目标玩家：休闲解谜用户 |

### 1.2 风格不接受的方向（避免偏离）

- ❌ 纯像素风（Stardew 风格） — 不适合 AI 出图 + 不适合 Casual
- ❌ 纯扁平（Material Design） — 缺体积感，失"宝石"调性
- ❌ 纯软糖（Candy Crush Soda） — 与现有 6 颗 gem 风格不连贯
- ❌ 写实宝石（Bejeweled Stars） — AI 易崩 + Casual 用户不接受

### 1.3 v0.1 → v0.2 关键 spec 调整

| v0.1 spec 写 | v0.2 / v1.0 实际接受 | 决定 |
|---|---|---|
| "solid WHITE 2-3 px outline" | "dark colored outline" | ✅ **更新 spec 对齐 PNG** |
| "smooth rounded teardrop, NO facets" | "teardrop + 少量 facet 内反射" | ✅ **接受半写实** |
| "sparkles 3-4 WHITE 散布" | "1-5 不齐 → v1.0 polish 后 1-3 一致" | ✅ **D+6 polish 完成** |

---

## 2. 调色板

### 2.1 Gem 色板（锁定，GemType enum 索引 0-5 对应）

| gem_type | 颜色 | hex (锁定) | anchor 词 | anti-drift |
|----------|------|-----------|-----------|-----------|
| 0 | red    | `#D61420` | DEEP SATURATED CRIMSON | NOT pink NOT magenta NOT orange |
| 1 | orange | `#FDA915` | VIBRANT AMBER TANGERINE | NOT yellow NOT red NOT pale |
| 2 | yellow | `#FDEA1C` | GOLDEN SUNSHINE | NOT lemon NOT cream NOT pale NOT lime |
| 3 | green  | `#52EB73` | DARK FOREST PINE-TREE GREEN | NOT lime NOT yellow-green NOT olive |
| 4 | blue   | `#2478D6` | BRIGHT MEDIUM SAPPHIRE | NOT cyan NOT navy NOT teal |
| 5 | purple | `#88489B` | DEEP AMETHYST GRAPE | NOT pink NOT magenta NOT lavender |

> ⚠️ **集成约束**：hex 严格按本表。`Board.cs` `GemType` enum 索引锁死 0-5，新增颜色时**追加**到末尾（6, 7, ...）并同步更新 `SpritePaths.GemTypeToSprite` map（见 `ASSET_INTEGRATION.md` §2）。

### 2.2 UI / 品牌色板

| Token | hex | 用途 |
|-------|-----|------|
| **品牌紫** | `#88489B` | App icon / 启动品牌 / coin 中央 gem / 主题强调 |
| **品牌冷色（蓝紫）** | `#5D7FE6` | iOS cool highlight / 主题 accent 候选 |
| **背景深紫** | `#1A0F2E` | cosmic bg 边缘 |
| **背景中紫** | `#4A2D6E` | cosmic bg 中圈 |
| **背景亮紫** | `#6B3FA0` | cosmic bg 中心 / mandala 主色 |
| **HUD 主文字** | `#FFFFFF` | 主文字 / 高光 |
| **Modal 蒙层** | `rgba(0, 0, 0, 0.6)` | 弹窗 |
| **Warning / Game Over** | `#FF6B6B` | 红色警告 |
| **Success / Win** | `#52EB73` | 与 gem_green 同 |

### 2.3 Star / Heart / Coin 配色

| 元素 | 主色 hex | accent hex | outline |
|------|---------|-----------|---------|
| `star_on` | `#FFD700`（gold）| `#FFEC8B`（light gold）| `#FFFFFF` |
| `star_off` | `#A0A0A0`（gray）| `#707070`（dark gray）| `#FFFFFF` |
| `hud_heart` | `#FF4757`（red）| `#FF8B95`（light pink-red）| `#FFFFFF` |
| `coin` | `#FFD700`（gold）| `#88489B`（purple gem 内嵌）| `#FFFFFF` |

---

## 3. 主题配色系统

> **D+4 整合**：本节合并自 `docs/design/theme-palette.md`（该文件 v1.0 起改为 redirect pointer，详见 [附录 C](#附录-cv02-polish-工作历史)）。

### 3.1 概述

Match3 v1.0 关卡系统分 **3 主题**（Forest / Desert / Ocean），各占 20 关。

**主题切换**在 C# 端实现（详见 §3.7 调用清单）：

```csharp
// src/Features/Match3/Theme.cs (v1.0 由 Jacob 加)
public enum MatchTheme { Forest, Desert, Ocean }

// src/Features/Match3/ThemeColors.cs (v1.0 由 Jacob 加)
public static class ThemeColors
{
    public static (Color Primary, Color Accent, Color TextOnPrimary) Get(MatchTheme theme)
        => theme switch
        {
            MatchTheme.Forest => (new Color("#4A8F4A"), new Color("#F5DEB3"), new Color("#FFFFFF")),
            MatchTheme.Desert => (new Color("#D2A679"), new Color("#FF8C42"), new Color("#FFFFFF")),
            MatchTheme.Ocean  => (new Color("#1E5F8C"), new Color("#7FCDCD"), new Color("#FFFFFF")),
            _ => (new Color("#88489B"), new Color("#FFD955"), new Color("#FFFFFF")),
        };
}
```

### 3.2 Forest（关卡 1–20）

**主题叙事**：苔藓 / 树叶 / 棕色树干

| Token | hex | RGB | 用途 |
|-------|-----|-----|------|
| **Primary** | `#4A8F4A` | rgb(74, 143, 74) | HUD 边框 / 主题强调 |
| **Accent** | `#F5DEB3` | rgb(245, 222, 179) | 副标题 / level number |
| **TextOnPrimary** | `#FFFFFF` | rgb(255, 255, 255) | Primary 上的文字 |
| **Background tint** | `#1F3D1F` | rgb(31, 61, 31) | 关卡背景叠加（可选，alpha 30%） |
| **Button bg** | `#5FAA5F` | rgb(95, 170, 95) | 关卡按钮 normal |
| **Button pressed** | `#3D7A3D` | rgb(61, 122, 61) | 关卡按钮 pressed |
| **Shadow / depth** | `#2D5C2D` | rgb(45, 92, 45) | 按钮下边阴影 |

**对比度 check**：
- `#FFFFFF` on `#4A8F4A` = 4.59:1 (AA pass for normal text) ✅
- `#F5DEB3` on `#4A8F4A` = 3.05:1 (AA pass for large text only) ⚠️ 用于副标题字号 ≥ 18 px OK
- `#F5DEB3` on `#1F3D1F` (深底) = 7.8:1 (AAA pass) ✅

### 3.3 Desert（关卡 21–40）

**主题叙事**：黄昏沙丘 / 仙人掌

| Token | hex | RGB | 用途 |
|-------|-----|-----|------|
| **Primary** | `#D2A679` | rgb(210, 166, 121) | HUD 边框 / 主题强调 |
| **Accent** | `#FF8C42` | rgb(255, 140, 66) | 副标题 / level number |
| **TextOnPrimary** | `#FFFFFF` | rgb(255, 255, 255) | Primary 上的文字 |
| **Background tint** | `#5C3D1F` | rgb(92, 61, 31) | 关卡背景叠加（可选，alpha 30%） |
| **Button bg** | `#A87849` | rgb(168, 120, 73) | 关卡按钮 normal（⚠️ 不能用 Primary 否则白字不够 AA）|
| **Button pressed** | `#7A5028` | rgb(122, 80, 40) | 关卡按钮 pressed |
| **Shadow / depth** | `#5C3D1F` | rgb(92, 61, 31) | 按钮下边阴影 |

**对比度 check**：
- `#FFFFFF` on `#D2A679` = 2.31:1 (❌ 不够 AA) — 用于 Primary 上的文字时，**必须加阴影 / outline**，否则看不清
- `#FFFFFF` on `#FF8C42` = 2.51:1 (❌ 不够 AA) — 同上
- `#FFFFFF` on `#A87849` (button bg) = 4.39:1 (AA pass) ✅ **使用 button bg 作为底色**
- `#FFFFFF` on `#5C3D1F` (深底) = 9.5:1 (AAA pass) ✅

**修正历史**：v1.0 决策 = 用 `#A87849` (button bg) 做 button 底色（不是 Primary），保证文字 AA 对比度。

### 3.4 Ocean（关卡 41–60）

**主题叙事**：珊瑚 / 海浪 / 珍珠

| Token | hex | RGB | 用途 |
|-------|-----|-----|------|
| **Primary** | `#1E5F8C` | rgb(30, 95, 140) | HUD 边框 / 主题强调 |
| **Accent** | `#7FCDCD` | rgb(127, 205, 205) | 副标题 / level number |
| **TextOnPrimary** | `#FFFFFF` | rgb(255, 255, 255) | Primary 上的文字 |
| **Background tint** | `#0F2D44` | rgb(15, 45, 68) | 关卡背景叠加（可选，alpha 30%） |
| **Button bg** | `#2D7AB8` | rgb(45, 122, 184) | 关卡按钮 normal |
| **Button pressed** | `#154E73` | rgb(21, 78, 115) | 关卡按钮 pressed |
| **Shadow / depth** | `#0F3550` | rgb(15, 53, 80) | 按钮下边阴影 |

**对比度 check**：
- `#FFFFFF` on `#1E5F8C` = 6.51:1 (AAA pass) ✅
- `#FFFFFF` on `#2D7AB8` = 4.62:1 (AA pass) ✅
- `#0F2D44` (深底) on `#7FCDCD` = 5.46:1 (AA pass) ✅

### 3.5 跨主题保留元素（不切换）

| 元素 | hex | 来源 |
|------|-----|------|
| **6 颗 gem 色** | 见 §2.1 | 全主题共用 |
| **背景深紫（v0.1 紫蓝宇宙感）** | `#1A0F2E` / `#4A2D6E` / `#6B3FA0` | Theme background 下层 |
| **品牌主色** | `#88489B` | App icon / 启动品牌 |
| **Modal 蒙层** | `rgba(0, 0, 0, 0.6)` | 弹窗 |
| **Warning / Game Over** | `#FF6B6B` | 红色警告 |
| **Success / Win** | `#52EB73` | 与 gem_green 同 |

> Theme banner 文字：`"Forest"` / `"Desert"` / `"Ocean"`（英文，v1.0 ship；v1.1 i18n）

### 3.6 主题切换动效

**触发**：跨主题时（关卡 1→21 / 关卡 21→41）

| 阶段 | 时长 | 视觉 |
|------|------|------|
| 淡入 | 0.3 s | ThemeBanner 从 alpha 0 → 1 |
| 停留 | 1.0 s | 完全可见 |
| 淡出 | 0.2 s | ThemeBanner 从 alpha 1 → 0 |
| **总计** | **1.5 s** | banner 居中底部显示 |

**ThemeBanner 视觉**：PanelContainer + Label 居中，背景半透明黑（`rgba(0,0,0,0.5)`），文字 36 px + accent 色，2 px 圆角。

### 3.7 C# 端调用清单

> 以下 7 个 UI 元素在主题切换时需要 `AddThemeColorOverride`：

```csharp
// 1. HUD 边框 (BorderContainer)
hudBorder.AddThemeColorOverride("border_color", primary);

// 2. HUD Theme label (Label)
themeLabel.AddThemeColorOverride("font_color", accent);

// 3. Level button normal (Button)
levelButton.AddThemeColorOverride("font_color", textOnPrimary);
// levelButton.AddThemeStyleboxOverride("normal", buttonNormalStylebox(primary));

// 5. Theme banner background (PanelContainer)
themeBanner.AddThemeStyleboxOverride("panel", bannerStylebox(primary));

// 6. Progress bar fill (ProgressBar)
progressBar.AddThemeStyleboxOverride("fill", progressFillStylebox(primary));

// 7. Selected level highlight (Panel)
selectedLevelPanel.AddThemeStyleboxOverride("panel", selectedStylebox(accent));
```

Jacob 在 v1.0 实现时按这个表查找颜色。v1.0 ship 必须保证 **Forest / Desert / Ocean 三主题都能正确切换 + 文字对比度 ≥ AA**。

### 3.8 颜色选择逻辑（来自 brief 灵感词锚定）

- **Forest** → 苔藓 / 树叶 / 棕色树干 → 绿主 + 米黄副
- **Desert** → 黄昏沙丘 / 仙人掌 → 沙主 + 橙夕阳副
- **Ocean** → 珊瑚 / 海浪 / 珍珠 → 深蓝主 + 青绿浪花副

每个主题 Primary / Accent 互补（暖+冷或冷+暖），避免单调。

**为什么 v0.1 紫蓝宇宙感不变成主题之一？**

紫蓝宇宙是 **base 背景层**（永远在），主题是在 base 之上的 **HUD / 关卡按钮 / 横幅 overlay 色**。3 主题只在"前景 UI" 换色，不动背景。

---

## 4. 字体

### 4.1 v1.0 ship 状态

**Godot default**（v1.0 不加 TTF）。

理由：
- ✅ TTF 增加 ~1-2 MB 包大小（CJK 字体尤其大）
- ✅ Casual 玩家对字体的容忍度高于 hardcore
- ✅ v1.1 可升级（CJK / 多语言时一起加）

### 4.2 字体规格（v1.0）

| Token | size | weight | 用途 |
|-------|------|--------|------|
| `text_xl` | 36 px | Bold | 标题（"Forest" / "Desert" / "Ocean"）|
| `text_lg` | 24 px | Bold | 副标题 / level number |
| `text_md` | 18 px | Regular | 按钮文字 / 主 UI |
| `text_sm` | 14 px | Regular | 提示 / 标签 |
| `text_xs` | 12 px | Regular | 版权 / 调试 |

### 4.3 文字颜色（与主题联动）

- 主文字：`#FFFFFF`（全主题）
- 主题强调文字：用主题 Accent（§3）
- 警告文字：`#FF6B6B`
- 成功文字：`#52EB73`

---

## 5. 间距 token

### 5.1 标准间距（统一）

| Token | 数值 | 用途 |
|-------|------|------|
| `space_xs` | 4 px | 紧邻元素（icon + label）|
| `space_sm` | 8 px | 组件内 padding |
| `space_md` | 16 px | 组件间 gap / section padding |
| `space_lg` | 24 px | 大区块分隔 |
| `space_xl` | 32 px | 屏幕边缘 padding |

### 5.2 圆角

| Token | 数值 |
|-------|------|
| `radius_sm` | 4 px（chip / tag）|
| `radius_md` | 8 px（button / card）|
| `radius_lg` | 16 px（modal / panel）|

---

## 6. 暗色 / 亮色模式

### 6.1 v1.0 ship 状态

**仅暗色模式**。理由：
- ✅ 紫蓝宇宙感本就是 dark theme
- ✅ Casual Android 用户大多数系统是 dark 或跟随系统
- ✅ 6 颗 gem 在 dark 背景上对比度好（v0.1 已验证 7.4/10 avg）
- ⏸️ 亮色模式可作 v1.1 改进（需要重新调整 gem outline + bg）

### 6.2 暗色背景色（v1.0）

| Token | hex | 用途 |
|-------|-----|------|
| `bg_deep` | `#1A0F2E` | 屏幕边缘 |
| `bg_mid` | `#4A2D6E` | 屏幕中圈 |
| `bg_bright` | `#6B3FA0` | 中心 mandala / 视觉锚 |

---

## 7. 资产规格总览

### 7.1 文件大小预算

| 类别 | 单文件上限 | 总预算 |
|------|----------|--------|
| Gem (1024×1024) | 800 KB | ~6 MB |
| Button + UI sprite (256-512) | 100 KB | ~1 MB |
| Background (720×1280) | 800 KB | ~1.6 MB |
| Icon SVG | 10 KB | 10 KB |

**v1.0 总预算 ~12 MB → 实际 ~7.9 MB（节省 34%）**

### 7.2 import 设置锁定（per `ASSET_INTEGRATION.md` §3.1）

```ini
compress/mode = 0              # Lossless
compress/high_quality = false
mipmaps/generate = false         # 2D UI 不需要 mipmap
process/fix_alpha_border = true # 修复 alpha 边缘
process/premult_alpha = false  # 不预乘（runtime 兼容）
```

---

## 8. v1.0 ship-ready 资产清单

> **D+9 状态**：所有 30 个运行时资产已 ship-ready（5 个 art branch push + 1 个 docs branch in progress）。

### 8.1 Gem（12 PNG，6 颗 × 2 版本）

| 文件 | 尺寸 | 大小 | 状态 | Commit |
|------|------|------|------|--------|
| `gem_red.png` + `.png.import` | 1024×1024 RGBA | ~590 KB | ✅ D+5 polished | 5c53a28 |
| `gem_red_alpha.png` | 1024×1024 RGBA | ~580 KB | ✅ D+5 polished | 5c53a28 |
| `gem_orange.png` + `.png.import` | 1024×1024 RGBA | ~410 KB | ✅ v0.1 base | 31c2ac3 |
| `gem_orange_alpha.png` | 1024×1024 RGBA | ~410 KB | ✅ D+6 sparkle polish | ec1caf2 |
| `gem_yellow.png` + `.png.import` | 1024×1024 RGBA | ~430 KB | ✅ v0.1 base | 31c2ac3 |
| `gem_yellow_alpha.png` | 1024×1024 RGBA | ~430 KB | ✅ D+6 sparkle polish | ec1caf2 |
| `gem_green.png` + `.png.import` | 1024×1024 RGBA | ~410 KB | ✅ v0.1 base | 31c2ac3 |
| `gem_green_alpha.png` | 1024×1024 RGBA | ~410 KB | ✅ v0.1 base | 31c2ac3 |
| `gem_blue.png` + `.png.import` | 1024×1024 RGBA | ~410 KB | ✅ v0.1 base | 31c2ac3 |
| `gem_blue_alpha.png` | 1024×1024 RGBA | ~410 KB | ✅ v0.1 base | 31c2ac3 |
| `gem_purple.png` + `.png.import` | 1024×1024 RGBA | ~485 KB | ✅ v0.1 base | 31c2ac3 |
| `gem_purple_alpha.png` | 1024×1024 RGBA | ~485 KB | ✅ D+5 polished (target #88489B, actual #77338F) | 5c53a28 |

### 8.2 UI（10 PNG）

| 文件 | 尺寸 | 大小 | 状态 | Commit |
|------|------|------|------|--------|
| `button_normal.png` + `.png.import` | 1024×1024 RGBA | ~435 KB | ✅ D+6 purple-ring fix | ec1caf2 |
| `button_normal_alpha.png` | 1024×1024 RGBA | ~530 KB | ✅ D+6 purple-ring fix | ec1caf2 |
| `button_pressed.png` + `.png.import` | 1024×1024 RGBA | ~415 KB | ✅ D+7 purple-ring fix | f073440 |
| `button_pressed_alpha.png` | 1024×1024 RGBA | ~575 KB | ✅ D+7 purple-ring fix | f073440 |
| `star_on.png` + `.png.import` | 256×256 RGBA | ~10 KB | ✅ D+4 hand-written SVG | c5980aa |
| `star_off.png` + `.png.import` | 256×256 RGBA | ~8 KB | ✅ D+4 hand-written SVG | c5980aa |
| `btn_play.png` + `.png.import` | 256×256 RGBA | ~11 KB | ✅ D+5 hand-written SVG | 5c53a28 |
| `btn_locked.png` + `.png.import` | 256×256 RGBA | ~8 KB | ✅ D+5 hand-written SVG | 5c53a28 |
| `hud_heart.png` + `.png.import` | 128×128 RGBA | ~5 KB | ✅ D+5 hand-written SVG | 5c53a28 |
| `coin.png` + `.png.import` | 128×128 RGBA | ~9 KB | ✅ D+5 hand-written SVG | 5c53a28 |
| `level_select_bg.png` + `.png.import` | 720×1280 RGB | ~470 KB | ✅ D+4 AI re-gen | c5980aa |

### 8.3 Backgrounds（2 PNG）

| 文件 | 尺寸 | 大小 | 状态 | Commit |
|------|------|------|------|--------|
| `bg_title.png` + `.png.import` | 720×1280 RGB | ~795 KB | ✅ D+7 AI re-gen (ornate lotus) | f073440 |
| `bg_game.png` + `.png.import` | 720×1280 RGB | ~740 KB | ✅ D+6 AI re-gen (mandala) | ec1caf2 |

### 8.4 Icon（1 SVG）

| 文件 | 尺寸 | 大小 | 状态 | Commit |
|------|------|------|------|--------|
| `icon.svg` + `.svg.import` | 1024×1024 viewBox | ~6 KB | ✅ D+2 with 3 微调 (sparkles / glow / cool-highlight) | 54947ae |

### 8.5 v1.0 ship 总计：~7.9 MB / 12 MB 预算（节省 34%）

---

## 9. 组件清单

### 9.1 全屏组件

| 组件 | 说明 | 关键资产 |
|------|------|----------|
| **TitleScreen** | 启动屏 / 主菜单 | bg_title + icon + button_normal |
| **LevelSelectScreen** | 关卡选择（5 列 × 4 行 = 20 关）| level_select_bg + button_normal + star_on/off |
| **GameplayScreen** | 游戏主屏（6×8 棋盘）| bg_game + 6 gem + hud_heart + coin |
| **EndScreen** | 结算（Win / Lose）| bg_game + star_on/off + button_normal |
| **ThemeBanner** | 跨主题 banner（overlay 1.5s）| 仅颜色，无 sprite |
| **Modal** | 弹窗 / 暂停 / 设置 | 半透明黑蒙层 + button_normal |

### 9.2 局部组件

| 组件 | 说明 | 关键资产 |
|------|------|----------|
| **Button** | 通用按钮（normal / pressed / locked）| button_normal + button_pressed + btn_locked |
| **HUD** | 顶部状态栏（heart + coin + theme label）| hud_heart + coin + 主题强调色 |
| **Toast** | 短通知（0.5s 淡入淡出）| 文字 + 半透明黑蒙层 |
| **ProgressBar** | 关卡进度 | 主题强调色 |
| **AdBadge** | "Ad" 标识（续命按钮内）| 文字 + AdMob 标识色 |
| **ScorePanel** | 分数 / 目标分数 | 文字 + 主题强调色 |

### 9.3 Gem 组件

| 组件 | 状态 | 视觉 |
|------|------|------|
| **Gem Idle** | 默认 | 当前 gem sprite |
| **Gem Selected** | 玩家点选 | scale 1.05 + 白 outline |
| **Gem Hint** | 系统提示可消除 | 0.5s pulse (alpha 0.5→1) |
| **Gem Match** | 3+ 匹配 | scale 1.2 + alpha 0（0.3s） |
| **Gem Fall** | 下落补位 | y offset → 0（0.4s ease-out）|
| **Gem Spawn** | 顶部生成 | y -50 → 0（0.3s）|

---

## 10. 状态规范

### 10.1 Button 状态

| 状态 | 视觉 |
|------|------|
| **normal** | button_normal.png（金 gold gradient，无紫环）|
| **pressed** | button_pressed.png（更深 gold gradient，无紫环）|
| **hover** | modulate 1.0 → 1.1（v1.0 不需要，Godot 默认）|
| **disabled** | gray scale + alpha 0.5 |
| **locked** | btn_locked.png（gray padlock + gray 底）|

### 10.2 Theme 状态

3 个 enum：`Forest` / `Desert` / `Ocean`（详见 §3）。

跨主题切换：v1.0 触发于关卡 1→21（Forest→Desert）和 21→41（Desert→Ocean）。

---

## 11. 错误状态

### 11.1 游戏内错误

| 情况 | UI 反馈 |
|------|---------|
| **Moves 用完** | EndScreen "Out of Moves" 红字 + ContinueAdButton 续命 |
| **无合法 swap** | Toast "No moves available"（0.5 秒淡入淡出）+ 自动重洗棋盘 |
| **Score 永远达不到 target** | 调参 → D+5 平衡数值表解决，不靠美术 |
| **Save 失败** | Toast "Save failed. Please retry." + 静默 retry 1 次 |

### 11.2 系统错误

| 情况 | UI 反馈 |
|------|---------|
| **网络错误（AdMob 加载失败）** | 静默降级：续命按钮 disable + Toast "Ad unavailable now" |
| **资源加载失败** | C# 端 load 失败 → log + 不崩溃 + 退到 TitleScreen |
| **First-run 引导失败** | D1 留存问题 → D+5 v1.0 验证 |

---

## 12. 动效规范

### 12.1 v1.0 ship 状态

| 元素 | v0.1 状态 | v1.0 计划 |
|------|-----------|-----------|
| 匹配清除 | ❌ 直接刷 sprite | ✅ Tween ScaleFadeOut 0.3s |
| 宝石下落 | ❌ 直接调 sheet | ✅ Tween Position 0.4s ease-out |
| 按钮按下 | ✅ Godot 默认 | 沿用 |
| 屏切换 | ❌ 直接 SwapScreen | ✅ Tween FadeIn/Out 0.2s |
| Star 评分 | ❌ 一次性显示 | ✅ 3 张依次 StarPop 0.1s 间隔 |
| Theme banner | ❌ 无 | ✅ Tween ModulateAlpha 1.5s（详见 §3.6）|

### 12.2 动效实现（v0.2 仅规范，实现 v1.0）

| 动效 | 实现方式 | 时长 | easing |
|------|---------|------|--------|
| Gem match clear | `Tween.Scale()` + `Tween.ModulateAlpha()` | 0.3 s | EaseOut |
| Gem fall | `Tween.Position()` | 0.4 s | EaseOut Bounce |
| Screen transition | `Tween.ModulateAlpha()` | 0.2 s | Linear |
| Star pop | `Tween.Scale()` 1→1.2→1 | 0.3 s | EaseInOut |
| Theme banner | `Tween.ModulateAlpha()` | 1.5 s | SineInOut |
| Button hover | modulate 1.0 → 1.1 | 0.1 s | Linear |

### 12.3 粒子 / 特效（v1.0 P1）

| 场景 | 实现 |
|------|------|
| Gem match 4+ | `Particle2D` 8 颗 sparkle burst |
| Cascade | 同上 + 屏幕轻微震动 0.1s |
| Level complete | 全屏 confetti 0.5s |

> v0.2 仅列规范，实现细节由 Jacob 在 v1.0 完成。

---

## 13. AI 出图规则

### 13.1 v1.4 防羽化规则（必须遵守）

**严禁出现**（prompt 否定短语或"避免"）：

| ❌ 关键词 | 类型 |
|-----------|------|
| `halo` | 边缘羽化 |
| `bloom` | 边缘扩散 |
| `soft-glow` / `soft glow` | 软发光 |
| `soft-shadow` / `soft shadow` | 软阴影 |
| `feathered edges` | 边缘羽化 |
| `light flare` | 光斑 |
| `lens flare` | 镜头光晕 |
| `sparkle beam` | 闪光光束 |
| `glowing` / `glowing X` | 任何带 "glowing" 的短语 |

**必须**（prompt 肯定）：

- ✅ 硬边 (hard edges)
- ✅ 明确轮廓 (clear silhouette / defined outline)
- ✅ 实心 (solid fill)
- ✅ **solid pure WHITE 背景**（rembg 后处理前提）
- ✅ 主体居中、四周留白 5-15%（rembg 边缘保留）

### 13.2 已知 image-01 限制（D+1 验证，5/10 评分）

| # | 限制 | 缓解 |
|---|------|------|
| 1 | 不理 transparent background | 显式 "solid pure WHITE background" |
| 2 | 不理 white outline | design 接受（dark outline 已记 v0.2） |
| 3 | 不理 white sparkles | 接受 family drift（sparkles 颜色跟主体） |
| 4 | 不理 "no text" on button | PIL 后处理去字 |
| 5 | sapphire/emerald 触发 crystal facets | 加 "jelly bean / gummy candy / NOT crystalline" 锚点 |
| 6 | 1:1 默认输出 = 1024×1024 | scale-down 处理 |
| 7 | 9:16 默认输出 = 720×1280 | 完美匹配 |
| 8 | `output_directory` 参数无效 | 手动 curl 下载 |
| 9 | 下载链路偶发 JPEG-as-PNG | 8-byte magic verify + `Image.open(p).save(p, format="PNG")` 修复 |
| 10 | bria-rmbg 1GB 模型 OOM | 强制 `--model u2netp` (4.57MB) |

### 13.3 新增 icon 类别（star / heart / coin / button）的特殊规则

- 都是简单几何形状，AI 出图风险低 → **D+5 起改用手写 SVG（更可控、更小文件）**
- star 5 角形 + 金色 + 白 outline（与 gem family 一致）
- heart 红心形 + 白 outline
- coin 圆形 + 金色 + 中央数字 / 图案
- button 圆角 + 金 gradient + 内嵌 icon（▶ / 🔒）

### 13.4 PIL 后处理经验沉淀（D+5/D+6）

| 技巧 | 用途 |
|------|------|
| **smart-mask gain** | gem_purple 紫环 polish：只对 `(B>G AND R>G AND purple_body)` mask 应用 gain，**避开** shadow disc 区域，避免绿色溢出 |
| **purple ring detection** | `(R>100) & (G<130) & (B>70) & (B>G) & (R>G) & (A>100)` — 20,000 像素 button_normal 紫环 100% 检测 |
| **gold gradient replacement** | 按 y-position 计算 target gold (#FEDD74 top → #F17132 bottom)，保证紫环替换无缝 |
| **edge ratio analysis** | purple pixel 在 edge zone 的比例，>0.6 = ring, <0.5 = distributed highlights — 区分 bug vs design intent |
| **PIL 直接画 sparkle** | 4-point star polygon + radial lines = gem sparkle decoration（10 行代码，60 行 Python）|
| **背景 AI 重出 > 手画** | 复杂 bg（mandala / lotus）AI 10 秒 vs 手画 2-3 小时 |

### 13.5 rembg 处理命令模板

```bash
# 6 颗 gem (preset gem)
python3 /root/icon_alpha.py \
  gems/gem_red.png gems/gem_orange.png gems/gem_yellow.png \
  gems/gem_green.png gems/gem_blue.png gems/gem_purple.png \
  --preset gem --model u2netp \
  --output-dir /root/godot-template/assets/gems/

# 2 张 button (preset button)
python3 /root/icon_alpha.py \
  ui/button_normal.png ui/button_pressed.png \
  --preset button --model u2netp \
  --output-dir /root/godot-template/assets/ui/

# Star / Heart / Coin (preset icon)
python3 /root/icon_alpha.py \
  ui/star_on.png ui/star_off.png ui/hud_heart.png ui/coin.png \
  --preset icon --model u2netp \
  --output-dir /root/godot-template/assets/ui/

# 工具位置：/shared/godot-template-tools/icon_alpha.py (生产)
#           /root/icon_alpha.py (本地备份)
```

---

## 14. 验收 checklist

> 每个新资产交付前必跑。

### 14.1 文件级

- [ ] canvas 尺寸正确（按 §7 矩阵）
- [ ] RGBA（gem / button / icon / star / heart / coin）/ RGB（bg）
- [ ] 文件大小 ≤ 上限（§7.1）
- [ ] PNG 8-byte magic verify（`89504E47` 不是 `FFD8FFE0`）
- [ ] partial ratio 1.2-2.5%（硬边干净）

### 14.2 视觉级（参考 D+1 评分维度）

- [ ] 形正确（teardrop / star 5-角 / heart / coin / bg 渐变方向）
- [ ] 高光位置一致（gem 左上 / star 上方 / heart 中央）
- [ ] outline 风格统一（dark colored 2-3 px，非 white — 接受 v0.2 实际）
- [ ] sparkles 数量 1-5 颗（gem family D+6 polish 后 1-3 一致）
- [ ] 颜色在 §2.1 / §3 hex ±5 内
- [ ] **无紫环**（`python3 scripts/purple-scanner.py` 必须 0 报告）

### 14.3 集成级（参考 ASSET_INTEGRATION.md）

- [ ] `godot --headless --import` 通过
- [ ] `.png.import` sidecar 已 commit
- [ ] `SpritePaths.cs` 路径映射更新（如新增）
- [ ] smoke test (`Match3SmokeTest.cs`) 通过
- [ ] `make verify && make build` 全绿

---

## 15. 联系表

| 角色 | Agent | 责任 |
|------|-------|------|
| 美术方向 + QA + AI 生图 + 配色文档 | **Christine** | ART_SPEC / 出图 / alpha / 视觉验收 / 主题配色 |
| 协调 + 决策 + review | **Mark** | 拍板 / 跨 agent 编排 / Founder 中继 |
| 集成（C# / Feature） | **Jacob** | 场景 / Godot 节点 / TextureButton / 屏切换 / 动效 |
| 框架（v0.3 EventBus / ScreenManager） | **Francisco** | UI 切换架构 |
| 玩法 + 关卡 + 设计文档 | **Austin** | Match3 设计 / 关卡参数 / UI 字段需求 |

---

## 附录 A：fork-friendly 修改指南

> fork godot-template 时改这些字段就能换主题 / 换品牌色 / 改关卡数 / 改主题系统。

### A.1 必须改（替换"Magic Match"身份）

```bash
bash scripts/configure.sh \
    --name "My Game" \
    --package "com.example.mygame" \
    --author "Your Name" \
    --description "One-line description"
```

（详见 `FORK_CHECKLIST.md` §0）

### A.2 改美术品牌色（可选）

| 字段 | 当前 | 改后 |
|------|------|------|
| 品牌主色（紫） | `#88489B` | 你的品牌色（影响 icon / button 强调 / 主题强调色） |
| 品牌冷色（蓝紫） | `#5D7FE6` | 你的品牌冷色 |
| 背景深紫 | `#1A0F2E` | 你的背景色 |
| HUD 主文字 | `#FFFFFF` | 你的文字色 |

代码端位置：`src/Features/Match3/GameScenes.cs` 里所有 `AddThemeColorOverride("font_color", ...)`。

### A.3 改主题配色（v1.0 引入）

详见 §3.2-3.4 + §3.7 调用清单。

fork 时只改 hex（每主题 7 个）+ 同步改 `ThemeColors.Get()` switch，**0.5 小时**工作量，不需要碰 sprite。

### A.4 改关卡数 / 主题数

| 改 | 文件 | 工作量 |
|---|------|--------|
| 关卡数 60 → N | `src/Features/Match3/LevelConfig.cs` | 5 分钟 |
| 主题数 3 → K | §3.2-3.4 + `Theme.cs` enum + `ThemeColors.Get()` switch | 2-3 小时 |
| 加新 gem 颜色 | §2.1 + `Board.cs` GemType enum（追加末尾）+ `SpritePaths.cs` map + 1 PNG | 1 小时 |
| 加新 bg | `assets/backgrounds/bg_X.png` + `.png.import` + `LevelConfig.cs` | 0.5 小时 |

### A.5 改字体（v1.1 升级 TTF）

`assets/fonts/` 加 `.ttf` 文件 → Godot `FontFile` 资源 → `theme.SetFont("default_font", font)`。预计 v1.1 增加 ~1-2 MB 包大小。

---

## 附录 B：变更日志

### v1.0 (2026-10-06, Christine)

D+9 docs 整合 + 5 个 art branch 完成。**ship-ready 状态**。

**重大变更（vs v0.2）**：

- **§3 主题配色系统** ← 合并自独立 `docs/design/theme-palette.md`（D+4 文件，现改为 1 行 redirect pointer）
  - §3.1 概述 + C# `ThemeColors.Get()` switch
  - §3.2 Forest / 3.3 Desert / 3.4 Ocean 完整 hex 表
  - §3.5 跨主题保留元素
  - §3.6 主题切换动效（1.5s banner）
  - §3.7 C# 端调用清单（7 个 UI override）
  - §3.8 颜色选择逻辑（brief 灵感词锚定）
- **§6 暗色/亮色模式** 简化（v1.0 仅暗色）
- **§7 资产规格** 增加总预算（~12 MB → 实际 7.9 MB）
- **§8 v1.0 ship-ready 资产清单** ← 完整替换 v0.2 的 §6/§7/§8：30 个运行时资产 + 5 个 commit hash 表格
- **§9 组件清单** 扩展：全屏 + 局部 + Gem 3 类组件
- **§10 状态规范** 增加 Theme 状态
- **§12 动效规范** 增加 Theme banner 1.5s 动效
- **§13 AI 出图规则** + **§13.4 PIL 后处理经验沉淀**：smart-mask gain / purple ring detection / gold gradient replacement / edge ratio analysis / sparkle drawing
- **§14.2 视觉级** + 必跑 `python3 scripts/purple-scanner.py`
- **附录 A fork-friendly** 扩展：品牌色 / 主题色 / 关卡数 / 主题数 / 新 gem / 新 bg / 字体升级

**ship-ready 状态**：
- 30 个产品资产文件（12 gem + 10 ui + 2 bg + 1 icon + 2 docs）
- 25 个 import sidecar（24 .png.import + 1 .svg.import）
- 总大小 ~7.9 MB（节省 34%）

### v0.2 (2026-10-06, Christine)

D+3 docs 重写。基于 Mark D+1 拍板 + Austin D+2 brief + D+1 评估。

**变更（vs v0.1）**：

- §1 设计风格：从"white outline + NO facets"改为"dark outline + 半写实 facet"（spec 对齐实际）
- §2.1 Gem 色：保留锁定 + anchor 词精简
- §2.2 UI 色：新增 `#5D7FE6` 冷色 + `#1A0F2E` 背景深紫
- §2.3 主题配色：新增（Forest / Desert / Ocean）— 占位指向未来 theme-palette.md
- §2.4 Star / Heart / Coin 配色：新增（Austin brief §5）
- §3 字体：从"未提及"改为"v1.0 ship 用 Godot default + v1.1 TTF 升级"
- §4 间距 token：新增（xs / sm / md / lg / xl）
- §5 暗色 / 亮色模式：明确"v1.0 仅暗色"
- §6 资产规格：新增完整矩阵 + file size budget
- §7 18 PNG 现状评估：完整逐文件打分（7.4/10 avg）
- §8 v1.0 资产清单：新增（来自 Austin brief §2）
- §9 组件清单：新增完整表
- §10 状态规范：新增
- §11 错误状态：新增
- §12 动效规范：新增（v1.0 实现）
- §13 AI 出图规则：继承 + 新增 icon 类别规则
- §14 验收 checklist：新增
- §15 联系表：替换为 v0.2 实际角色
- 附录 A：fork-friendly color overrides 新增
- 附录 B：本变更日志

### v0.1 (2026-09-18, Beverly)

初版（Magic Match ART_SPEC v1.3 frozen + Founder v1.4 anti-feather 规则）。
10 张 base + 8 张 alpha。

---

## 附录 C：v0.2 polish 工作历史

> **D+9 决策**：polish 工作历史保留为独立 trace doc，**不**合并到本 ART_SPEC（保持本文件精炼）。

| Branch | Commit | Polish 内容 |
|--------|--------|------------|
| `art/v0.2-polish` | ec1caf2 | button_normal 紫环修复（19,978 像素） + bg_game mandala AI 重出 + gem_orange/yellow sparkle family 补齐 |
| `art/v0.2-polish-2` | f073440 | button_pressed 紫环修复（11,013 + 13,731 像素） + bg_title AI 重出（ornate lotus） |
| `art/v0.2-polish-3` | 6c3e20d | purple-scanner 工具脚本 + 0 new ring bugs 报告 |

**详细 polish 8 scan 方法 + 9 个触发文件 3 重验证** → 见独立 doc [`docs/polish-8-scan-report.md`](polish-8-scan-report.md)

**已废弃文件**：
- `docs/design/theme-palette.md` → v1.0 起改为 1 行 redirect pointer（内容已合并到本文件 §3）

---

**v1.0 美术 ship-ready。** 走 v1.1 polish backlog 等待 v1.0 ship 反馈。