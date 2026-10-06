# Font Integration Spec — v1.1 (Christine → Jacob Handoff)

> **作者**：Christine (Artist)
> **Date**：2026-10-06 (D+17 周日)
> **状态**：build spec (TTF prep → implementation handoff)
> **目标读者**：Jacob (D+18 上午实施 TTF 集成)
> **设计参考**：`docs/design/ttf-font-research.md` (D+11 font research) + `docs/CREDITS.md` (attribution)

---

## 0. TL;DR

**TTF files ready** (per `assets/fonts/`):
- `Fredoka-Bold.ttf` (33 KB)
- `Fredoka-SemiBold.ttf` (33 KB)
- `Nunito-Regular.ttf` (39 KB)
- `Nunito-Medium.ttf` (39 KB)
- `Nunito-Bold.ttf` (39 KB)
- `OFL.txt` (118 行, per Mark D+11 attribution rule)

**Total**: 182 KB (subset, +88 KB vs estimated 95 KB)

**新增 C# 文件**：`src/Core/ThemeBuilder.cs` (font loader + theme setter)

**实施工作量**：0.5-0.7 天 (per Mark D+15 schedule D+18 上午 + 下午)

---

## 1. 文件清单

### 1.1 assets/fonts/ 目录

```
assets/fonts/
├── Fredoka-Bold.ttf       (33 KB)  ← display title (splash "Magic Match")
├── Fredoka-SemiBold.ttf   (33 KB)  ← large button labels (Play / Continue)
├── Nunito-Regular.ttf     (39 KB)  ← body text
├── Nunito-Medium.ttf      (39 KB)  ← UI labels (HUD numbers)
├── Nunito-Bold.ttf        (39 KB)  ← emphasized text (end screen)
└── OFL.txt                (118 lines)  ← SIL OFL 1.1 license text
```

### 1.2 docs/CREDITS.md (D+17 NEW)

字体 designer attribution (per Mark D+11 rule):
- Fredoka by Milena Brandão (2016)
- Nunito by Vernon Adams + Manvel Shmavonyan + Jacques Le Bailly

### 1.3 src/Core/ThemeBuilder.cs (D+18 NEW by Jacob)

C# class to load fonts + apply to Godot Theme.

---

## 2. 字体选择 (per Mark D+11 decision)

| Font | Weight | 用途 | v1.1 subset |
|------|--------|------|-------------|
| **Fredoka** | Bold (700) | splash title + button labels | ✅ 33 KB |
| **Fredoka** | SemiBold (600) | large UI display | ✅ 33 KB |
| **Nunito** | Regular (400) | body text | ✅ 39 KB |
| **Nunito** | Medium (500) | UI labels | ✅ 39 KB |
| **Nunito** | Bold (700) | emphasized text | ✅ 39 KB |

**CJK 字体延后 v2.0**（per Mark D+11 + D+12）

---

## 3. Godot 集成 (Jacob C# pseudocode)

### 3.1 加载 TTF（FontFile）

```csharp
// src/Core/ThemeBuilder.cs
using Godot;

public static class ThemeBuilder
{
    public static Theme BuildCosmicTheme()
    {
        var theme = new Theme();

        // Load Fredoka Bold (display title)
        var fredokaBold = new FontFile();
        var err = fredokaBold.LoadDynamicFont(
            "res://assets/fonts/Fredoka-Bold.ttf",
            64  // base size for splash title
        );
        if (err != Error.Ok) {
            GD.PrintErr($"Failed to load Fredoka-Bold: {err}");
        }
        theme.SetDefaultFont(fredokaBold);
        theme.SetFontSize("title", "Label", 52);

        // ... similar for SemiBold + 3 Nunito weights

        return theme;
    }
}
```

### 3.2 在场景应用 Theme

```csharp
// In TitleScreen.cs (or SplashScreen.cs)
public override void _Ready()
{
    var theme = ThemeBuilder.BuildCosmicTheme();
    Theme = theme;
    // Or apply per child node:
    foreach (var child in GetChildren())
    {
        if (child is Control control) control.Theme = theme;
    }
}
```

### 3.3 Fallback (v1.1 Latin only)

```csharp
// v1.1: en + es + pt-BR 都用 Latin subset，无 fallback 需要
// v2.0 (future): 加 CJK fallback:
theme.SetFont("fallback", "Label", cjkFontFile);
```

---

## 4. 字体使用规范 (per ART_SPEC §4.1)

| 元素 | Font | Size | Weight |
|------|------|------|--------|
| splash title "Magic Match" | Fredoka | 52 px | Bold |
| splash subtitle "60 Levels · 3 Themes" | Nunito | 18 px | Regular |
| button label "Play" / "Continue" | Fredoka | 28 px | SemiBold |
| HUD numbers (Score / Moves) | Nunito | 22 px | Medium |
| body text (Level description) | Nunito | 18 px | Regular |
| end screen title "You Won!" | Nunito | 32 px | Bold |
| end screen button label | Fredoka | 24 px | SemiBold |
| i18n strings (es / pt-BR) | Nunito | (same as above) | (same as above) |

