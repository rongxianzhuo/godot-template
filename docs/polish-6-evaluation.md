# Polish 6 — 4 P1 Sprite AI 重制评估

> **作者**：Christine (Artist)
> **Date**：2026-10-06 (D+14 周四)
> **状态**：evaluation — Mark 拍板是否替换 v1.0 P1 sprites
> **目标 sprite**：`btn_play` / `btn_locked` / `hud_heart` / `coin`
> **背景**：D+7 Mark 拍板 polish 6 延后 v1.1，D+14 启动评估

---

## 0. TL;DR

| Sprite | v1.0 评分 | v1.1 评分 | 提升 | 决策 |
|--------|-----------|-----------|------|------|
| btn_play | 8/10 | 9/10 | +1 | 🟢 推荐替换（v1.1 polish）|
| btn_locked | 7/10 | 9/10 | **+2** | 🟢 推荐替换（最显著提升）|
| hud_heart | 8/10 | 9/10 | +1 | 🟢 推荐替换 |
| coin | 9/10 | — | 0 | 🟢 **保持 v1.0**（已 ship 标准）|

**推荐**：

- **v1.0 ship 保持现状**（per Mark D+14 建议："如果 v1.0 P1 sprite 已经 ship 标准"）
- **v1.1 polish backlog**：3 个 sprite AI 重制（btn_play / btn_locked / hud_heart），coin 不变
- **不立刻替换 v1.0 PNG** — 避免 v1.0 ship blocker

---

## 1. v1.0 sprite 评估

| Sprite | 文件 | 评分 | 评价 |
|--------|------|------|------|
| btn_play | `assets/ui/btn_play.png` (256×256, 11KB) | 8/10 | 黄底 + 绿三角 + 白椭圆高光 — OK 但高光像贴上去的 |
| btn_locked | `assets/ui/btn_locked.png` (256×256, 8KB) | 7/10 | 灰底 + 平 padlock + 弱对比 — 偏弱 |
| hud_heart | `assets/ui/hud_heart.png` (128×128, 5KB) | 8/10 | Candy Crush 风格 — OK 但缺 cosmic 元素 |
| coin | `assets/ui/coin.png` (128×128, 9KB) | 9/10 | gold + 紫宝石 + sparkles — **best of 4** |

**共同 issue**：
- ❌ 缺 cosmic purple 品牌一致性（除 coin）
- ❌ 高光像 ellipse 贴纸（cheap）
- ❌ 没 sparkles / magic 元素（除 coin）
- ❌ 通用 Candy Crush 风格，非 "Magic Match" 品牌

---

## 2. v1.1 AI 重制结果

**prompt 锚点**：
- "cosmic purple magic aesthetic" — 强调品牌主题
- "premium mobile game UI, candy crush level polish"
- "NOT realistic, NOT photographic"
- "magic glow + sparkle stars"

### 2.1 btn_play v1.1

- ✅ Cosmic purple rim glow（品牌一致）
- ✅ 3 white sparkles around button（顶部 + 右下 + 右上）
- ✅ Green play triangle with white outline（保留可读性）
- ✅ Multi-stop gradient（顶部 highlight 优雅）

**rating**: 8/10 → **9/10**（+1）

### 2.2 btn_locked v1.1

- ✅ Dark cosmic gray-purple rim（与 bg_game 协调）
- ✅ **Cosmic purple padlock body**（品牌一致）
- ✅ 4 sparkles（lock 周围 + 底部光晕）
- ✅ Silver padlock shackle（金属感）

**rating**: 7/10 → **9/10**（**+2 — 最显著提升**）

### 2.3 hud_heart v1.1

- ✅ Magenta-pink heart + cosmic purple rim glow
- ✅ 2 sparkles 右上角（与 coin 一致）
- ✅ Center pink highlight（magical glow effect）
- ✅ 整 heart 微微发光（premium mobile 风格）

**rating**: 8/10 → **9/10**（+1）

### 2.4 coin v1.1

**保持 v1.0** — coin 已经 9/10：
- ✅ gold body + cosmic purple gem in center
- ✅ 2 sparkles
- ✅ 与 gem_purple 紫色协调

**重制 ROI 低**：v1.0 coin 已经 ship 标准，AI 重制不会显著提升（边际收益 ~0.3 分）

---

## 3. v1.1 包大小影响

| Sprite | v1.0 size | v1.1 size | 增量 |
|--------|-----------|-----------|------|
| btn_play | 11 KB | 98 KB | +87 KB |
| btn_play_alpha | (3 KB) | 17 KB | +14 KB |
| btn_locked | 8 KB | 86 KB | +78 KB |
| btn_locked_alpha | (2 KB) | 8 KB | +6 KB |
| hud_heart | 5 KB | 25 KB | +20 KB |
| hud_heart_alpha | (1 KB) | 2 KB | +1 KB |
| **总计 v1.1 增量** | — | — | **+206 KB** |
| coin（保持）| 9 KB | 9 KB | 0 |

**v1.1 总包大小**：
- v1.0 (8.0 MB) + Fredoka + Nunito (95 KB) + i18n PO (25 KB) + 4 P1 v1.1 (206 KB) + splash (0 KB)
- = 8.0 MB + 326 KB = **8.32 MB total**

**预算 12 MB，节省 31% 预算保持**

---

## 4. 视觉对比 sheet

