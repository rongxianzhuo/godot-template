# Splash Screen Design — v1.1 Proposal

> **作者**：Christine (Artist)
> **Date**：2026-10-06 (D+10 周三)
> **状态**：v1.1 design proposal（v1.0 ship 仍用 icon.svg 临时方案）
> **推荐方案**：**方案 B**（全屏沉浸式，复用 bg_title）
> **视觉 sample**：见 `/shared/christine-splash-samples.md`（AI 渲染）

---

## 0. 现状（v1.0 临时方案）

`export_presets.cfg` 当前配置：
```ini
splash_screens/xxxhdpi_1920x1080="res://icon.svg"
splash_screens/xxhdpi_1440x810="res://icon.svg"
splash_screens/xhdpi_960x540="res://icon.svg"
splash_screens/hdpi_640x360="res://icon.svg"
splash_screens/mdpi_480x270="res://backgrounds/bg_title.png"   # ← 这一行是临时错的
```

**问题**：
- 5 个 density 都是 icon.svg，无品牌感
- mdpi 480×270 引用的是 bg_title.png 不是 splash asset
- v1.1 应统一替换为正式 splash screen

---

## 1. 屏幕规格

| 维度 | 值 |
|------|-----|
| Portrait canvas | 720 × 1280（设计尺寸） |
| 实际 density | 270×480 → 1920×1080 五个 size |
| 安全区（vertical） | 中央 600×800（避开顶部 status bar + 底部 nav bar） |
| 加载时长 | 1.5-2.5s（Android cold start 标准） |

---

## 2. 三个方案

### 2.1 方案 A：极简居中（Minimal Centered）

```
┌──────────────────────────┐
│                          │ ← y=0%   status bar 透明
│                          │
│                          │
│                          │
│        ┌──────┐          │ ← y=35%  icon 256×256
│        │      │          │
│        │ ICON │          │
│        │      │          │
│        └──────┘          │
│                          │
│      Magic Match         │ ← y=55%  标题 64 px
│                          │
│        ◌ ◌ ◌              │ ← y=75%  loading spinner
│                          │
│                          │ ← y=100% nav bar 透明
└──────────────────────────┘
```

| 属性 | 值 |
|------|-----|
| 背景 | 纯色 `#1A0F2E` |
| icon | 居中 256×256 |
| 标题 | Magic Match 64 px 白色 + gold `#FFD955` 下划线 |
| loading | 3 颗 orbiting sparkles |
| 优点 | 简单 / 跨语言 / 包大小 0（vector icon） |
| 缺点 | 缺品牌感 / 与 cosmic purple bg 不一致 / 太平淡 |

### 2.2 方案 B：全屏沉浸式（Full-Screen Immersive）⭐ **推荐**

```
┌──────────────────────────┐
│ ·  ·    ✦         ·      │ ← y=0%   cosmic sparkles
│      ·    ✦  ·           │
│  ·      ·       ·        │
│                          │
│                          │
│      ┌──────────┐        │ ← y=30%
│      │          │        │
│      │   ICON   │        │   App icon 320×320 (略放大)
│      │          │        │
│      └──────────┘        │
│                          │
│       Magic Match        │ ← y=55%   标题 72 px gold #FFD955 + 白 outline
│                          │
│   ──────────────────     │ ← y=60%   分隔线（细 gold）
│                          │
│  60 Levels · 3 Themes    │ ← y=65%   副标题 24 px 白
│                          │
│                          │
│        ◌ ◌ ◌ ◌ ◌          │ ← y=85%   loading (5 sparkles)
│                          │
│  ·    ✦    ·     ·  ✦    │ ← y=95%   cosmic sparkles
└──────────────────────────┘
```

| 属性 | 值 |
|------|-----|
| 背景 | `bg_title.png`（**复用 D+7 ornate lotus**） |
| icon | 居中 320×320 |
| 标题 | Magic Match 72 px gold + 白 outline |
| 副标题 | `60 Levels · 3 Themes` 24 px 白（**trust signal**） |
| loading | 5 颗 sparkles |
| 优点 | 强品牌感 / **零新美术资产** / loading 与 gem family 一致 |
| 缺点 | bg_title 已用于 TitleScreen，splash 用同一张图 = 视觉重复（可接受：splash 1.5s 玩家不会注意） |

### 2.3 方案 C：动画魔法显现（Animated Magic Reveal）

