# Magic Match v1.0 — Release Notes

> **Date**：2026-10-06 (v1.0 release, Week 6)
> **Version**：v1.0.0 (semver)
> **Build**：D+10
> **Platform**：Android (arm64-v8a, min SDK 24, target SDK 36)
> **Audience**：Founder / Google Play store / Internal team

---

## 🎯 Headline

> **"Magic Match v1.0 — A cosmic match-3 adventure through 3 enchanted themes."**

---

## ✨ What's New

**First public release of Magic Match** — a cosmic-themed match-3 puzzle game with 60 levels across 3 themed worlds.

### 🎮 60 Levels · 3 Themes

- **Forest (Levels 1–20)** — mossy greens, mushroom browns, dappled sunlight
- **Desert (Levels 21–40)** — sunset oranges, cactus greens, sand dunes
- **Ocean (Levels 41–60)** — deep coral blues, pearl whites, rippling waves

### 💎 6 Gem Types

- 🔴 Crimson · 🟠 Amber · 🟡 Sunshine · 🟢 Forest · 🔵 Sapphire · 🟣 Amethyst

### ⭐ Star Rating System

- Earn up to **3 stars per level** based on score
- 0 / 1 / 2 / 3 star thresholds per level
- Track progress across all 60 levels

### 🏆 Endless Mode

- After completing 60 levels, **Endless Mode** unlocks
- Procedurally generated boards
- Compete for high score (local, no leaderboard yet)

### ❤️ Lives System

- 3 hearts per session
- Refill via AdMob rewarded ad (1 heart per view)
- 30-minute natural refill

### 🎨 Visual Polish

- 30 hand-crafted assets (12 gem variants + 10 UI elements + 2 backgrounds + App icon)
- Cosmic purple background with ornate mandala + lotus motifs
- Glossy "glossy magical gem" art style (Candy Crush inspired)

---

## 📸 Screenshots

> **TODO D+11+**: Capture from real Android device or emulator. 4-6 screenshots per Google Play best practices:
> 1. Title screen (with ornate lotus background)
> 3. Gameplay screen (gem grid)
> 4. Level select screen (with stars)
> 5. EndScreen (Win with 3 stars)
> 6. Theme banner cross-fade

For now, see [`/root/christine-icon-drafts/visual-review-v1-sprites.png`](/root/christine-icon-drafts/visual-review-v1-sprites.png) for art assets preview.

---

## 🐛 Bug Fixes

**First release — no fixes from previous versions**.

---

## ⚠️ Known Issues & Limitations

| # | Issue | Severity | Plan |
|---|-------|----------|------|
| 1 | Splash screen uses `icon.svg` (no brand splash) | 🟡 Low | v1.1 (splash-screen-design B方案) |
| 2 | Godot default font (not TTF) | 🟡 Low | v1.1 (TTF upgrade) |
| 3 | English only (no localization) | 🟠 Medium | v2.0 (i18n strategy) |
| 4 | Dark mode only (no light mode) | 🟡 Low | v2.0+ |
| 5 | AdMob only on Continue (no banner ads) | 🟡 Low | A/B test later |
| 6 | Single player (no social) | 🟢 Not in v1.0 scope | v2.0+ |
| 7 | gem_purple polish: p50 #77338F vs target #88489B (17/21/12 通道差) | 🟢 Acceptable | v1.1 if budget |
| 8 | 4 P1 sprite (btn_play / btn_locked / hud_heart / coin) visual 8/10 — could be AI 重制 9/10 | 🟢 Acceptable | v1.1 if budget |

---

## 🚀 Coming Next (v1.1)

**ETA**: v1.0 ship 后 2-4 周

| Feature | Source | Est. Work |
|---------|--------|-----------|
| **Splash screen redesign**（方案 B 复用 bg_title） | D+10 `docs/design/splash-screen-design.md` | 1 天 |
| **TTF font upgrade**（Fredoka + Nunito 候选）| v0.2 ART_SPEC §4 | 1 天 |
| Polish backlog from `art/v0.2-polish-3`（per Mark D+7 拍板，可激活） | D+6-D+7 polish releases | 1-2 天 |

---

## 💡 Apps Internal Notes

#### v1.0 ship 决策（Mark 已拍板）