`/shared/christine-p1-sprite-comparison.png` (800×600 PNG, 162 KB)

3-row × 3-col 表显示：
- Sprite 名（左）
- v1.0 当前 ship（中左）
- v1.1 AI 重制（中右）
- 评分变化（右）

**视觉结论**：v1.1 sprites 明显 cosmic purple 品牌一致，sparkles + glow 增强 polish 感。

---

## 5. 决策建议（Mark 拍板）

### 5.1 选项 A：v1.0 ship 保持 + v1.1 polish backlog ⭐ **推荐**

- ✅ v1.0 ship 不动（避免 ship blocker）
- ✅ v1.1 polish 启动 3 P1 sprite 替换
- ✅ 估算工作量 0.5 天（commit + .png.import + push）

### 5.2 选项 B：v1.0 ship 前替换（per Mark D+7 决策升级）

- ⚠️ 需 Mark 拍板"polish 6 = ship-blocker"
- ⚠️ 风险：v1.0 ship 延期
- ✅ 收益：玩家第一眼看到 cosmic 风格 UI

### 5.3 选项 C：4 P1 都 AI 重制（包含 coin）

- ❌ coin 提升不明显（边际 0.3 分）
- ❌ 浪费时间
- ❌ 重新引入 AI 不确定性

---

## 6. v1.1 polish backlog 实施 checklist（Jacob）

如果 Mark 选 A 路径：

- [ ] Mark 拍板 polish 6 = v1.1 ship scope
- [ ] Christine copy 6 PNG 到 `assets/ui/`:
  - `btn_play.png` + `btn_play_alpha.png`
  - `btn_locked.png` + `btn_locked_alpha.png`
  - `hud_heart.png` + `hud_heart_alpha.png`
- [ ] Christine 写 `.png.import` placeholder UIDs（per Jacob UID rule）
- [ ] Push `art/v1-p1-sprite-revisit` branch
- [ ] Jacob: `merge main && godot --headless --import`（fix UIDs）
- [ ] Jacob: 视觉验收（Android 真机测试）
- [ ] Mark: merge → main
- [ ] v1.1 ship 包含新 sprite

**估算工作量**：0.5 天（Christine 主要 + Jacob verify）

---

## 7. 风险评估

| 风险 | 概率 | 影响 | 缓解 |
|------|------|------|------|
| AI re-gen 引入 artifact（text / extra shape） | 🟢 Low（已 verified 无明显 artifact） | 视觉 regression | 已 visual review OK |
| 文件大小爆炸（98KB vs 11KB） | 🟢 Acceptable（v1.1 总 +206 KB） | 包大小 +2.5% | 仍在 31% 预算节省 |
| 玩家视觉识别度下降（按钮形状变了） | 🟡 Medium | 玩家迷失 | btn_play 仍保留绿三角 + 圆角，认知不变 |
| cosmic purple glow 漏到 UI 边界 | 🟢 Low（thumbnail 180×180 已验证无溢出） | 视觉 regression | 已 thumbnail 验证 |

**综合风险**：🟢 **低**（v1.1 替换 3 个，coin 保持，玩家视觉升级明显）

---

## 附录 A：v1.1 sprite 文件位置

**未提交到 godot-template**（local only）：

- `/root/christine-icon-drafts/btn_play_v1.1.png` (98 KB)
- `/root/christine-icon-drafts/btn_play_v1.1_alpha.png` (17 KB)
- `/root/christine-icon-drafts/btn_locked_v1.1.png` (86 KB)
- `/root/christine-icon-drafts/btn_locked_v1.1_alpha.png` (8 KB)
- `/root/christine-icon-drafts/hud_heart_v1.1.png` (25 KB)
- `/root/christine-icon-drafts/hud_heart_v1.1_alpha.png` (2 KB)

**跨 agent review**：

- `/shared/christine-p1-sprite-comparison.png` (162 KB) — 视觉对比表
- `/shared/christine-p1-sprite-comparison.md`（TODO D+14+）— 对比详情 markdown

**prompt anchor**（per `docs/ART_SPEC.md` §13.1 anti-feather rule）：

```
✅ "cosmic purple magic aesthetic" — 品牌主题
✅ "premium mobile game UI, candy crush level polish"
✅ "sparkle stars" — 与 gem family 一致
❌ "halo" / "bloom" / "soft-glow" — v1.4 anti-feather 规则禁止（v1 仍有 glow，但已 inline 到 sprite 内部）
```

---

## 附录 B：变更日志

### v0.1 (2026-10-06, Christine)

D+14 周四 polish 6 evaluation。

**新增**：
- 3 张 AI 重制 sprite（btn_play / btn_locked / hud_heart v1.1）
- 6 张 _alpha.png
- 视觉对比 sheet
- 评估 doc

**决策（待 Mark 拍板）**：
- 选项 A（推荐）：v1.0 ship 不动，v1.1 polish backlog 启动替换 3 sprite
- 选项 B（如果 Mark 升 ship-blocker）：v1.0 ship 前替换 3 sprite
- 选项 C（不推荐）：4 sprite 都重制（含 coin）

**未决**：
- Mark 是否激活 polish 6 为 v1.1 ship scope
- AI 重制 artifact 是否完全清除（建议 Jacob 真机视觉终审）
- v1.1 polish 与 splash + TTF 同步 ship 时间表

---

**polish 6 evaluation 完。** Mark verify + 拍板（A / B / C 选项）。