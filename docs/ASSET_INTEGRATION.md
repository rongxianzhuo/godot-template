# Asset Integration — Magic Match v0.1

> 美术资产集成指南。Beverly (art) + Robin (runtime hookup) + Juan (view code)
> 协同产物。修改前先读一遍。

## 1. 资产清单 (18 PNG)

| 子目录 | 文件 | 用途 | 运行时使用方 |
|--------|------|------|--------------|
| `assets/gems/` | `gem_{red,orange,yellow,green,blue,purple}.png` (base RGB) | 留档 — 原始白底图, 1024×1024 | 暂无 |
| `assets/gems/` | `gem_{red,orange,yellow,green,blue,purple}_alpha.png` (RGBA) | **运行时实际使用** | `Match3BoardView.Refresh()` via `SpritePaths.LoadGem()` |
| `assets/ui/` | `button_normal.png` + `_alpha` | **P1 待启用** — Robin 留位 (TextureButton 化时用) | 暂无 (当前用 Godot 默认 Button) |
| `assets/ui/` | `button_pressed.png` + `_alpha` | 同上 | 同上 |
| `assets/backgrounds/` | `bg_game.png` (720×1280 RGB) | 棋盘背景 | `GameScenes.BuildGameScreen()` |
| `assets/backgrounds/` | `bg_title.png` (720×1280 RGB) | 标题背景 | `GameScenes.BuildTitleScreen()` |
| `assets/icons/` | `icon.svg` (Robin 占位) | 启动器图标 (launcher) | 系统级, 不进游戏渲染 |

---

## 2. Sprite → Algorithm color 映射 (锁定 0-5)

**GemType enum**（`src/Features/Match3/Board.cs` 第 7 行）：

