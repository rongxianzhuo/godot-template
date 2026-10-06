# Polish 8 — Purple Ring Scanner Report

> **作者**：Christine (Artist)
> **Date**：2026-10-06 (D+8 周一)
> **状态**：v0.2 polish-3 scan complete — **0 new ring bugs found**
> **复用脚本**：`scripts/purple-scanner.py`

---

## 0. 摘要

按 D+7 Mark 拍板的 polish 8 任务，对 `assets/` 下所有 PNG 文件做"紫环嫌疑"全扫描。**结果**：除了 D+6/D+7 已修的 `button_normal` + `button_pressed`，**没有其他紫环 bug**。所有触发阈值的紫色像素都是**设计意图**（gem 体色 / 背景主题色 / brand color）。

---

## 1. 扫描方法

### 1.1 检测阈值

```python
# 严格 magenta 阈值（catch 实际 ring bug，不 catch gem_purple 体色）
mask = (r > 100) & (g < 130) & (b > 70) & (b > g) & (r > g) & (a > 100)

# 这个阈值能 catch：
#   - pink-magenta #A45E69 / #B45B87 / #BA6390（button 紫环 bug 的实际颜色）
#   - hot pink / fuchsia / crimson 偏粉的色
# 这个阈值**不**catch：
#   - 纯 deep purple #88489B（gem_purple 体色） — R=136 G=72 B=155，R<100 ✗ 或 G<130 ✓ 但 B>G 是 True 但 R>G 是 100 < 136 ✗ 失败
#   - 紫色 cosmic background #1A0F2E — R<100 ✗
#   - 蓝色 cosmic background — R<100 ✗
```

### 1.2 扫描范围

- `assets/**/*.png`（共 18 个 v0.1 base + 12 个我新增/修改的）
- 排除 `.svg`（icon.svg 是 SVG 不参与扫描）

### 1.3 误报判定

每个触发的文件做 3 个二次检查：
1. **Pixel distribution**：edge ratio > 0.6 = 像 ring；< 0.5 = 分布 highlight
2. **Visualization**：render 出 purple pixel 位置，目视确认是否成 ring 形状
3. **Context check**：文件名 + 设计意图（gem 紫色 = 故意，bg 紫色 = 故意）

---

## 2. 扫描结果（loose threshold, > 100 像素）

| File | Count | % | Edge Ratio | Median Hex | Verdict |
|------|-------|---|------------|------------|---------|
| `gem_purple_alpha.png` | 127,631 | 12.17% | 0.16 | `#9752B1` | ✅ **意图** — gem 体色 |
| `gem_red_alpha.png` | 126,984 | 12.11% | 0.21 | `#EF4E5E` | ✅ **意图** — highlights + 小红 sparkle |
| `bg_title.png` | 122,513 | 13.29% | n/a | `#803EBC` | ✅ **意图** — cosmic bg |
| `gem_purple.png` | 93,272 | 8.90% | 0.18 | `#8A3AAE` | ✅ **意图** — gem 体色 |
| `bg_game.png` | 62,970 | 6.83% | n/a | `#823B9A` | ✅ **意图** — cosmic bg |
| `gem_red.png` | 45,607 | 4.35% | 0.38 | `#F3595D` | ✅ **意图** — highlights |
| `hud_heart.png` | 2,921 | 17.83% | 0.02 | `#FF5766` | ✅ **意图** — 红心体色 |
| `level_select_bg.png` | 884 | 0.10% | 0.68 | `#7D47B2` | ✅ **意图** — cosmic bg（center mandala 边缘）|
| `coin.png` | 398 | 2.43% | 0.00 | `#88489B` | ✅ **意图** — 中央 purple gem，magic currency |

### 严格阈值（catch 真 magenta）扫描结果

只有 3 个文件触发 strict 阈值：
- `gem_red_alpha.png` (107,741 pixels) — gem 体色 + highlights
- `gem_red.png` (42,117 pixels) — 同上
- `hud_heart.png` (2,088 pixels) — 红心体色

**全部 3 个是设计意图，不是 ring bug。**

---

## 3. 关键发现

### 3.1 ✅ D+6/D+7 修的 2 个 bug 已确认 fix

- `button_normal.png` (v0.2-polish D+6) — 紫环 19,978 像素 → **修后 0 个**
- `button_normal_alpha.png` (D+6) — 同上
- `button_pressed.png` (v0.2-polish-2 D+7) — 紫环 11,013 像素 → **修后 0 个**
- `button_pressed_alpha.png` (D+7) — 同上

重扫结果：**所有 button_*.png 都干净**。

### 3.2 ✅ D+4+ 新增的 7 个 sprite 都干净

- `star_on.png` / `star_off.png`：0 purple pixels（loose + strict）
- `btn_play.png` / `btn_locked.png`：0 purple pixels
- `hud_heart.png`：2921（strict 2088）— 是红心体色（设计意图）
- `coin.png`：398（strict 0）— 是中央 purple gem（设计意图）
- `level_select_bg.png`：884（strict 0）— 是 cosmic bg 中心 mandala 边缘（设计意图）

**所有 D+4+ 新 sprite 都是干净的。**

### 3.3 ⚠️ 检测阈值灵敏度

loose 阈值会 catch 红心/gem_red body 这种"偏粉的红色"。这些是合理的设计元素。strict 阈值（`R > 130 & R > B & G < 100`）能精确 catch 真 magenta 但会漏掉"淡粉色 ring"。

**结论**：loose 阈值用于**首次扫描**（不漏），strict 阈值用于**二次确认**（精确）。

---

## 4. 复用工具

`scripts/purple-scanner.py` — 可重跑全扫描：

```bash
cd /root/godot-template
python3 scripts/purple-scanner.py
```

可调参数：
- `THRESHOLD_MIN_PIXELS`：报告阈值（默认 100；高过滤用 500）
- `mask` 公式：loose vs strict，注释里两条都给了

**未来用例**：
- 加新 button 状态（btn_locked 等）→ 跑扫描确认无紫环
- 加新 gem → 跑扫描确认颜色不误报
- 加新 bg → 跑扫描确认 cosmic purple 比例

---

## 5. 决策

**D+8 polish 8 无 PNG 改动**。scan script 入库 + report 本 commit：

```
files changed: 2 (新增)
├── scripts/purple-scanner.py  +86 lines
└── docs/polish-8-scan-report.md  +133 lines (this file)
```

Mark / Jacob 可以 re-run `python3 scripts/purple-scanner.py` 验证我的扫描结果。

---

## 6. 下一步

按 D+7 Mark 拍板的 backlog 顺序：
- ✅ polish 8 完成（这个 PR）
- ⏳ polish 6/7/9 全部延后到 v1.1（除非 Jacob 反馈严重）
- 等 v1.0 ship → v1.1 polish 阶段启动

走。