# godot-template — 美术规范 ART_SPEC v0.2

> **状态**：v0.2 Draft — 替换 v0.1 (基于 Mark D+1 拍板：接受现状 gem 风格 + Austin D+2 brief + Christine D+1 评估)
> **范围**：godot-template + Magic Match v1.0（60 关 / 3 主题 / Android-first）
> **负责**：Christine（art direction + asset QA + 配色文档）/ Mark（review + 拍板）
> **生成时间**：2026-10-06 (D+3 周四)
> **依赖**：`ASSET_INTEGRATION.md` v0.1（资产接入流程不变）/ `/shared/austin-christine-brief.md`（v1.0 资产清单来源）

---

## 目录

1. [设计风格（沿用 v0.1 实际，不重画）](#1-设计风格)
2. [调色板](#2-调色板)
3. [字体](#3-字体)
4. [间距 token](#4-间距-token)
5. [暗色 / 亮色模式](#5-暗色--亮色模式)
6. [资产规格总览](#6-资产规格总览)
7. [现有 18 PNG 现状评估](#7-现有-18-png-现状评估)
8. [v1.0 资产清单（新增）](#8-v10-资产清单新增)
9. [组件清单](#9-组件清单)
10. [状态规范](#10-状态规范)
11. [错误状态](#11-错误状态)
12. [动效规范](#12-动效规范)
13. [AI 出图规则（继承 v0.1 §3/§5）](#13-ai-出图规则继承-v01-3§5)
14. [验收 checklist](#14-验收-checklist)
15. [联系表](#15-联系表)
16. [附录 A：fork-friendly color overrides](#附录-afork-friendly-color-overrides)
17. [附录 B：变更日志](#附录-b变更日志)

---

## 1. 设计风格

### 1.1 风格定义（已拍板）

**"Glossy magical gem"** — 半写实宝石 + 卡通光泽 + 紫蓝宇宙感

| 维度 | 现状 |
|------|------|
| 宝石形 | smooth rounded teardrop（v0.1 实际有少量 facet，**接受不修**） |
| 宝石 outline | dark outline（深红 / 深蓝 / 深绿 / 深紫 / 深橙），与 v0.1 spec "white outline" 不同 — **spec 已对齐实际** |
| 宝石高光 | 左上白色硬心月牙 + 右下小白点（每颗位置一致） |
| 宝石 sparkle | 0-4 颗白色 4-point star / 圆点（family 不齐，**已记 P1 项**） |
| 风格参照 | Candy Crush 早期宝石（少"软糖"多"宝石"）、Bejeweled Classic |
| 整体调性 | Casual / F2P，目标玩家：休闲解谜用户 |

### 1.2 风格不接受的方向（避免偏离）

- ❌ 纯像素风（Stardew 风格） — 不适合 AI 出图 + 不适合 Casual
- ❌ 纯扁平（Material Design） — 缺体积感，失"宝石"调性
- ❌ 纯软糖（Candy Crush Soda） — 与现有 6 颗 gem 风格不连贯
- ❌ 写实宝石（Bejeweled Stars） — AI 易崩 + Casual 用户不接受

### 1.3 替代 v0.1 §1.1 的关键差异

| v0.1 spec 写 | v0.2 实际接受 | 决定 |
|---|---|---|
| "solid WHITE 2-3 px outline" | "dark colored outline" | ✅ **更新 spec 对齐 PNG** |
| "smooth rounded teardrop, NO facets" | "teardrop + 少量 facet 内反射" | ✅ **接受半写实** |
| "sparkles 3-4 WHITE 散布" | "0-4 不齐，需 polish" | ⚠️ **P1 polish 项** |

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

### 2.2 UI 色板（v0.2 新增）

| 用途 | hex | 备注 |
|---|---|---|
| **品牌主色（紫）** | `#88489B` | 与 `gem_purple` 同色 = 视觉一致性 |
| **品牌冷色（蓝紫）** | `#5D7FE6` | 🆕 v0.2 新加 — 用于 gem 冷色高光、链接、次级强调 |
| **背景深紫** | `#1A0F2E` | 🆕 v0.2 — `bg_game` 实际色 |
| **背景中紫** | `#4A2D6E` | `bg_title` 渐变中段 |
| **背景亮紫** | `#6B3FA0` | 背景径向渐变中心 |
| **HUD 主文字** | `#FFFFFF` | 100% white |
| **HUD 次级文字** | `#E0E0FF` | 微紫白 |
| **Warning / Game Over** | `#FF6B6B` | EndScreen "You Lost!" |
| **Success / Win** | `#52EB73` | 与 `gem_green` 同 |
| **按钮 normal（已交付）** | 金色 `#FDA915→#FFEC8B` | 沿用 v0.1 |
| **按钮 pressed（已交付）** | 深琥珀 `#D2841F→#8B5A00` | 沿用 v0.1 |
| **Modal 蒙层** | `rgba(0, 0, 0, 0.6)` | popup / 关卡间过渡 |

### 2.3 主题配色（Austin brief §2.3，v1.0 引入）

**实现方式**：用 Godot `AddThemeColorOverride` 在 C# 端按 `CurrentTheme` enum 切换，**不增加任何 sprite**。

| 主题 | 关卡 | 主色 | 强调色 | 灵感 |
|------|-----|------|--------|------|
| **Forest** | 1–20 | `#4A8F4A`（森林绿） | `#F5DEB3`（米色） | 苔藓 / 树叶 / 棕色树干 |
| **Desert** | 21–40 | `#D2A679`（沙色） | `#FF8C42`（橙色夕阳） | 黄昏沙丘 / 仙人掌 |
| **Ocean** | 41–60 | `#1E5F8C`（深海蓝） | `#7FCDCD`（青绿浪花） | 珊瑚 / 海浪 / 珍珠 |

> Theme transition banner：仅跨主题（关卡 1→21 / 21→41）显示 1.5 秒淡入淡出。

### 2.4 Star / Heart / Coin 配色（Austin brief §5）

| icon | 启用色 | 灰禁色 | outline |
|------|--------|--------|---------|
| Star on | `#FFD700`（金黄） | `#A0A0A0`（灰） | `#FFFFFF` 2-3 px |
| Heart on | `#FF4757`（红） | `#A0A0A0`（灰） | `#FFFFFF` 2-3 px |
| Coin | `#FFD700`（金黄） | — | `#FFFFFF` 2-3 px |

> Star / Heart / Coin 都是简单几何形状（不像 sapphire 那样有"水晶面"歧义），AI 出图风险低，但仍跑 rembg alpha 处理。

---

## 3. 字体

### 3.1 v1.0 ship：Godot Default Font

**理由**（Mark D+1 拍板）：
- ✅ 零资产（Godot 4 自带）
- ✅ 零集成（无需 .ttf / .import）
- ✅ 中文 + 英文 OK
- ⚠️ 视觉感偏 generic（但 Casual 赛道 OK）

### 3.2 字号规范（继承 GameScenes.cs 现有用法）

| 用途 | 字号 | 颜色 | 位置 |
|------|------|------|------|
| Title ("Magic Match") | 72 px | `#FFD955`（gold-ish） | TitleScreen 居中 |
| End headline ("You Won!" / "Game Over") | 64 px | Win: `#52EB73` / Lose: `#FF6B6B` | EndScreen 居中 |
| HUD (Score / Moves / Theme) | 28 px | `#FFFFFF` | HUD TopWide |
| Button label | 28 px | 按钮内置默认 | 按钮中央 |
| Level number (按钮内) | 24 px | `#FFFFFF` | 关卡按钮中央 |
| 副信息 (Target / Moves left / Status) | 20 px | `#E0E0FF` | EndScreen 副标题 |

### 3.3 v1.1+ TTF 升级（暂不实施）

候选字体（按备选）：
- **Fredoka** / **Baloo 2** — Title 用（圆体友好）
- **Nunito** / **Quicksand** — HUD / 按钮用（无衬线圆润）
- **Noto Sans CJK** — 中文 fallback（如启用多语言）

---

## 4. 间距 token

### 4.1 token 定义（v0.2 新增）

| token | 值 | 用途 |
|-------|-----|---------|
| `xs`  |  4 px | 装饰元素间距（小图标到文字） |
| `sm`  | 12 px | 元素内部紧凑间距 |
| `md`  | 20 px | 按钮之间垂直间距（Title→Quit） |
| `lg`  | 40 px | 主要元素之间（Title→Play / HUD 字段间） |
| `xl`  | 80 px | 屏与屏之间 / 大分隔 |

### 4.2 现有用法对齐（GameScenes.cs）

```
HUD separation:       40 px  (lg)        ← GameScenes.BuildHud
Title → Play:         40 px  (lg)        ← BuildTitleScreen spacer
Play → Quit:          20 px  (md)
Button min size:    220 × 70 px          ← GameScenes.MakeButton
Board cell:          64 × 64 px          ← Match3BoardView._Ready
Title font:           72 px
HUD font:             28 px
End headline:         64 px
```

### 4.3 v1.0 新增用法（待 Austin / Jacob 实现时遵循）

| 位置 | token |
|------|-------|
| 关卡按钮之间水平间距 | `md` |
| Level select footer 按钮之间 | `lg` |
| Star rating 3 颗星之间 | `sm` |
| Theme banner 内边距 | `md` |
| Modal popup 内边距 | `lg` |

---

## 5. 暗色 / 亮色模式

### 5.1 v1.0：仅暗色基调

- 整体深紫背景（`#1A0F2E` / `#4A2D6E` / `#6B3FA0` 渐变）
- HUD 文字白（`#FFFFFF` / `#E0E0FF`）
- 强调元素（按钮 / star / coin）保持亮色（金色 / 红色 / 紫色）

> **不接受亮色模式**：v1.0 ship 只一套暗色。亮色模式推到 v2.0（如果有用户反馈再评估）。

### 5.2 暗色基调的对比度要求

| 文本/背景组合 | 最小对比度 | 备注 |
|--------------|-----------|------|
| 白字 / 深紫底 | 7:1 (AAA) | HUD / Title |
| 米色（`#F5DEB3`）/ Forest 主色 | 4.5:1 (AA) | Forest 主题副标题 |
| `#E0E0FF` / 深紫底 | 7:1 | 副信息 |

---

## 6. 资产规格总览

### 6.1 通用规格

| 维度 | 规格 | 备注 |
|------|------|------|
| **Canvas 尺寸** | 见各类资产（1024×1024 / 720×1280 / 256×256） | **不偏离** |
| **文件大小上限** | PNG ≤ 800 KB（asset 目录内），SVG ≤ 10 KB | rembg 后 magic verify |
| **透明通道** | gem / button / icon / star / heart / coin 必须 RGBA | background 接受 RGB |
| **PNG 压缩** | Godot import `compress/mode = 0` (Lossless) | 见 `ASSET_INTEGRATION.md` §3.1 |
| **PNG filter** | Linear (Godot 4 默认) | 见 `ASSET_INTEGRATION.md` §3.3 |
| **Mipmaps** | false (2D sprite 不需要) | 同上 |
| **Alpha border fix** | `process/fix_alpha_border = true` | 同上 |
| **Premultiplied alpha** | `false`（卡通 sprite 不需要 premult） | 同上 |

### 6.2 各类资产 canvas / 用途矩阵

| 类别 | canvas | 格式 | 透明 | 用途 | 备注 |
|------|--------|------|------|------|------|
| 单 gem | 1024×1024 | RGBA | ✅ | 棋盘宝石（运行时） | Lossless import |
| 单 button | 1024×1024 | RGBA | ✅ | UI 按钮（v1.0 启用 TextureButton） | 同上 |
| 单 background | 720×1280 | RGB | ❌ | 标题 / 游戏 / 关卡选择背景 | 同上 |
| App icon | 1024×1024 | SVG / PNG | ✅ | 启动器图标 + splash | 🆕 v0.2 由我手写 SVG（替换原占位） |
| Splash | = App icon | SVG | — | 现有 export_presets 用 icon.svg | v1.0 不单独做 splash |
| Star icon | 256×256 | RGBA | ✅ | 评分（3 颗 / 组） | 🆕 v1.0 P0 |
| Heart icon | 128×128 | RGBA | ✅ | 续命 UI | 🆕 v1.0 P1 |
| Coin icon | 128×128 | RGBA | ✅ | 货币 | 🆕 v1.0 P1 |
| Lock icon | 256×256 | RGBA | ✅ | 锁定关卡 | 🆕 v1.0 P1 |
| Level select bg | 720×1280 | RGB | ❌ | 关卡选择屏背景 | 🆕 v1.0 P0 |
| Btn Play | 256×256 | RGBA | ✅ | 主按钮（关卡选择） | 🆕 v1.0 P1 |

### 6.3 File size budget（粗略）

| 类别 | 单文件 | 总预算（v1.0） |
|------|--------|----------------|
| Gem (×6 + _alpha ×6) | 600 KB | ~7 MB（12 文件） |
| Button (×2 + _alpha ×2) | 600 KB | ~2.4 MB（4 文件） |
| Background (×3) | 500 KB | ~1.5 MB（3 文件） |
| Star (on + off) | 100 KB | 200 KB |
| Heart (1) | 50 KB | 50 KB |
| Coin (1) | 50 KB | 50 KB |
| Lock (1) | 100 KB | 100 KB |
| Icon SVG (1) | 10 KB | 10 KB |
| **总计（v1.0 估算）** | — | **~12 MB**（APK 总体积 110 MB 中美术 < 1%） |

---

## 7. 现有 18 PNG 现状评估

> 评估人：Christine (D+1 用 PIL 视觉检查 + 文件分析)

### 7.1 总览

| 指标 | 值 |
|------|-----|
| 文件总数 | 18（12 gem + 4 button + 2 background）+ 18 `.png.import` sidecar |
| 总大小 | ~7 MB |
| 平均评分 | **7.4 / 10** |
| 可 ship（P0 / P0.5） | 17 / 18（94%） |
| 需 polish（P1） | 4 / 18（22%） |

### 7.2 逐文件评分（10 分制）

| # | 资产 | 评分 | 状态 | 改进项 |
|---|------|------|------|--------|
| 1 | `gem_red.png` + `_alpha` | 7.0 | ✅ teardrop + 高光 + 渐变扎实；⚠️ 颜色偏 orange-red（应是 crimson）；❌ 无 sparkles | P1: 颜色微调 + 加 3-4 颗 sparkle |
| 2 | `gem_orange.png` + `_alpha` | 7.0 | ✅ 渐变扎实；⚠️ 颜色好；❌ 无 sparkles | P1: 加 3-4 颗 sparkle |
| 3 | `gem_yellow.png` + `_alpha` | 7.5 | ✅ 颜色饱和；⚠️ 高光偏弱；❌ 无 sparkles | P1: 加 sparkle + 高光 |
| 4 | `gem_green.png` + `_alpha` | 7.5 | ✅ 森林绿 + 2 颗 sparkle；⚠️ outline 偏深（v2 已修 halo） | OK |
| 5 | `gem_blue.png` + `_alpha` | **8.0** | ✅ 最强一颗；蓝色 sapphire 风；4 颗 sparkle | OK |
| 6 | `gem_purple.png` + `_alpha` | 7.0 | ✅ 紫色好；4 颗 sparkle；⚠️ 颜色略 hot-pink；⚠️ 底部阴影 family 不齐 | P1: 颜色微调（→ deep amethyst） |
| 7 | `button_normal.png` + `_alpha` | 6.0 | ⚠️ 底部紫色 ring bug（v0.1 spec 已记） | P1: 紫色 ring 修 |
| 8 | `button_pressed.png` + `_alpha` | 7.5 | ✅ 深琥珀 + 白色 outline；⚠️ 深度感弱 | OK |
| 9 | `bg_game.png` | 6.0 | ⚠️ 太暗（`#1A0F2E` 实际色比 spec 说的"深紫渐变"更深）；⚠️ mandala 偏弱；⚠️ 星点稀疏 | P1: mandala 强化 |
| 10 | `bg_title.png` | **8.5** | ✅ 最强一张；紫→深蓝渐变 + 八角 starburst 像魔法阵 | OK |

### 7.3 spec vs 实际差异总结（v0.1 → v0.2 关闭）

| 字段 | v0.1 spec 说 | 实际 | v0.2 处理 |
|------|-------------|------|-----------|
| Gem outline | white 2-3 px | dark colored | ✅ 更新 spec 对齐实际 |
| Gem 形 | NO facets | 少量 facet | ✅ 接受半写实 |
| Gem sparkles | 3-4 white 散布 | 0-4 不齐 | ⚠️ P1 polish |
| Button normal | 金渐变 | 金 + 紫色 ring | ⚠️ P1 polish |
| bg_game mandala | 中心 mandala | 单层偏弱 | ⚠️ P1 polish |
| bg_title starburst | 中心 starburst | 八角清晰 | ✅ 完全匹配 |

---

## 8. v1.0 资产清单（新增）

> 来源：`/shared/austin-christine-brief.md` §2。详见 brief 完整版。

### 8.1 P0（阻塞 v1.0）

| # | 资产 | canvas | 格式 | 透明 | 风格约束 |
|---|------|--------|------|------|---------|
| 1 | `ui/level_select_bg.png` | 720×1280 | RGB | ❌ | 沿用 v0.1 紫蓝宇宙感 + 主体留出 Level Grid 区域 |
| 2 | `ui/star_on.png` | 256×256 | RGBA | ✅ | 5 角金色实心 + 白 outline 2-3 px + 1 颗 sparkle（与 gem 风格一致） |
| 3 | `ui/star_off.png` | 256×256 | RGBA | ✅ | 5 角灰色实心 + 白 outline 2-3 px（与 star_on 形一致，仅色改） |

### 8.2 P1（v1.0 强烈推荐，不阻塞）

| # | 资产 | canvas | 格式 | 透明 | 风格约束 |
|---|------|--------|------|------|---------|
| 4 | `ui/btn_play.png` | 256×256 | RGBA | ✅ | 金色实心 + 白 outline + ▶ 三角（绿色）+ 白高光 |
| 5 | `ui/btn_locked.png` | 256×256 | RGBA | ✅ | 灰锁实心 + 白 outline（与 btn_play 形一致） |
| 6 | `ui/hud_heart.png` | 128×128 | RGBA | ✅ | 红心 + 白 outline 2-3 px |
| 7 | `ui/coin.png` | 128×128 | RGBA | ✅ | 金色硬币 + 白 outline + 1 颗 sparkle |

### 8.3 P2（v1.0 不做，列在这里做规划）

| # | 资产 | 用途 |
|---|------|------|
| 8 | `ui/world_map_*.png` (大背景) | v2.0 升级世界地图 |
| 9 | `ui/map_node_*.png` | v2.0 地图节点 sprite |
| 10 | `ui/path_line_*.png` | v2.0 地图路径连线 |

### 8.4 v1.0 美术工作量估算（来自 Austin brief §7）

| 项目 | 工作量 | 备注 |
|------|--------|------|
| 3 张 P0 sprite（level_select_bg + star_on + star_off） | 0.5–1 天 | 含 rembg + QA |
| 4 张 P1 sprite（btn_play + btn_locked + heart + coin） | 0.5–1 天 | 含 rembg + QA |
| 3 套主题配色文档（CSS-style color override table） | 0.5 天 | **不算 C# 端实现** |
| **v1.0 美术总工作量** | **1–2 天** | 比 world map 方案省 50% |

---

## 9. 组件清单

> 每个组件 = 一个可复用的视觉单元。代码端用 C# 程序化构建（per AGENTS.md），美术端给规格。

### 9.1 通用组件

| 组件 | 视觉规格 | 字号 | 颜色 | 数据来源 |
|------|---------|------|------|---------|
| **Button (normal)** | button_normal_alpha.png（220×70 px min） | 28 px | 默认 | `GameScenes.MakeButton` |
| **Button (pressed)** | button_pressed_alpha.png | 28 px | 默认 | 同上 |
| **Button (disabled)** | 50% alpha + 灰 | 28 px | `#A0A0A0` | v1.0 加 |
| **Score Label** | HUD 左 | 28 px | `#FFFFFF` | `GameScenes.BuildHud` |
| **Moves Label** | HUD 中 | 28 px | `#FFFFFF` | 同上 |
| **Theme Label** | HUD 右（v1.0 新增） | 28 px | 主题强调色 | Austin brief §3.3 |
| **Pause Button** | HUD 远右（v1.0 新增） | icon | `#FFFFFF` | 同上 |
| **Star Icon (on)** | star_on.png | — | `#FFD700` + 白 outline | Austin brief §2.1 |
| **Star Icon (off)** | star_off.png | — | `#A0A0A0` + 白 outline | 同上 |
| **Heart Icon** | hud_heart.png | — | `#FF4757` + 白 outline | 同上 |
| **Coin Icon** | coin.png | — | `#FFD700` + 白 outline | 同上 |
| **Lock Icon** | btn_locked.png | — | `#A0A0A0` + 白 outline | 同上 |

### 9.2 屏级别组件

| 屏 | 子组件 | 视觉规格 |
|---|--------|---------|
| **TitleScreen** | Background (bg_title) + Title (72px) + VBox (Play + Quit) | 见 GameScenes.BuildTitleScreen |
| **GameScreen** | Background (bg_game) + HUD (TopWide) + Board (Center) | 见 Match3Feature.RebuildGameScreen |
| **EndScreen** | Background (bg_game) + StarRating (3 star) + ScoreLabel + TargetLabel + StatusLabel + ContinueButton + MainMenuButton + ContinueAdButton | 扩展 GameScenes.BuildEndScreen（Austin brief §3.2） |
| **LevelSelectScreen** 🆕 | Background (level_select_bg) + Header (Title + CoinDisplay + HeartsDisplay) + LevelGrid (5×4 = 20 关/主题) + Footer (Back + Shop) | 新建 LevelSelectScreen.cs（Austin brief §3.1） |
| **ThemeBanner** 🆕 | PanelContainer (BottomCenter) + ThemeLabel ("Forest" / "Desert" / "Ocean") | 跨主题时显示 1.5 秒淡入淡出 |
| **Modal (Pause / Confirm)** 🆕 | 半透明黑蒙层 + 居中 VBox | v1.0 加 |

---

## 10. 状态规范

### 10.1 按钮状态

| 状态 | 视觉 | 触发 |
|------|------|------|
| **Normal** | button_normal_alpha + 全色 | 鼠标未接触 |
| **Hover** | Normal + 10% 亮度提升（ColorRect modulate） | 鼠标进入 |
| **Pressed** | button_pressed_alpha（v0.1 已交付） | 鼠标按下 |
| **Disabled** | 50% alpha + 灰 | 不可交互 |
| **Focused** | Normal + 白 outline 2 px（额外描边） | 键盘 focus（v1.0 可选） |

### 10.2 Star 状态

| 状态 | 视觉 |
|------|------|
| **On（达成）** | star_on.png（5 角金色） |
| **Off（未达成）** | star_off.png（5 角灰色，相同形） |

### 10.3 关卡状态

| 状态 | 视觉 | 触发 |
|------|------|------|
| **Unlocked** | 按钮 normal + 关卡数字 + stars | 已解锁可玩 |
| **Locked** | btn_locked.png 覆盖整个按钮 | 未解锁 |
| **Current** | 按钮 normal + 边框金色脉冲 | 当前关（v1.0 加） |

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

### 12.1 v1.0 ship 状态（基于 v0.1 当前）

| 元素 | v0.1 状态 | v1.0 计划 |
|------|-----------|-----------|
| 匹配清除 | ❌ 直接刷 sprite | ✅ Tween ScaleFadeOut 0.3s |
| 宝石下落 | ❌ 直接调 sheet | ✅ Tween Position 0.4s ease-out |
| 按钮按下 | ✅ Godot 默认 | 沿用 |
| 屏切换 | ❌ 直接 SwapScreen | ✅ Tween FadeIn/Out 0.2s |
| Star 评分 | ❌ 一次性显示 | ✅ 3 张依次 StarPop 0.1s 间隔 |

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

## 13. AI 出图规则（继承 v0.1 §3/§5）

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

### 13.3 新增 icon 类别（star / heart / coin）的特殊规则

- 都是简单几何形状，AI 出图风险低
- 仍跑 icon_alpha.py rembg
- star 5 角形 + 金色 + 白 outline（与 gem family 一致）
- heart 红心形 + 白 outline
- coin 圆形 + 金色 + 中央数字 / 图案（v1.0 决定是否加图案）

### 13.4 rembg 处理命令模板

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

- [ ] canvas 尺寸正确（按 §6.2 矩阵）
- [ ] RGBA（gem / button / icon / star / heart / coin）/ RGB（bg）
- [ ] 文件大小 ≤ 上限（§6.1）
- [ ] PNG 8-byte magic verify（`89504E47` 不是 `FFD8FFE0`）
- [ ] partial ratio 1.2-2.5%（硬边干净）

### 14.2 视觉级（参考 D+1 评分维度）

- [ ] 形正确（teardrop / star 5-角 / heart / coin / bg 渐变方向）
- [ ] 高光位置一致（gem 左上 / star 上方 / heart 中央）
- [ ] outline 风格统一（dark colored 2-3 px，非 white — 接受 v0.2 实际）
- [ ] sparkles 数量 3-4 颗（gem family 一致）
- [ ] 颜色在 §2.1 / §2.4 hex ±5 内

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

## 附录 A：fork-friendly color overrides

> fork godot-template 时改这些字段就能换主题 / 换品牌色。

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

见 §2.3 + 主题 enum 在 `src/Features/Match3/Theme.cs`（v1.0 由 Jacob 加）。

---

## 附录 B：变更日志

### v0.2 (2026-10-06, Christine)

基于 Mark D+1 拍板 + Austin D+2 brief + D+1 评估重写。

**变更**：
- §1 设计风格：从"white outline + NO facets"改为"dark outline + 半写实 facet"（spec 对齐实际）
- §2.1 Gem 色：保留锁定 + anchor 词精简
- §2.2 UI 色：新增 `#5D7FE6` 冷色 + `#1A0F2E` 背景深紫
- §2.3 主题配色：新增（Forest / Desert / Ocean）
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
**v0.2 起作废**，保留作历史参考。