| `GemType` enum | 数值 | Sprite path (res://) |
|----------------|------|----------------------|
| `Red` | 0 | `res://assets/gems/gem_red_alpha.png` |
| `Orange` | 1 | `res://assets/gems/gem_orange_alpha.png` |
| `Yellow` | 2 | `res://assets/gems/gem_yellow_alpha.png` |
| `Green` | 3 | `res://assets/gems/gem_green_alpha.png` |
| `Blue` | 4 | `res://assets/gems/gem_blue_alpha.png` |
| `Purple` | 5 | `res://assets/gems/gem_purple_alpha.png` |

⚠️ **索引顺序锁死** — 不要重排 `Board.cs` 的 `GemType` enum。新增颜色时**追加**到末尾 (6, 7, ...) 并同步更新 `SpritePaths.GemTypeToSprite` map。

**Single source of truth**: `src/Features/Match3/SpritePaths.cs` 的 `GemTypeToSprite` Dictionary。`Match3BoardView.Refresh()` 调用 `SpritePaths.LoadGem(type)` 时走这张表。

---

## 3. Godot 4 PNG Import 设置

每张 `*.png.import` sidecar（git tracked）由 `godot --headless --import` 生成。

### 3.1 当前 18 张 PNG 实际 import 设置（v0.1）

| 参数 | 实际值 | Bev 推荐 | 含义 |
|------|--------|----------|------|
| `compress/mode` | **0** (Lossless) | ✅ Lossless | UI 资产走无损, 比 VRAM Compressed 慢一点但颜色精准 |
| `compress/hdr_compression` | 1 (Disabled) | ✅ Disabled | 8-bit UI 不用 HDR 压缩 |
| `mipmaps/generate` | **false** | ✅ false | 2D sprite 不要 mipmap (会模糊) |
| `process/fix_alpha_border` | **true** | ✅ true | **关键** — 避免 alpha 边缘黑线 |
| `process/premult_alpha` | **false** | ✅ false | Premultiplied 会让 edge 看起来"自亮", 卡通 sprite 不需要 |

### 3.2 ⚠️ Robin 旧版 ASSET_INTEGRATION.md 错误更正

Robin 在 v0.1 早期写的文档说 `process/premult_alpha = Premultiplied`。**实际 .png.import 里是 false**。以 `.png.import` 文件实际值为准。

如果未来需要改成 premultiplied（比如想用 GPU blend 加速），需要：
1. 改 `.png.import` 文件
2. 重新跑 `godot --headless --import`
3. 同步更新本文档

### 3.3 Filter 设置

当前 `.png.import` 没有显式 `filter` 行 → 走 Godot 4 默认值 (Linear)。

**Linear 适合本项目**：卡通风格 sprite 有硬边, Linear 比 Nearest 平滑一点。如果发现 sprite 显示锯齿明显，编辑 `.png.import` 加：

```ini
[params]
...
filter=0  # Nearest (硬边, 适合像素风)
```

---

## 4. 验证步骤

### 4.1 import 成功验证

```bash
cd /root/godot-template
godot --headless --import 2>&1 | tail -20
# 期望输出: 18 个 PNG import 成功, 无 error
```

### 4.2 .png.import sidecar 全部存在

```bash
ls assets/*/*.png.import | wc -l
# 期望: 18
```

### 4.3 .ctex 缓存生成（运行时）

```bash
ls .godot/imported/*.ctex | wc -l
# 期望: 18 (运行 godot --headless 后生成)
```

`.godot/` 在 .gitignore 里, 不会进仓库 — 是每台机器本地缓存。

### 4.4 运行时 sprite 加载验证

Juan 的 smoke test (`Match3SmokeTest.cs`) 应该输出:

```
[Match3] Illegal swap (X,Y <-> X+1,Y) — no match.
[Match3] Swap ...: +N pts, M cells cleared. Moves left: ...
```

如果看不到 sprite 而是看到灰色格子:
- 路径错 → 看 stderr 有没有 `ERROR: res://path not found`
- import 没跑 → 重跑 `godot --headless --import`
- alpha 全透明 → `process/fix_alpha_border` 误关 + `premult_alpha` 误开

---

## 5. 故障排查 (Troubleshooting)

### 5.1 Sprite 显示但 alpha 边缘有黑线

**症状**: gem 主体 OK, 但 1-2 px 边上有明显黑色描边。

**原因**: 三个可能 (按概率排序):
1. `process/premult_alpha = true` 但 sprite 源不是 premultiplied 格式
2. `process/fix_alpha_border = false`
3. 源 PNG 的 alpha 是 binary (0/255) 而非 partial alpha — rembg 已经处理过

**修复**:
```bash
# 改 .png.import
sed -i 's/process\/premult_alpha=true/process\/premult_alpha=false/' assets/gems/gem_*_alpha.png.import
sed -i 's/process\/fix_alpha_border=false/process\/fix_alpha_border=true/' assets/gems/gem_*_alpha.png.import
godot --headless --import
```

### 5.2 Sprite 完全透明 / 看不到

**症状**: TextureRect 是空白的。

**原因**:
1. `SpritePaths.GemTypeToSprite` 路径错 (vs Board.cs GemType enum)
2. 加载的是 base PNG (非 `_alpha`) — base 是 RGB, Godot 会显示但 alpha 区域 = 0 看不见
3. `.godot/imported/*.ctex` 没生成 → `godot --headless --import` 没跑

**修复**:
```bash
# 1. 检查 map
grep "GemTypeToSprite" src/Features/Match3/SpritePaths.cs

# 2. 确保用 _alpha 版本
grep "Texture = SpritePaths.LoadGem" src/Features/Match3/Match3BoardView.cs

# 3. 重跑 import
godot --headless --import && ls .godot/imported/*.ctex | wc -l
```

### 5.3 颜色和 ART_SPEC §1.2 hex 不一致

**症状**: gem_red 显示出来偏 orange-red, 不是 #D61420 深红。

**原因**:
1. image-01 自然 drift (per-iteration rule: max 3 次 v1/v2/v3)
2. VRAM Compressed 会让颜色偏移 (本项目已用 Lossless 避免)
3. Filter = Nearest 时边缘锐利但内部颜色正确, Linear 平滑但色阶有 blending

**修复**:
- 短期: 接受 (已在 ART_SPEC §1.2 标注 drift)
- 长期: 用 `ImageMagick` 或 PIL 后处理微调颜色, 或重新出图 v3

```python
from PIL import Image
img = Image.open("assets/gems/gem_red_alpha.png").convert("RGBA")
# shift red channel up by 20
arr = np.array(img)
arr[:,:,0] = np.clip(arr[:,:,0].astype(int) + 20, 0, 255).astype(np.uint8)
Image.fromarray(arr).save("assets/gems/gem_red_alpha.png", format="PNG", optimize=True)
```

### 5.4 Button sprite 没被使用

**症状**: 我交付了 `button_normal_alpha.png`, 但游戏里看不到 — 因为 `GameScenes.MakeButton()` 用的是 Godot 默认 `Button`。

**原因**: v0.1 决策 — Robin 决定先 ship 默认按钮, P1 polish 时切 `TextureButton` 用我的 sprite。

**不在 v0.1 阻塞**: 按钮功能正常, 美术等到 v0.2 启用。

---

## 6. v0.1 Status 快照

- [x] 18 PNGs 全部 push 到 `assets/{gems,ui,backgrounds}/`
- [x] 18 个 `.png.import` sidecar 已生成并 commit
- [x] `src/Features/Match3/SpritePaths.cs` 锁定 GemType → sprite 映射
- [x] `src/Features/Match3/Match3BoardView.cs` 渲染棋盘用 `*_alpha.png`
- [x] `src/Features/Match3/GameScenes.cs` 标题/游戏/结束屏用 `*_alpha.png` (bg_game / bg_title)
- [x] 6 颗 gem alpha 处理后 partial ratio avg 1.77% (硬边干净, 不需要 premult)
- [x] Board.cs `GemType` enum 索引 0=red..5=purple 锁死
- [ ] P1: 把 `GameScenes.MakeButton()` 改 `TextureButton` 用 button_normal_alpha / button_pressed_alpha
- [ ] P1: 替换 `assets/icons/icon.svg` (Robin 占位) 为真正的 app icon

---

## 7. 后续流程 (Bev 出新图后)

1. **覆盖** `assets/<subdir>/<asset>.png` (base) 或 `*_alpha.png` (运行时使用)
2. **新图 alpha 处理** (如 base 改了):
   ```bash
   python3 /root/icon_alpha.py assets/gems/gem_red.png --preset gem --model u2netp --output-dir assets/gems/
   ```
3. **重跑 import**: `godot --headless --import`
4. **commit PNG + `.png.import` + `SpritePaths.cs`** (如有新颜色) 一起 push
5. **不要**只 commit PNG 不 commit `.png.import` — 其他 agent pull 后 import 会失败

---

## 联系表

| 主题 | 找谁 |
|------|------|
| sprite 路径映射 / 新增颜色 | Beverly (art) |
| 运行时 sprite 加载 bug | Juan (game logic) |
| import 设置 / APK 构建 | Robin (DevOps) |
| 整体协调 / API 冲突 | Shawn |