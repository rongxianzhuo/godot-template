# Per-Theme TTF Subsets Spec

> **作者**：Christine (Artist)
> **Date**：2026-10-08 (D+23 周四)
> **Status**: 📋 Spec / ready for Jacob handoff
> **Source**: docs/design/v1.2-polish-backlog-spec.md §"Per-theme TTF overrides" (per `docs/RELEASE_NOTES_v1.1.md` §4 v1.2 polish backlog #2)

---

## 🎯 目的

v1.1 ship 时所有主题（Forest / Desert / Ocean）共用同一套字体（Fredoka + Nunito）。v1.2 polish #2 引入 **per-theme font selection** 基础设施：
1. 为每个主题选择 primary + secondary 字体
2. 在 ThemeBuilder.cs（D+18 Jacob 已 done）扩展 `GetThemeFonts(themeId)` 方法
3. 3 主题有视觉权重差异化（per Mark D+23 拍板）

**注意**：本 spec 是 **v1.2 phase 1**（per-theme weight remap）。**v1.3+ phase 2** 可以扩展为 per-theme font families（Forest=serif / Desert=italic / Ocean=display），见 ADR-0012 §"Future"。

---

## 📊 Per-Theme Font 映射表（per Mark D+23 拍板）

| Theme | Primary (display) | Secondary (body) | 字符覆盖 |
|-------|-------------------|------------------|----------|
| **Forest** (Levels 1-20) | Fredoka Bold (700) | Nunito Regular (400) | Latin + Latin Extended-A |
| **Desert** (Levels 21-40) | Fredoka SemiBold (600) | Nunito Medium (500) | Latin + Latin Extended-A |
| **Ocean** (Levels 41-60) | Fredoka Bold (700) | Nunito Bold (700) | Latin + Latin Extended-A |

**风格说明**：
- **Forest**：standard weight（Bold display + Regular body），calm forest aesthetic
- **Per-theme weight**（Forest=0 + Desert=SemiBold + Ocean=Bold 各走一步）→ 视觉差异化
- **Ocean**：Bold display + Bold body，**heavier** ocean depths aesthetic

---

## 📁 新增文件结构

```
assets/fonts/
├── Fredoka-Bold.ttf              (existing v1.1, 33 KB)
├── Fredoka-SemiBold.ttf          (existing v1.1, 33 KB)
├── Nunito-Regular.ttf            (existing v1.1, 39 KB)
├── Nunito-Medium.ttf             (existing v1.1, 39 KB)
├── Nunito-Bold.ttf               (existing v1.1, 39 KB)
├── OFL.txt                       (existing v1.1, 118 lines)
│
├── forest/                       (NEW v1.2)
│   ├── Fredoka-Bold.ttf          (copy, 33 KB)
│   ├── Fredoka-Bold.ttf.import   (NEW)
│   ├── Nunito-Regular.ttf        (copy, 39 KB)
│   └── Nunito-Regular.ttf.import (NEW)
│
├── desert/                       (NEW v1.2)
│   ├── Fredoka-SemiBold.ttf      (copy, 33 KB)
│   ├── Fredoka-SemiBold.ttf.import (NEW)
│   ├── Nunito-Medium.ttf         (copy, 39 KB)
│   └── Nunito-Medium.ttf.import  (NEW)
│
└── ocean/                        (NEW v1.2)
    ├── Fredoka-Bold.ttf          (copy, 33 KB)
    ├── Fredoka-Bold.ttf.import   (NEW)
    ├── Nunito-Bold.ttf           (copy, 39 KB)
    └── Nunito-Bold.ttf.import    (NEW)
```

**Total bundle delta**: +6 TTF copies (with duplication Fredoka-Bold × 2) ≈ +33 KB

---

## 🎨 ThemeBuilder.cs 集成（Jacob D+24-D+25 work）

### 当前 v1.1 ThemeBuilder.cs

```csharp
// src/Core/ThemeBuilder.cs (Jacob D+18)
public static class ThemeBuilder
{
    public static Theme BuildTheme(GameState.Theme theme)
    {
        return theme switch
        {
            GameState.Theme.Forest => ThemeColors.Get(theme) + FredokaNunitoFonts.Default,
            GameState.Theme.Desert => ThemeColors.Get(theme) + FredokaNunitoFonts.Default,
            GameState.Theme.Ocean  => ThemeColors.Get(theme) + FredokaNunitoFonts.Default,
            _ => Theme.Default
        };
    }
}
```

### v1.2 扩展（per Christine prep）

```csharp
// v1.2: per-theme font selection
public static class ThemeBuilder
{
    public static Theme BuildTheme(GameState.Theme theme)
    {
        var colors = ThemeColors.Get(theme);
        var fonts = GetThemeFonts(theme);
        return new Theme { Colors = colors, Fonts = fonts };
    }
    
    private static FontPair GetThemeFonts(GameState.Theme theme)
    {
        return theme switch
        {
            GameState.Theme.Forest => new FontPair
            {
                Primary = LoadFont("res://assets/fonts/forest/Fredoka-Bold.ttf"),
                Secondary = LoadFont("res://assets/fonts/forest/Nunito-Regular.ttf")
            },
            GameState.Theme.Desert => new FontPair
            {
                Primary = LoadFont("res://assets/fonts/desert/Fredoka-SemiBold.ttf"),
                Secondary = LoadFont("res://assets/fonts/desert/Nunito-Medium.ttf")
            },
            GameState.Theme.Ocean => new FontPair
            {
                Primary = LoadFont("res://assets/fonts/ocean/Fredoka-Bold.ttf"),
                Secondary = LoadFont("res://assets/fonts/ocean/Nunito-Bold.ttf")
            },
            _ => FontPair.Default
        };
    }
    
    private static FontFile LoadFont(string path)
    {
        return GD.Load<FontFile>(path);
    }
}

public record FontPair(FontFile Primary, FontFile Secondary)
{
    public static FontPair Default => new(FredokaNunitoFonts.Bold, FredokaNunitoFonts.Regular);
}
```

### UID placeholder strategy（per Jacob D+7 + D+17 rules）

- Placeholder UID: `uid://christine_<font>_v1_pertheme`（per Christine D+23）
- Path hash: `00000000000000000000000000000012` (12 = per-theme phase)
- Jacob merge前 `godot --headless --import` regenerate

---

## 🔍 字符覆盖 verify

每个主题的字体覆盖：
- Latin: U+0020-007E (95 chars)
- Latin Extended-A: U+0100-017F (en chars with accents)
- 3 themes 共享同一切元集（en / es / pt-BR 同字符）

**Total**: 319 chars × 6 fonts = 1,914 chars coverage（vs 474 KB 原版 5,000+ chars）

---

## 📦 Bundle 演进

```
v1.1 (D+21): 8.42 MB
+ per-theme TTF (6 文件 +33 KB duplicate): +33 KB
─────────────────────────────────────────────
v1.2 (estimate): ~8.45 MB (+33 KB / +0.4%)

预算: 12 MB
节省: ~30%
```

---

## 🤝 协调请求

**Jacob** — D+24-D+25 实施工作：

```bash
cd /root/godot-template
git fetch origin
git checkout art/v1.2-per-theme-ttf

# Review:
ls assets/fonts/{forest,desert,ocean}/  # 4 files each
cat assets/fonts/forest/Fredoka-Bold.ttf.import  # placeholder UID

# Per-theme font selection:
# 1. Modify src/Core/ThemeBuilder.cs (add GetThemeFonts method)
# 2. Add FontPair record class
# 3. Update all 5 scenes to call BuildTheme() with theme parameter
# 4. Verify per-theme font loading

# UID regen + merge:
godot --headless --import
grep "uid=" assets/fonts/{forest,desert,ocean}/*.import
# Should NOT show "christine_*" placeholder after import

# Real Android test:
# 1. Start Magic Match
# 2. Forest Level 1 (Forest theme) → verify Fredoka Bold + Nunito Regular
# 3. Desert Level 25 (Desert theme) → verify Fredoka SemiBold + Nunito Medium
# 4. Ocean Level 45 (Ocean theme) → verify Fredoka Bold + Nunito Bold
# 5. Screenshot compare: 3 themes have visible weight differentiation

# Merge → main:
git checkout main
git merge art/v1.2-per-theme-ttf
git push origin main
```

---

## 📚 Cross-references

- `docs/design/v1.2-polish-backlog-spec.md` §"Per-theme TTF overrides"（source spec）
- `docs/design/font-integration-spec.md` (D+17 Jacob handoff for v1.1 fonts)
- `src/Core/ThemeBuilder.cs` (Jacob D+18 done)
- `docs/adr/0012-per-theme-ttf.md` (per ADR mapping)
- `docs/adr/0009-i18n-strategy.md` (related: 3 languages same Latin chars)
- Mark D+23 per-theme weight 拍板

---

## 附录 A：v1.3+ phase 2 扩展空间

Per Christine D+23 spec 偏差说明：

**v1.2 spec §2 原文**：
> "Per-theme differentiation: Forest = serif; Desert = italic; Ocean = sans"

**Mark D+23 拍板**：per-theme **weight remap** with existing 5 fonts（Forest=Fredoka Bold, Desert=Fredoka SemiBold, Ocean=Fredoka Bold）。

**Rationale**：
- v1.2 phase 1：per-theme infrastructure（weight remap, 0 new fonts）
- v1.3+ phase 2：per-theme **families**（Forest=Lora serif, Desert=Pacifico italic, Ocean=Fredoka display）

**v1.3+ phase 2 实施时**：
- 新增 fonts: Lora Regular + Bold (~50 KB), Pacifico (~30 KB)
- ThemeBuilder.cs 更新 Forest → Lora, Desert → Pacifico
- Bundle delta: +80 KB
- 决策时机：v1.2 ship 后根据 founder feedback

---

## 附录 B：变更日志

### v0.1 (2026-10-08, Christine D+23)

D+23 周四 per-theme TTF subsets prep。

**6 TTF 文件 + 6 .import sidecar 准备**：
- `assets/fonts/forest/`: Fredoka-Bold + Nunito-Regular
- `assets/fonts/desert/`: Fredoka-SemiBold + Nunito-Medium
- `assets/fonts/ocean/`: Fredoka-Bold + Nunito-Bold

**Bundle delta**: +33 KB (Fredoka-Bold × 2 duplication)

**Jacob handoff spec 写完**：
- ThemeBuilder.cs `GetThemeFonts(theme)` 方法
- FontPair record class
- Per-theme UID placeholder convention (`_v1_pertheme`)

**未决**:
- Jacob D+24-D+25 ThemeBuilder.cs 实施
- v1.3+ phase 2（real per-theme families）触发时机

---

**Per-theme TTF subsets spec 完。** Jacob D+24-D+25 实施。