- ✅ 30 美术资产 全部 ship-ready（D+2 至 D+8 polish 累计 22 commits + 4 docs commits）
- ✅ 5 个 art branch push + 2 docs branch push + 1 tools branch push
- ✅ 总文件大小 ~7.9 MB（节省 34% 预算）
- ✅ purple-scanner.py 工具 commit（团队标准工具）
- ✅ ART_SPEC v1.0 (single source of truth, 789 lines)
- ⏳ v0.2 polish 5/5 完成
- ⏳ polish 6/7/9 全部延后 v1.1

---

## 🙏 Credits

| 角色 | Agent | 贡献 |
|------|-------|------|
| Founder + Coordinator | Mark | 协调 / 拍板 / 跨 agent 中继 |
| Game Designer | Austin | Match3 设计 / 关卡 / 产品设计 doc |
| Art Director + Artist | Christine | 全部 30 美术资产 + ART_SPEC + AI 出图 + 后处理 |
| Engineer | Jacob | C# 实现 / Godot 集成 / UID 规则 / purple-scanner 部署 |
| Framework Engineer | Francisco | godot-framework SDK / v0.3 EventBus |

---

## 📦 Google Play Store Listing

> **TODO D+11**: Fill actual values when store assets ready

### Short description (80 chars)
```
Match 3 cosmic gems across 3 enchanted themes! 60 levels of pure fun.
```

### Full description (4000 chars)
```
Welcome to Magic Match — a cosmic match-3 puzzle adventure!

Journey through 60 hand-crafted levels across 3 enchanted themes:

🌲 FOREST (Levels 1-20)
Mossy greens, mushroom browns, dappled sunlight. Match gems beneath the ancient trees.

🏜️ DESERT (Levels 21-40)
Sunset oranges, cactus greens, sand dunes. Match gems under the desert sky.

🌊 OCEAN (Levels 41-60)
Deep coral blues, rippling waves, pearl shimmer. Match gems in the ocean depths.

FEATURES:
• 60 unique levels with increasing challenge
• 6 different gem types
• Earn up to 3 stars per level
• 3 themed worlds with distinct color palettes
• Lives system with optional ad refills
• Hand-crafted art with cosmic purple aesthetic
• Smooth animations and satisfying match effects
• Endless mode after completing all levels

PERFECT FOR:
• Casual puzzle fans
• Match-3 lovers
• Anyone who loves beautiful, magical aesthetics
• Quick play sessions on the go

Whether you have 2 minutes or 20, Magic Match is your perfect cosmic companion. Download now and start your journey through enchanted worlds!
```

### What's New (Google Play "Release Notes" field)
```
🎉 Magic Match v1.0 launches!

✨ 60 cosmic levels across 3 enchanted themes (Forest / Desert / Ocean)
💎 6 gem types: Crimson, Amber, Sunshine, Forest, Sapphire, Amethyst
⭐ Star rating system — earn up to 3 stars per level
❤️ Lives system — refill via rewarded ads
🎨 Hand-crafted cosmic purple art style
🏆 Endless mode unlocks after level 60

We can't wait for you to play.
```

### Category / Tags
```
Category: Puzzle (GAME_PUZZLE)
Tags: match-3, puzzle, casual, free
```

### Age rating
```
Everyone (PEGI 3 / ESRB E) — no objectionable content
```

---

## 📋 Technical Specs

| 维度 | 值 |
|------|-----|
| Min Android SDK | 24 (Android 7.0 Nougat) |
| Target Android SDK | 36 (Android 16) |
| Architecture | arm64-v8a |
| Godot version | 4.7.2 mono |
| .NET | 9 (TFM = `net9.0`) |
| APK size (估算) | ~110 MB total (含 assets + Godot runtime) |
| Assets only | ~7.9 MB |
| Frame rate target | 60 FPS |

---

## 🔗 相关链接

- 美术规范：[`docs/ART_SPEC.md`](ART_SPEC.md) (v1.0, 789 lines)
- 资产集成流程：[`docs/ASSET_INTEGRATION.md`](ASSET_INTEGRATION.md)
- v0.2 polish 历史：[`docs/polish-8-scan-report.md`](polish-8-scan-report.md)
- v1.1 splash 设计：[`docs/design/splash-screen-design.md`](design/splash-screen-design.md)
- 仓库：[github.com/rongxianzhuo/godot-template](https://github.com/rongxianzhuo/godot-template)

---

**v1.0 release notes 完。** Mark verify 后 commit 到 main。