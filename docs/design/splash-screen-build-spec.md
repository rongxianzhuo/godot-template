# Splash Screen Build Spec — v1.1 (Christine → Jacob Handoff)

> **作者**：Christine (Artist)
> **Date**：2026-10-06 (D+11 周四)
> **状态**：build spec（design → implementation handoff）
> **目标读者**：Jacob（实施 Godot scene）
> **设计参考**：`docs/design/splash-screen-design.md`（设计提案）+ `/shared/christine-splash-production-target.png`（生产视觉目标）

---

## 0. TL;DR

- **方案**：B 全屏沉浸式（v1.1 实施，v1.0 ship 仍用 icon.svg 临时）
- **资产需求**：0 张新图（**复用 `bg_title.png` + `icon.svg`**）
- **新增文件**：`assets/scenes/SplashScreen.tscn`（Godot scene）
- **更新文件**：`export_presets.cfg`（5 个 density 引用 `SplashScreen.tscn`）
- **工作量**：1 天（scene 排版 + 5 density + 测试）

---

## 1. 视觉目标（production reference）

`/shared/christine-splash-production-target.png`（720×1280 RGB, 802 KB）

该图是**生产目标**——Jacob 实施 Godot scene 后应该 match 这个视觉效果。

**关键布局**：
- y=0-30%：上 1/3 留空（仅 cosmic sparkles，bg 已有）
- y=30-50%：icon + Magic Match title + 副标题 + 分隔线
- y=50-90%：bg_title 自带的 ornate lotus mandala（**不动**）
- y=88%：5 颗 loading dot

---

## 2. 精确像素规范（720×1280 canvas）

### 2.1 Z-order (bottom to top)

```
Layer 0  Background      bg_title.png          (existing, 720×1280 RGB)
Layer 1  App Icon        icon.svg @ 280×280   (existing, resized from 1024×1024)
Layer 2  Title text      "Magic Match"         (dynamic, gold + white outline)
Layer 3  Separator       gold horizontal line  (dynamic, 200×2 px)
Layer 4  Subtitle text   "60 Levels · 3 Themes"(dynamic, white)
Layer 5  Loading dots    5 white dots          (dynamic, animated)
```

### 2.2 元素精确位置（1280 height 基准）

| 元素 | x | y | width | height | 颜色 |
|------|---|---|--------|--------|----------|
| Background | 0 | 0 | 720 | 1280 | `bg_title.png` |
| **Icon** | 220 | 102 | 280 | 280 | `icon.svg` (resize from 1024×1024) |
| **Title "Magic Match"** | 文本居中 (x≈120) | 422 | ~480 | ~76 | fill `#FFD955` (gold) + 3 px white outline |
| **Separator line** | 260 | 512 | 200 | 2 | `#FFD955` (gold) |
| **Subtitle "60 Levels · 3 Themes"** | 文本居中 | 538 | ~270 | ~28 | fill `#FFFFFF` |
| **Loading dot 1** | 336 | 1126 | 8 | 8 | `#FFFFFF` |
| **Loading dot 2** | 360 | 1126 | 8 | 8 | `#FFFFFF` |
| **Loading dot 3** | 384 | 1126 | 8 | 8 | `#FFFFFF` |
| **Loading dot 4** | 408 | 1126 | 8 | 8 | `#FFFFFF` |
| **Loading dot 5** | 432 | 1126 | 8 | 8 | `#FFFFFF` |

### 2.3 字体（v1.0 ship 沿用 Godot default，v1.1 TTF 升级后调整）

| 元素 | Font | Size | Weight | Color |
|------|------|------|--------|-------|
| Title "Magic Match" | Godot default | 64 px | Bold | `#FFD955` fill + 3 px `#FFFFFF` outline |
| Subtitle | Godot default | 22 px | Regular | `#FFFFFF` |

**v1.1 升级 TTF 后**：
- Title: Fredoka 或 Baloo 2 (圆体友好)
- Subtitle: Nunito (无衬线圆润)

字体文件位置：`assets/fonts/`（v1.1 加 TTF 后由 Jacob 在 scene 设置）

---

## 3. Godot Scene 排版指南

### 3.1 Scene tree

```
SplashScreen (Control, full rect)
├── TextureRect (Background)
│   ├── texture: bg_title.png
│   └── anchors: full rect (0,0,720,1280)
├── TextureRect (Icon)
│   ├── texture: icon.svg
│   ├── position: (220, 102)
│   └── size: (280, 280)
├── Label (Title)
│   ├── text: "Magic Match"
│   ├── position: centered horizontally
│   └── theme overrides:
│       - font_size: 72 (use Godot default Bold)
│       - font_color: #FFD955
│       - outline_size: 3
│       - outline_color: #FFFFFF
├── ColorRect (Separator)
│   ├── position: (260, 512)
│   └── size: (200, 2)
│       - color: #FFD955
├── Label (Subtitle)
│   ├── text: "60 Levels · 3 Themes"
│   ├── position: centered horizontally
│   └── theme overrides:
│       - font_size: 22
│       - font_color: #FFFFFF
└── HBoxContainer (LoadingDots)
    ├── spacing: 24
    └── 5 × ColorRect (8×8 white dots)
```

### 3.2 锚定建议

```
SplashScreen (Control)
├── anchor_right = 1.0
├── anchor_bottom = 1.0
└── offset_right = 0
└── offset_bottom = 0

TextureRect (Background)
├── anchor_right = 1.0
├── anchor_bottom = 1.0
└── expand_mode = KEEP_ASPECT_COVERED

TextureRect (Icon)
├── position = (220, 102)
└── size = (280, 280)
```

