# godot-template — 美术规范 ART_SPEC v0.1

> **状态**：v0.1 Draft (基于 Magic Match ART_SPEC v1.3 frozen + Founder v1.4 anti-feather 规则)
> **范围**：godot-template 项目的 6 颗 gem + 2 张 button + 2 张背景美术
> **负责**：Beverly (art direction + asset QA) / Shawn (review)
> **生成时间**：2026-09-18

---

## 1. 视觉系统

### 1.1 主题 (沿用 v1.3 frozen)

- cartoon / glossy / magical / sparkles / vibrant / clean line art / soft rounded edges
- 宝石形：smooth rounded teardrop, **无 facets, 无 gemstone cuts, 无 crystal facets**
- 描边：solid WHITE 2-3 px (NOT dark / black outline — image-01 不理, 见 §5)
- sparkle：3-4 颗 tiny WHITE 散布 (实际 image-01 偶发同色, 见 §5)
- 风格：match-3 mobile puzzle / Candy Crush / jelly bean candy

### 1.2 6 色板 (hex 严格)

| gem_type | 颜色 | hex (spec) | anchor 词 | anti-drift |
|----------|------|-----------|-----------|-----------|
| 0 | red    | `#D61420` | DEEP SATURATED CRIMSON | NOT pink NOT magenta NOT orange |
| 1 | orange | `#FDA915` | VIBRANT AMBER TANGERINE | NOT yellow NOT red NOT pale |
| 2 | yellow | `#FDEA1C` | GOLDEN SUNSHINE | NOT lemon NOT cream NOT pale NOT lime |
| 3 | green  | `#52EB73` | DARK FOREST PINE-TREE GREEN | NOT lime NOT yellow-green NOT olive |
| 4 | blue   | `#2478D6` | BRIGHT MEDIUM SAPPHIRE | NOT cyan NOT navy NOT teal |
| 5 | purple | `#88489B` | DEEP AMETHYST GRAPE | NOT pink NOT magenta NOT lavender |

> ⚠️ **godot-template 项目使用 C# (.NET 9) + Godot 4.7.2 Mono** — 集成到 Gem.cs / Board.cs 时 hex 严格按本表。gem_type 索引锁定 0-5。

### 1.3 Family 设计语言

- 单 gem canvas：1024×1024 (image-01 默认 1:1)
- 单 button canvas：1024×1024 (便于 Juan 任意 aspect 缩放)
- 背景 canvas：720×1280 (image-01 默认 9:16, 9:16 竖屏 match-3)
- 6 颗 gem 同 teardrop 形 + 同 highlight 位置（左上）+ 同 outline 风格
- 风格：candy / match-3 / jelly bean / NOT gemstone / NOT crystalline / NOT realistic

---

## 2. 已交付资产清单 (v0.1)

### 2.1 Base 资产 (10 张 RGB)

| 资产 | staging 路径 | size | 出图版本 | 评估 |
|------|--------------|------|----------|------|
| gem_red         | `gems/gem_red.png`          | 463 KB | v1  | ✅ teardrop + white highlight + 2 sparkles；⚠️ color 略偏 orange-red |
| gem_orange      | `gems/gem_orange.png`       | 390 KB | v1  | ✅ teardrop + amber gradient + 2 sparkles |
| gem_yellow      | `gems/gem_yellow.png`       | 376 KB | v1  | ✅ teardrop + golden + 6 pale sparkles |
| gem_green       | `gems/gem_green.png`        | 400 KB | v2  | ✅ teardrop + forest green + 5 white sparkles；v1 有 glow halo (已修) |
| gem_blue        | `gems/gem_blue.png`         | 446 KB | v2  | ✅ teardrop + bright blue + 4 white sparkles；v1 有 crystal facets (已修) |
| gem_purple      | `gems/gem_purple.png`       | 334 KB | v1  | ✅ teardrop + deep purple + 8 white sparkles；⚠️ color 略 hot-pink-purple |
| button_normal   | `ui/button_normal.png`      | 425 KB | v1  | ✅ rounded square + 金渐变 + highlight；⚠️ image-01 误把 "purple outline" 渲染在底部成圆环 |
| button_pressed  | `ui/button_pressed.png`     | 415 KB | v1  | ✅ rounded square + 更深金 + 白色 outline 清晰；⚠️ pressed 深度感仅靠颜色差 |
| bg_game         | `backgrounds/bg_game.png`   | 217 KB | v1  | ✅ 720×1280 深紫渐变 + 中心 mandala + 星点 |
| bg_title        | `backgrounds/bg_title.png`  | 496 KB | v1  | ✅ 720×1280 紫→蓝渐变 + 中心 starburst + 星点 |