```
┌──────────────────────────┐
│          ✦               │ ← y=15%   sparkle 出现 0.2s
│        ·     ✦           │
│        ┌──────┐          │ ← y=30%   icon 旋转 + scale 弹入 0.4s
│        │ ICON │          │
│        └──────┘          │
│       Magic Match        │ ← y=55%   文字 fade-in 0.3s
│                          │
│  ╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱╱   │ ← y=70%   loading bar = 真实进度
│  ╱╱╱╱╱╱╱╱╱╱╱╱╱╱          │     (横向进度条 1.5s)
│                          │
│  ·    ✦     ·    ✦       │ ← y=95%   sparkles fade-in 0.5s
└──────────────────────────┘
```

| 属性 | 值 |
|------|-----|
| 背景 | 与 B 同（复用 bg_title） |
| icon | 旋转 + scale 弹入 0.4s |
| 标题 | fade-in 0.3s |
| loading | **真实进度条**（横向 1.5s） |
| 优点 | 强品牌感 / 真实进度反馈 / 动画 polish |
| 缺点 | Godot Tween × 4 + 真实进度条 = 0.5 天开发 / v1.0 ship 太赶 |

---

## 3. 推荐方案 B（理由）

1. **零新增资产** = 复用 v1.0 已 ship 的 `bg_title.png`
2. **强品牌感** = cosmic purple + lotus 与 v1.0 全游戏一致
3. **跨语言** = 副标题 `"60 Levels · 3 Themes"` 是数字 + 英文，i18n 安全
4. **实现简单** = 仅 1 张 PNG + Godot scene 排版（0.25 天 vs 方案 C 0.5 天）
5. **v1.1 polish** 可升级到方案 C（动画）

**v1.0 ship 决策保持**：仍用 icon.svg（per Mark D+5 决策）

**v1.1 ship 计划**：方案 B 替换 icon.svg（zero new asset cost）

**v2.0 polish 可选**：方案 C 升级动画（如玩家反馈 splash 太短）

---

## 4. 视觉 sample（AI 渲染 / 不是 production）

`/shared/christine-splash-samples.md` 含 1 张 AI 渲染 sample：

- **方案 B 渲染**（720×1280 RGB，202 KB）
  - cosmic purple 背景
  - 居中 icon（multi-gem cluster，与 v1 icon 概念相近 — production 应用 v1 icon）
  - "Magic Match" gold 标题
  - "60 Levels · 3 Themes" 白副标题
  - 顶部 + 底部 sparkles

**注意**：sample 是 AI 渲染，**非 production asset**。production splash 用 bg_title.png + v1 icon.svg 组合。

---

## 5. 实现计划（v1.1）

### 5.1 资产需求

| 资产 | 来源 | 改动 |
|------|------|------|
| 背景 PNG | `bg_title.png`（已 ship） | **无改动** |
| App icon | `icon.svg`（已 ship） | **无改动** |
| 加载 sparkle | 不需要（bg_title 自带 sparkles） | **无改动** |
| Godot scene | 新建 `assets/scenes/SplashScreen.tscn` | **新增** |

### 5.2 export_presets.cfg 更新（v1.1）

```ini
splash_screens/xxxhdpi_1920x1080="res://assets/scenes/SplashScreen.tscn"
splash_screens/xxhdpi_1440x810="res://assets/scenes/SplashScreen.tscn"
splash_screens/xhdpi_960x540="res://assets/scenes/SplashScreen.tscn"
splash_screens/hdpi_640x360="res://assets/scenes/SplashScreen.tscn"
splash_screens/mdpi_480x270="res://assets/scenes/SplashScreen.tscn"
```

### 5.3 工作量（v1.1 Jacob 实现）

| 任务 | 工时 |
|------|------|
| Godot scene 排版（bg + icon + text + sparkles） | 0.25 天 |
| 调整密度 × 5 个 size | 0.5 天 |
| 测试冷启动 / loading 流程 | 0.25 天 |
| **总计** | **1 天** |

---

## 6. v1.0 ship 决策（保持原状）

**v1.0 不实现 splash**。仍用 icon.svg 临时方案，原因：

- v1.0 ship timebox 紧（Week 6 内）
- icon.svg 是 OK 临时方案（玩家不会记 splash 1.5s 内容）
- splash polish 是 nice-to-have，不是 ship-blocker

**v1.1 (预计 v1.0 ship 后 2-4 周) 实施本方案 B**。

---

## 附录 A：变更日志

### v0.1 (2026-10-06, Christine)

D+10 草图阶段。3 方案 ASCII + 方案 B AI 渲染 sample。

**决策**：
- 推荐方案 B（v1.1 实施）
- v1.0 ship 仍用 icon.svg 临时方案

**未决**：
- v1.1 实施时间（Mark / Jacob 拍板）
- 副标题文案 i18n 准备（v2.0）
- 方案 C 动画 v2.0 polish 决策（需玩家反馈 trigger）

---

**设计 doc 完。下一步：D+10 下午 v1.0 release notes。**