### 3.3 Loading dots 动画（Godot Tween）

```csharp
// 5 个 dot 依次 0.1s 间隔 alpha pulse (0.3 → 1.0 → 0.3)
for (int i = 0; i < 5; i++) {
    var dot = dots[i];
    var tween = CreateTween();
    tween.SetLoops();  // 永久循环
    tween.SetTrans(Tween.TransitionType.Sine);
    tween.SetEase(Tween.EaseType.InOut);
    tween.TweenInterval(i * 0.1);  // stagger
    tween.TweenProperty(dot, "modulate:a", 0.3f, 0.6);
    tween.TweenProperty(dot, "modulate:a", 1.0f, 0.6);
}
```

总动画时长：2.5s/cycle（5 dots × 0.5s + 4 × 0.1s stagger + 0.6s+0.6s alpha）≈3 秒。

**Splash 显示总时长**：实际 loading 完成时间（不是固定值）。完成后 `SceneTree.ChangeSceneToPacked(TitleScreen)`。

---

## 4. export_presets.cfg 更新

```ini
# v1.0 (临时方案)
splash_screens/xxxhdpi_1920x1080="res://icon.svg"
splash_screens/xxhdpi_1440x810="res://icon.svg"
splash_screens/xhdpi_960x540="res://icon.svg"
splash_screens/hdpi_640x360="res://icon.svg"
splash_screens/mdpi_480x270="res://backgrounds/bg_title.png"

# ↓ v1.1 替换为：

splash_screens/xxxhdpi_1920x1080="res://assets/scenes/SplashScreen.tscn"
splash_screens/xxhdpi_1440x810="res://assets/scenes/SplashScreen.tscn"
splash_screens/xhdpi_960x540="res://assets/scenes/SplashScreen.tscn"
splash_screens/hdpi_640x360="res://assets/scenes/SplashScreen.tscn"
splash_screens/mdpi_480x270="res://assets/scenes/SplashScreen.tscn"
```

**密度自适应**：Godot 4 export 引擎自动根据 density 选择对应 size scene，**不**需要手动 scale。

---

## 5. 设计决策 trade-off

### 5.1 为什么不用 bg_game（已经有）。

### 5.2 为什么 subtitle 文本位置 y=538 与 lotus ornamental ring 顶部重叠

**原因**：bg_title 的 lotus 设计上覆盖 y=50-95% 区域。icon + title + subtitle 必须在 y=50% 之前。

**妥协**：subtitle y=538 落在 lotus ornamental ring 顶部（深紫底），白字 #FFFFFF on 深紫 ring 可视性 OK（4.5:1+ 对比度）。

**替代**（如果 Jacob 想更干净）：
- 方案 B-v2：把 bg 改为 bg_game（mandala 是 y=30-65%，icon+text 在 y=70-90%）— 但 bg_game 没有 cosmic lotus 那种 "splash 仪式感"
- 方案 B-v3：拉低 icon + title + subtitle 文本字号（title 56 / subtitle 20）— 让 y=50% 之前能容下所有文字

**Jacob 决定**：按本 build spec 默认实施 B-original（title 64 / subtitle 22 + 轻微 lotus 顶部重叠）；如想更优可在实施时选 B-v2 / B-v3。

---

## 6. 实施 checklist (Jacob)

- [ ] 创建 `assets/scenes/SplashScreen.tscn`（按 §3.1 scene tree）
- [ ] 引用 `assets/backgrounds/bg_title.png`（已 ship, 不动）
- [ ] 引用 `assets/icons/icon.svg`（已 ship, 不动）
- [ ] 实现 5 个 loading dot 动画（按 §3.3 Tween）
- [ ] 测试 Godot 编辑器内 layout match `/shared/christine-splash-production-target.png`
- [ ] 测试 5 个 density 导出（xxxhdpi / xxhdpi / xhdpi / hdpi / mdpi）
- [ ] 测试 cold-start 流程：splash 显示 → 跳转到 TitleScreen
- [ ] 更新 `export_presets.cfg` 5 行（如 §4）
- [ ] smoke test (`make verify`) 全绿
- [ ] commit + push to `art/splash-screen-implementation` branch
- [ ] 通知 Christine 验收

---

## 7. 与原 design doc 关系

- `docs/design/splash-screen-design.md`（v0.1, D+10 上午）：**设计提案**（为什么 / trade-off / 3 方案 ASCII + AI render）
- 本 doc（`docs/design/splash-screen-build-spec.md`，D+11 周四）：**实施 build spec**（像素精确 / Z-order / scene tree / checklist）

两个 doc 配合：design doc 给 Mark / Christine 决策参考；本 doc 给 Jacob 直接实施。

---

## 附录 A：变更日志

### v0.1 (2026-10-06, Christine)

D+11 周四上午 build spec handoff。

**新增**：
- §0 TL;DR
- §1 视觉目标（reference `/shared/christine-splash-production-target.png`）
- §2 精确像素规范（720×1280 + Z-order）
- §3 Godot Scene 排版指南（scene tree + 锚定 + loading dots Tween）
- §4 export_presets.cfg 更新模板
- §5 设计决策 trade-off（subtitle 与 lotus 重叠处理）
- §6 实施 checklist for Jacob
- §7 与 design doc 关系

**未决**：
- subtitle "stocky=0.1/3" 字体 v1.1 升级 TTF 后是否要改字号
- 是否需要新增 loading dots sprite（替代 ColorRect）— 建议不加，用 ColorRect 即可

**与 D+10 splash design doc 关系**：本 doc 是 design 的"实施版"，pixel-precise。

走。