> **最终路径** (Robin cp 后)：`/root/godot-template/assets/{gems,ui,backgrounds}/<asset>.png`
> **当前 staging 路径**：`/root/_art_staging/godot-template/{gems,ui,backgrounds}/<asset>.png`

### 2.2 Alpha 输出 (8 张 RGBA, via icon_alpha.py)

| 资产 | staging 路径 | size | **partial ratio** | 处理时间 |
|------|--------------|------|-------------------|----------|
| gem_red_alpha         | `gems/gem_red_alpha.png`         | 587 KB | 2.45% | 4.87s |
| gem_orange_alpha      | `gems/gem_orange_alpha.png`      | 407 KB | 1.78% | 2.32s |
| gem_yellow_alpha      | `gems/gem_yellow_alpha.png`      | 423 KB | 1.55% | 2.96s |
| gem_green_alpha       | `gems/gem_green_alpha.png`       | 467 KB | 1.43% | 3.22s |
| gem_blue_alpha        | `gems/gem_blue_alpha.png`        | 607 KB | 1.60% | 3.15s |
| gem_purple_alpha      | `gems/gem_purple_alpha.png`      | 472 KB | **1.20%** | 2.28s |
| button_normal_alpha   | `ui/button_normal_alpha.png`     | 527 KB | 2.15% | 4.54s |
| button_pressed_alpha  | `ui/button_pressed_alpha.png`    | 573 KB | 1.96% | 3.33s |
| **AVG**               | —                              | —      | **1.77%** | 3.33s |

> partial ratio = (0 < alpha < 255) / total pixels。**1.2-2.5% 表示硬边干净**（商业 icon 级别）。所有 alpha PNG 通过 8-byte magic verify (无 JPEG-as-PNG bug)。

---

## 3. v1.4 防羽化规则 (Founder 12h 前指示)

### 3.1 严禁出现 (prompt 中**否定短语**或**避免**)

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
| `glowing` / `glowing X` | 任何带 "glowing" 的短语 (包括 "magical glowing portal") |

### 3.2 必须 (prompt 肯定)

- ✅ **硬边** (hard edges)
- ✅ **明确轮廓** (clear silhouette / defined outline)
- ✅ **实心** (solid fill)
- ✅ **solid pure WHITE 背景** (rembg 后处理前提)
- ✅ 主体居中、四周留白 5-15% (rembg 边缘保留)

### 3.3 grep 验证流程

每个 prompt 写完后，必须跑：

```python
import re
def strip_negations(t):
    t = re.sub(r'\bNO\s+[^,;.]*', '', t, flags=re.IGNORECASE)
    t = re.sub(r'\bNOT\s+[^,;.]*', '', t, flags=re.IGNORECASE)
    return t
# 在 strip 后的文本里 grep forbidden words
```

10/10 prompt 本轮已通过验证 ✅。

---

## 4. icon_alpha.py 处理记录

### 4.1 命令模板

```bash
# 6 颗 gem 批量 (preset gem)
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

# 单张 (preset icon / soft / 显式 mode 也行)
python3 /root/icon_alpha.py path/to/asset.png --preset icon --model u2netp
```

### 4.2 实现要点

- 后端：**rembg 2.0.84** + **onnxruntime 1.30.0** + **u2netp (4.57MB)**
- 默认 mode：`rembg_matting` (alpha_matting=True, post_process_mask=True)
- 性能：~3s/image 单线程；avg 3.33s/asset
- partial ratio：1.2-2.5% (硬边干净，比 v1.2 bake_alpha 法 50% partial 提升 ~30x)
- 8-byte PNG magic verify 防 JPEG-as-PNG download bug

### 4.3 Presets