---

## 5. UID + .import sidecar (per Jacob UID fix rule)

**重要**：TTF 文件不生成 .png.import，但 Godot 4.7 需要 import resource。

**Jacob 流程**:
1. Copy TTF files from `assets/fonts/` to godot-template
2. Run `godot --headless --import` (regenerates UIDs)
3. TTF files auto-import as `FontFile` resources
4. Reference in C# via `res://assets/fonts/Fredoka-Bold.ttf`

**预期 .uid 自动生成**：无需 placeholder UIDs（font 文件不需要 .png.import sidecar）

---

## 6. v1.1 + v2.0 升级路径

### 6.1 v1.1 scope (D+18-D+19)

- ✅ 5 TTF subset files ready
- ✅ OFL.txt + docs/CREDITS.md
- ⏳ Jacob 实施 ThemeBuilder.cs + apply Theme to scenes
- ⏳ 真机测试 (Android emulator + USB device)

### 6.2 v2.0 scope (future)

- ⏳ CJK fonts (Noto Sans SC/JP/KR) — +2.7 MB
- ⏳ Per-locale `cmap` subsetting (per `docs/design/i18n-strategy.md` §3)
- ⏳ Android App Bundle per-language split (节省 19-25%)
- ⏳ Theme.set_fallback (Latin → CJK chain)

---

## 7. Jacob 实施 checklist (D+18)

- [ ] Read `docs/design/font-integration-spec.md` (this file)
- [ ] Read `docs/design/ttf-font-research.md` (D+11 font research)
- [ ] Read `docs/ART_SPEC.md` §4.1 (字体规格表)
- [ ] Create `src/Core/ThemeBuilder.cs` (per §3.1 pseudocode)
- [ ] Apply Theme to `src/Features/Splash/SplashScreen.cs`
- [ ] Apply Theme to `src/Features/Match3/Screens/TitleScreen.cs`
- [ ] Apply Theme to `src/Features/Match3/Screens/LevelSelectScreen.cs`
- [ ] Apply Theme to `src/Features/Match3/UI/EndScreen.cs`
- [ ] Apply Theme to `src/Features/Match3/UI/HUD.cs`
- [ ] Run `godot --headless --import` (regenerate UIDs)
- [ ] 真机测试 Android (splash + title + level select + end screen + HUD)
- [ ] 视觉验收 (Christine review: text rendering + size + alignment)
- [ ] Commit + push to `art/v1.1-ttf-integration`
- [ ] Jacob UID fix (if applicable)
- [ ] merge → main

**预期 commit**: 1 (ThemeBuilder.cs + Theme application to 5 scenes)

---

## 8. 风险评估

| 风险 | 概率 | 影响 | 缓解 |
|------|------|------|------|
| fontTools subset 漏字符 | 🟢 Low (122 chars verified) | i18n 字符显示为 .notdef | per §4 字符规范表 + per-locale verify |
| Godot FontFile 不支持 dynamic load | 🟢 Low (Godot 4.7+ 支持) | runtime crash | static load via ResourceLoader 备选 |
| CJK 字符 v1.1 显示为 □ | 🟢 N/A (Latin only) | — | v2.0 加 CJK subset |
| TTF 文件大小爆炸 | 🟢 Low (subset = 182 KB) | 包大小 +182 KB | 仍在 30% 预算节省 |

**综合风险**：🟢 **低**（TTF subset 已 verify，Godot 集成标准模式）

---

## 9. 包大小影响

```
v1.0 ship bundle:                          8.0 MB
+ Fredoka Bold (subset)                  +33 KB
+ Fredoka SemiBold (subset)              +33 KB
+ Nunito Regular (subset)                +39 KB
+ Nunito Medium (subset)                 +39 KB
+ Nunito Bold (subset)                   +39 KB
+ OFL.txt (118 lines)                    +3 KB
+ docs/CREDITS.md (123 lines)            +3 KB
────────────────────────────────────────────────
v1.1 bundle (post TTF polish #3):         8.19 MB

预算: 12 MB
节省: 32% 预算保持
```

---

## 10. 变更日志

### v0.1 (2026-10-06, Christine D+17)

D+17 周日 TTF #3 prep:
- Downloaded Fredoka (Bold 700 + SemiBold 600) + Nunito (Regular 400 + Medium 500 + Bold 700) from Google Fonts
- Subset all 5 fonts to Latin + Latin Extended-A (~319 chars) via fontTools pyftsubset
- Saved subset TTFs to `assets/fonts/` (182 KB total)
- Created `assets/fonts/OFL.txt` (SIL OFL 1.1 license + per-font copyright)
- Created `docs/CREDITS.md` (per Mark D+11 attribution rule)
- Created `docs/design/font-integration-spec.md` (Jacob handoff, this file)
- Verified subset character coverage (122 test chars including es / pt-BR accents)

**未决**：
- Jacob 实施 ThemeBuilder.cs + Theme application
- Android 真机测试
- v1.1 polish scope 整合

---

**TTF prep 完。** Jacob verify + D+18 实施。