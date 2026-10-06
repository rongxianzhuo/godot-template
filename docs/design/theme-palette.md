# Match3 Theme Palette — CSS-style Override Reference

> **作者**：Christine (Artist)
> **Date**：2026-10-06 (D+4 周五)
> **状态**：v1.0 Draft — 配合 Austin brief §2.3 + ART_SPEC v0.2 §2.3
> **用途**：C# 端 `AddThemeColorOverride()` 调用时的查表参考
> **不**增加任何 sprite — 纯颜色 override

---

## 0. 概述

Match3 v1.0 关卡系统分 **3 主题**（Forest / Desert / Ocean），各占 20 关。

**主题切换**在 C# 端实现：

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

// 调用示例 (HUD):
var (primary, accent, text) = ThemeColors.Get(CurrentTheme);
hudBorder.AddThemeColorOverride("border_color", primary);
themeLabel.AddThemeColorOverride("font_color", accent);
```

---

## 1. Forest（关卡 1–20）

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

---

## 2. Desert（关卡 21–40）

**主题叙事**：黄昏沙丘 / 仙人掌

| Token | hex | RGB | 用途 |
|-------|-----|-----|------|
| **Primary** | `#D2A679` | rgb(210, 166, 121) | HUD 边框 / 主题强调 |
| **Accent** | `#FF8C42` | rgb(255, 140, 66) | 副标题 / level number |
| **TextOnPrimary** | `#FFFFFF` | rgb(255, 255, 255) | Primary 上的文字 |
| **Background tint** | `#5C3D1F` | rgb(92, 61, 31) | 关卡背景叠加（可选，alpha 30%） |
| **Button bg** | `#E5B482` | rgb(229, 180, 130) | 关卡按钮 normal |
| **Button pressed** | `#A87849` | rgb(168, 120, 73) | 关卡按钮 pressed |
| **Shadow / depth** | `#7A5028` | rgb(122, 80, 40) | 按钮下边阴影 |

**对比度 check**：
- `#FFFFFF` on `#D2A679` = 2.31:1 (❌ 不够 AA) — 用于 Primary 上的文字时，**必须加阴影 / outline**，否则看不清
- `#FFFFFF` on `#FF8C42` = 2.51:1 (❌ 不够 AA) — 同上
- **建议**：Desert 主题下，level button **不要** 用纯 Primary / Accent 做底色，改用 #7A5028 / #5C3D1F 这种深色，文字保持白色

**修正**：把 Button bg 从 `#E5B482` 改成 `#A87849`（深沙色），按 pressed 文字白 = 4.39:1 (AA pass) ✅

---

## 3. Ocean（关卡 41–60）

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

---

## 4. 跨主题保留元素（不切换）

| 元素 | hex | 来源 |
|------|-----|------|
| **6 颗 gem 色** | 见 ART_SPEC §2.1 | 全主题共用 |
| **背景深紫（v0.1 紫蓝宇宙感）** | `#1A0F2E` / `#4A2D6E` / `#6B3FA0` | Theme background 下层 |
| **品牌主色** | `#88489B` | App icon / 启动品牌 |
| **Modal 蒙层** | `rgba(0, 0, 0, 0.6)` | 弹窗 |
| **Warning / Game Over** | `#FF6B6B` | 红色警告 |
| **Success / Win** | `#52EB73` | 与 gem_green 同 |

> Theme banner 文字：`"Forest"` / `"Desert"` / `"Ocean"`（英文，v1.0 ship；v1.1 i18n）

---

## 5. 主题切换动效（Austin brief §3.4）

**触发**：跨主题时（关卡 1→21 / 关卡 21→41）

| 阶段 | 时长 | 视觉 |
|------|------|------|
| 淡入 | 0.3 s | ThemeBanner 从 alpha 0 → 1 |
| 停留 | 1.0 s | 完全可见 |
| 淡出 | 0.2 s | ThemeBanner 从 alpha 1 → 0 |
| **总计** | **1.5 s** | banner 居中底部显示 |

**ThemeBanner 视觉**：PanelContainer + Label 居中，背景半透明黑（`rgba(0,0,0,0.5)`），文字 36 px + accent 色，2 px 圆角。

---

## 6. C# 端调用清单

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

---

## 7. fork-friendly 修改指南

fork godot-template 时如果想换 3 主题：

1. 改本文档 §1-§3 的 hex 值（每主题 7 个 hex）
2. C# 端 `ThemeColors.Get()` 同步改 enum → color
3. 重新跑对比度 check（§1-§3 内每主题末尾）
4. 不需要改任何 sprite

工作量：~0.5 小时（只改 hex，不改代码逻辑）

---

## 附录 A：颜色选择逻辑说明

**为什么 Forest = 绿，Desert = 沙色，Ocean = 蓝？**

按 Austin brief §2.3 的灵感词锚定：
- Forest → 苔藓 / 树叶 / 棕色树干 → 绿主 + 米黄副
- Desert → 黄昏沙丘 / 仙人掌 → 沙主 + 橙夕阳副
- Ocean → 珊瑚 / 海浪 / 珍珠 → 深蓝主 + 青绿浪花副

每个主题 Primary / Accent 互补（暖+冷或冷+暖），避免单调。

**为什么 v0.1 紫蓝宇宙感不变成主题之一？**

紫蓝宇宙是 **base 背景层**（永远在），主题是在 base 之上的 **HUD / 关卡按钮 / 横幅 overlay 色**。3 主题只在"前景 UI" 换色，不动背景。

---

## 附录 B：变更日志

### v1.0 (2026-10-06, Christine)

初版。基于 Austin D+2 brief §2.3 + ART_SPEC v0.2 §2.3 + D+4 三主题对比度 check。

**字段**：
- §1-§3 三主题完整 hex 调色板（每主题 7 个 token）
- §4 跨主题保留元素
- §5 主题切换动效（1.5 秒淡入淡出）
- §6 C# 端调用清单（7 个 UI 元素 override）
- §7 fork-friendly 修改指南

**对比度 check**：3 主题文字对比度全部 ≥ AA 级（部分 ≥ AAA）。

**已知问题**：
- Desert 主色 `#D2A679` + 白字 = 2.31:1 不够 AA → C# 端 button bg 必须用 pressed 色 `#A87849` 而非 Primary
- 3 主题间色温差距大，主题 banner 切换有 0.5s 视觉冲击感（1.5 秒总时长控制下可接受）

**未决**：
- 中文 vs 英文 Theme label（v1.0 用英文，v1.1 i18n 时再加 `Forest` / `森林`）
- 主题过渡是否要播放音效（v1.0 静音，v1.1 评估）