| preset | mode | alpha_matting | post_process_mask | 适用 |
|--------|------|---------------|-------------------|------|
| `gem`    | rembg_matting  | True  | True  | 单个 subject，干净边 |
| `button` | rembg_matting  | True  | True  | UI button，halo 清除 |
| `icon`   | rembg_matting  | True  | True  | 通用 icon |
| `soft`   | rembg_default  | False | False | 软边 (如果需要) |

### 4.4 工具位置

| 路径 | 版本 | 状态 |
|------|------|------|
| `/shared/godot-template-tools/icon_alpha.py` | Robin v0.3 (待 install) | **生产** |
| `/root/icon_alpha.py` (239 行) | 本地备份 | 当前 staging 使用 |

---

## 5. 已知 image-01 限制 (沿用 v1.0 spec §10)

| # | 限制 | 影响 | 缓解 |
|---|------|------|------|
| 1 | 不理 transparent background | 输出必带 bg | 显式 `"solid pure WHITE background"` |
| 2 | 不理 white outline | gem/button 自带深色 outline | rembg 后续处理, design 接受 |
| 3 | 不理 white sparkles | sparkles 颜色跟主体 | 接受 family drift |
| 4 | 不理 "no text" on button | button 偶发文字 | 集成时 PIL 后处理去字 |
| 5 | sapphire/emerald 触发 crystal facets | blue/green 失真 | 加 "jelly bean / gummy candy / NOT crystalline" 锚点 (gem_blue/green v2 已修) |
| 6 | 1:1 默认输出 = 1024×1024 (非 512×512) | 高分辨率 | Juan scale-down 处理 |
| 7 | 9:16 默认输出 = 720×1280 | 竖屏 bg | 完美匹配 |
| 8 | `output_directory` 参数无效 | 输出位置不可控 | 手动 cp 到 staging |
| 9 | 下载链路偶发 JPEG-as-PNG | magic bytes 错 (`FFD8FFE0` 替代 `89504E47`) | 8-byte verify + `Image.open(p).save(p, format="PNG")` 修复 |
| 10 | bria-rmbg 1GB 模型 OOM | 容器内存不够 | 强制 `--model u2netp` (4.57MB) |

---

## 6. 后续待办 (v0.2)

### 6.1 集成 (P0, 阻塞 ship)

- [ ] **Robin**: cp staging → `/root/godot-template/assets/{gems,ui,backgrounds}/`
- [ ] **Robin**: `install_toolchain_csharp.sh` 补 Python + rembg + onnxruntime 安装步骤
- [ ] **Framework Engineer**: 创建 `project.godot` + `.csproj` + Main/Game/EndScreen 3 场景

### 6.2 美术 polish (P1)

- [ ] shop_icon.png 集成 (从 GameFramework demo 拷, 暂存待 verify)
- [ ] panel_popup.png (P1, 待 prompt)
- [ ] gem_red color drift → v3 (如果 Founder 要 crimson 而非 orange-red)
- [ ] gem_purple color drift → v3 (如果 Founder 要 deep amethyst 而非 hot-pink)
- [ ] button_normal 底部紫色 ring → v2 (如果 Founder 不接受)
- [ ] button_pressed 深度感 → v2 (如果 Founder 不接受)

### 6.3 视觉 QA (支持性, 非阻塞)

- [ ] Juan 跑 `godot --headless` 场景 trace 后, visual review (Beverly 远程协助)
- [ ] Android APK 出图后, 真机截图视觉终审

---

## 7. 联系表

| 角色 | Agent | 责任 |
|------|-------|------|
| 美术方向 + QA + AI 生图 | Beverly | ART_SPEC / 出图 / alpha / 视觉验收 |
| 协调 + 决策 + review | Shawn | 派活 / review / 跨 agent 编排 |
| 集成 | Framework Engineer (TBD) | 场景 / Godot 节点 / .tscn / .cs |
| DevOps | Robin | 工具链 / 签名 / 集成 cp / install |
| Founder | — | 最终决策 + 反馈 |

---

## 附录 A: 出图 prompts (备份)

详细 prompt 模板保存在 staging: `/root/_art_staging/_prompts.py` (65 行, 10 个 asset prompt)。

修改建议：v0.2 之前**不动**，v0.2 polish 时单独 git commit。