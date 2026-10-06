# TTF Font Research — v1.1 Polish Backlog

> **作者**：Christine (Artist)
> **Date**：2026-10-06 (D+11 周四)
> **状态**：v1.1 polish backlog #2 — 字体升级研究
> **背景**：v1.0 ship 沿用 Godot default 字体；v1.1 升级为自选 TTF 增强 polish

---

## 0. TL;DR

| 决策 | 值 |
|------|-----|
| **v1.1 推荐字体** | **Fredoka Bold**（标题/display） + **Nunito Regular/Medium/Bold**（UI / body） |
| **备选** | Baloo 2 Bold（标题，更厚重） + Nunito（body） |
| **许可证** | SIL Open Font License 1.1（OFL）— 商业可用 + 应用内嵌 + 无 attribution 强制 |
| **字符子集** | v1.1 Latin + Latin Extended-A（~319 chars），约 95 KB 总大小（5 fonts subset 后）|
| **字符子集策略** | pyftsubset（fontTools），生成 Latin-only 子集 → Android assets/fonts/ |
| **包大小影响** | +~95 KB（v1.1 ship 总 ~8.0 MB，含 5 个 fonts subset 后）|
| **Godot 集成** | Godot 4 `FontFile` resource + `theme.set_font("default_font", font)` |
| **实施工作量** | 0.5-1 天（download + subset + Godot scene 更新 + smoke test） |

---

## 1. 候选字体对比

### 1.1 Fredoka ⭐ **推荐 (Title)**

| 属性 | 值 |
|------|-----|
| Designer | Milena Brandão + Hafontia (Ben Nathan) |
| 类型 | Sans-serif rounded |
| Weights | 300 / 400 / 500 / 600 / 700 |
| License | **SIL OFL 1.1**（免费商用 + 应用内嵌 + 修改）|
| 字符子集 | Latin + Latin Extended + Hebrew |
| TTF 大小 | ~49 KB / weight（未子集） |
| WOFF2 大小 | ~23 KB / weight |
| Variable | ✅（154 KB 包含所有 weights） |
| Google Fonts rank | #131 |

**视觉风格**：
- 圆润、友好、温暖的几何 sans-serif
- "casual magic" 风格 = match-3 游戏调性
- 比 Baloo 2 略瘦，比 Nunito 略胖
- 标题 "Magic Match" 用 Fredoka Bold 64 px **极合适**

**适用**：
- ✅ 标题（display）
- ✅ Logo
- ✅ 大号 UI 标签
- ⚠️ Body text（可读但稍重，UI 文字推荐配 Nunito）

### 1.2 Nunito ⭐ **推荐 (Body / UI)**

| 属性 | 值 |
|------|-----|
| Designer | Vernon Adams |
| 类型 | Sans-serif rounded |
| Weights | 200 / 300 / 400 / 500 / 600 / 700 / 800 / 900 |
| License | **SIL OFL 1.1** |
| 字符子集 | Latin + Latin Extended + Vietnamese |
| TTF 大小 | ~150 KB / weight（未子集） |
| Google Fonts rank | ~ #90 |

**视觉风格**：
- 圆润 sans-serif，高 x-height（71%）= 可读性优秀
- 8 weights（含 200 Light + 900 Black）= 灵活
- 比 Fredoka 略瘦，比 Inter 略胖

**适用**：
- ✅ Body text
- ✅ UI labels（HUD 分数 / 计时器）
- ✅ 按钮文字
- ⚠️ Display（可读但太平淡，标题推荐 Fredoka）

### 1.3 Baloo 2（备选 Title）

| 属性 | 值 |
|------|-----|
| Designer | Ek Type (Sarang Kulkarni) |
| 类型 | **Display** rounded |
| Weights | 400 / 500 / 600 / 700 / 800 |
| License | **SIL OFL 1.1** |
| 字符子集 | Latin + Latin Extended + Devanagari + Vietnamese |
| TTF 大小 | ~640 KB / weight（未子集，未子集大因为 Devanagari）|
| WOFF2 大小 | ~197 KB / weight |
| Google Fonts rank | #264 |

**视觉风格**：
- "bouncy baseline"、heavy weight
- 比 Fredoka 更重、更"magical"
- 是**display only**——小号 body 文字可读性下降

**适用**：
- ✅ 大标题（display only）
- ⚠️ Body text（不推荐）
- 风险：粗度可能盖过 logo / icon

**比较**：
- Fredoka vs Baloo 2：Fredoka 更精致 + 更轻；Baloo 2 更 playful + 更重
- 决策：**Magic Match 选 Fredoka**——整体 cosmic 风格偏精致，Fredoka 更 match

### 1.4 排除候选

| 字体 | 排除理由 |
|------|---------|
| **Quicksand** | 几何感太强，缺少 "magical" 温度 |
| **Pacifico** | Script 体 = 难读，不适合 match-3 数字 HUD |
| **Comfortaa** | Display only，body 不可读；与 Fredoka 重复 |
| **Inter** | Sans-serif 太理性，缺 magic 感 |
| **Poppins** | 同上 |

---

## 2. 许可证对比

| 字体 | License | 商业可用 | 应用内嵌 | 修改 | Attribution |
|------|---------|----------|----------|------|-------------|
| Fredoka | **SIL OFL 1.1** | ✅ | ✅ | ✅ | 推荐（不强） |
| Nunito | **SIL OFL 1.1** | ✅ | ✅ | ✅ | 推荐（不强） |
| Baloo 2 | **SIL OFL 1.1** | ✅ | ✅ | ✅ | 推荐（不强） |

**OFL 1.1 关键条款**：
- ✅ 免费商业使用
- ✅ 应用内嵌（mobile app / game）
- ✅ 可修改（subset / 重新打包）
- ✅ 可二次分发（前提：保留 OFL.txt）
- ⚠️ **不可单独销售字体文件本身**

**结论**：3 个候选都是 OFL 1.1，对 Magic Match（移动游戏 + 不单独卖字体）**完全合适**。

**合规 checklist**（v1.1 ship）：
- [ ] 保留每个字体的 `OFL.txt` 文件（在 `assets/fonts/` 目录或 `docs/THIRD_PARTY_LICENSES.md`）
- [ ] 在 Google Play store listing 或 in-app credits 提及 "Fredoka by Milena Brandão + Nunito by Vernon Adams"（推荐非强制）
- [ ] 不将字体单独销售

---

## 3. 字符子集策略

### 3.1 v1.1 Latin only 策略

**范围**：
- ASCII (U+0020 - U+007E)：95 chars（基础英文 + 数字 + 标点）
- Latin-1 Supplement (U+00A0 - U+00FF)：96 chars（含 © ® × ÷ 等）
- Latin Extended-A (U+0100 - U+017F)：128 chars（Eastern European 字符）

**合计**：~319 chars / weight

**工具**：`pyftsubset` (fontTools / Brotli)

```bash
pip install fonttools brotli
pyftsubset Fredoka-Bold.ttf \
  --unicodes="U+0020-007E,U+00A0-00FF,U+0100-017F" \
  --output-file=Fredoka-Bold-subset.ttf \
  --flavor=ttf  # 不使用 woff2 (Godot 不直接吃 woff2)
```

**subset 大小**（估算）：

| 字体 | 原 TTF | subset TTF | 节省 |
|------|--------|------------|------|
| Fredoka Bold | 49 KB | ~10 KB | 80% |
| Fredoka Regular | 49 KB | ~10 KB | 80% |
| Nunito Regular | 150 KB | ~25 KB | 83% |
| Nunito Bold | 150 KB | ~25 KB | 83% |
| Nunito SemiBold | 150 KB | ~25 KB | 83% |

**总计**（5 个 fonts）：
- Fredoka Regular + Bold = ~20 KB
- Nunito Regular + Medium + Bold = ~75 KB
- 共 **~95 KB**（含 .ttf，未压缩）

### 3.2 v2.0 Latin + CJK 策略（future）

**CJK 字符数**：
- 简体中文 GB2312 (U+4E00 - U+9FFF + U+3000 - U+303F)：~21,000 chars（常用 ~3,500）
- 繁体 + 日文 + 韩文：~50,000 chars 总

**CJK 子集大小估算**（3,500 常用简体）：
- Fredoka 不含 CJK → 必须额外引入 Noto Sans SC
- Noto Sans SC Regular subset（3,500 chars）：~700 KB
- Noto Sans SC Bold subset：~700 KB
- **CJK 总计**：~1.4 MB（**比 Latin 大 15x**）

**v2.0 风险**：CJK 字体让包大小从 8 MB 跳到 ~10 MB——值得但需要 Mark 决策。

### 3.3 不做的子集优化（避免）

- ❌ 不做 hinting 优化（Godot 4.7 在 Android 上 hinting 自动处理）
- ❌ 不做 OpenType feature 移除（保留 liga / frac / tnum = 玩家在 HUD 看到分数分隔符）
- ❌ 不做 ligature 移除（保留 "fi" "fl" ligature = 文字更美观）

---

## 4. Godot 4.7 集成方式

### 4.1 三种集成方式对比

| 方式 | 优点 | 缺点 | 适合 |
|------|------|------|------|
| **SystemFont (Android 系统字体)** | 0 包大小 | 跨设备字体不一致、emoji 错乱 | ❌ 不推荐（无品牌感） |
| **Dynamic Font (运行时 load .ttf)** | 自定义字体、品牌一致 | 增加包大小 | ✅ **推荐** |
| **Bitmap Font (预渲染 PNG)** | 渲染最快 | 字号受限、不能放大 | ❌ 不适合 TTF 升级 |

### 4.2 Dynamic Font 实施

**Step 1**: 把 subset .ttf 放到 `assets/fonts/`

```
assets/fonts/
├── Fredoka-Bold-subset.ttf
├── Fredoka-Regular-subset.ttf
├── Nunito-Bold-subset.ttf
├── Nunito-Medium-subset.ttf
├── Nunito-Regular-subset.ttf
└── OFL.txt  # 三个字体的 OFL license（合并）
```

**Step 2**: 在 Godot 创建 Theme

```csharp
// src/Core/ThemeBuilder.cs (新文件, Jacob 实现)
public static class ThemeBuilder {
    public static Theme BuildMagicTheme() {
        var theme = new Theme();
        
        var fredokaBold = new FontFile();
        fredokaBold.LoadDynamicFont("res://assets/fonts/Fredoka-Bold-subset.ttf");
        var fredokaRegular = new FontFile();
        fredokaRegular.LoadDynamicFont("res://assets/fonts/Fredoka-Regular-subset.ttf");
        
        var nunitoRegular = new FontFile();
        nunitoRegular.LoadDynamicFont("res://assets/fonts/Nunito-Regular-subset.ttf");
        var nunitoMedium = new FontFile();
        nunitoMedium.LoadDynamicFont("res://assets/fonts/Nunito-Medium-subset.ttf");
        var nunitoBold = new FontFile();
        nunitoBold.LoadDynamicFont("res://assets/fonts/Nunito-Bold-subset.ttf");
        
        // Default font for body text
        theme.SetFont("default_font", "Label", nunitoRegular);
        
        // Title (Label.Theme override)
        theme.SetFont("font", "TitleLabel", fredokaBold);
        theme.SetFontSize("font_size", "TitleLabel", 64);
        
        return theme;
    }
}
```

**Step 3**: 在 GameScenes.cs 应用主题

```csharp
// 每个 scene 加 theme = ThemeBuilder.BuildMagicTheme();
```

### 4.3 ⚠️ Android 打包警告

**Android APK 加载 .ttf 文件必须放在 `assets/` 目录**（不是 `res://` 根）。

Godot 4.7 export 引擎自动把 `assets/fonts/*.ttf` 打包到 Android APK assets/fonts/。

**测试**：必须在真机 / Android emulator 测试（不止 Godot 编辑器）—— 某些 Android 设备对 TTF subset 兼容性差异。

---

## 5. 推荐 + 理由 + 风险评估

### 5.1 推荐组合：Fredoka Bold + Nunito

**理由**：
1. **品牌契合**：Fredoka 圆润 + cosmic purple 主题 = "casual magic" 调性完美匹配
2. **可读性**：Nunito 8 weights + 高 x-height = HUD / 数字 / 按钮可读性强
3. **包大小**：~95 KB total（5 fonts subset）= 节省 87% vs 不 subset
4. **License**：OFL 1.1 = 商业 + 应用内嵌无风险
5. **Godot 集成**：FontFile + Theme.SetFont = 标准 0.5 天工作

### 5.2 备选组合：Baloo 2 Bold + Nunito

**何时考虑**：
- Mark / 玩家反馈 "Fredoka 太轻，希望更厚重" → 切 Baloo 2
- 但 v1.1 ship 推荐先上 Fredoka，Baloo 2 留 v1.2 polish

### 5.3 风险评估

| 风险 | 概率 | 影响 | 缓解 |
|------|------|------|------|
| **Android 设备字体渲染差异** | 🟡 Medium | 视觉不一致 | 真机测试 ×3 设备（Samsung / Pixel / Xiaomi） |
| **TTF subset 工具链失败** | 🟢 Low | 实施延期 | 用 fontTools CLI（成熟工具） |
| **Godot 4.7 FontFile 兼容问题** | 🟢 Low | 需 work-around | 文档已查 Godot 4.7 stable API |
| **包大小超 12 MB 预算** | 🟢 Low（+95 KB 仍 < 9 MB） | ship 阻塞 | 已在 v1.0 节省 34% |
| **Title font 字号与原设计不 match** | 🟡 Medium | 视觉返工 | v1.1 视觉评审由 Christine + Mark 拍板 |
| **字体加载慢（cold start +500ms）** | 🟡 Medium | splash 时间 +500ms | splash screen 加长 0.5s |

**综合风险等级**：🟢 **低-中**（可 v1.1 ship 后立刻验证，worst case fallback 回 Godot default 字体——font files 在 assets/ 但 theme 不引用 = 不增加冷启动成本）

### 5.4 Fallback 策略

如果 v1.1 TTF 集成出问题（如 Android 渲染 bug）：

```csharp
// 保留 Godot default 字体作为 fallback
var defaultFont = ThemeDB.GetDefaultTheme().GetFont("font", "Label");
var actualFont = OS.HasFeature("mobile") ? nunitoRegular : defaultFont;
```

**回退成本**：0——TTF 文件可保留但不引用。

---

## 6. 实施工作量估算

### 6.1 v1.1 TTF 集成（Jacob 主要工作）

| 任务 | 工时 |
|------|------|
| Download 5 个 .ttf 字体 from Google Fonts | 0.5h |
| pyftsubset 子集化（Latin + Latin Extended-A） | 0.5h |
| 编写 ThemeBuilder.cs（Godot C#） | 1h |
| 在所有 scene 替换 default font（Title / Game / LevelSelect / HUD / EndScreen / ThemeBanner / Splash） | 2h |
| 真机测试 ×3（Android emulator + 真机 ×2） | 1h |
| 视觉验收（Christine + Mark） | 0.5h |
| **总计** | **0.5-0.7 天** |

### 6.2 v1.1 ship blocker？

❌ **不是**。v1.0 ship 用 Godot default，v1.1 升级 TTF。

### 6.3 与 D+10 release notes "Known Issues #2" 关系

RELEASE_NOTES_v1.0.md 已知问题 #2："Godot default font (not TTF)" — severity 🟡 Low, plan v1.1 (TTF upgrade)。本 research doc 是该 plan 的 detail。

---

## 7. 实施 checklist (Jacob, v1.1)

- [ ] Download Fredoka (5 weights) + Nunito (8 weights) from Google Fonts
- [ ] pyftsubset to Latin + Latin Extended-A（unicodes per §3.1）
- [ ] 复制 OFL.txt 到 `assets/fonts/OFL.txt`（合并 3 个字体）
- [ ] 创建 `src/Core/ThemeBuilder.cs`（per §4.2）
- [ ] 在 `assets/scenes/*.tscn` 应用 Theme（每个 scene 都要）
- [ ] Godot 编辑器 verify 文字渲染
- [ ] Android emulator verify 文字渲染（重要——Android 系统字体环境不同）
- [ ] 真机 verify ×1（Samsung / Pixel 任一）
- [ ] `make verify` smoke test 全绿
- [ ] 视觉验收：Christine + Mark
- [ ] commit + push to `art/v1.1-ttf-integration` branch
- [ ] 通知 Christine + Mark 验收

---

## 8. 决策点（Mark 拍板）

### 8.1 必拍板

1. **字体选择**：Fredoka + Nunito ✅ / Baloo 2 + Nunito / 其他？
2. **weights 数量**：5 个（Fredoka 2 + Nunito 3）vs 3 个（Fredoka Bold + Nunito Regular + Bold）？推荐 5。

### 8.2 可延后

3. v1.1 上 TTF 还是 v1.2 上？（Mark D+7 拍板 v1.1 polish backlog 启动，本 doc 默认 v1.1 上）
4. CJK v2.0 是否在 v1.1 同时 prep？（推荐：v1.1 仅 Latin，v2.0 加 CJK）

### 8.3 视觉验收标准

- [ ] 标题 "Magic Match"（Fredoka Bold 64 px）与 splash build spec match
- [ ] HUD 数字（"1234" score, "00:30" timer, "3/3" lives）Nunito Bold 28 px 可读
- [ ] 按钮 "Play" / "Continue" Nunito SemiBold 32 px 居中
- [ ] ThemeBanner 主题切换 "Forest" / "Desert" / "Ocean" Nunito Bold 36 px
- [ ] EndScreen "You Win!" Fredoka Bold 64 px + Nunito body 24 px

---

## 9. 风险监控

**v1.1 ship 后监控**：
- Crash report：font load failure
- 视觉回归：玩家反馈"字看不清" / "字太小"
- Android 设备兼容性：低端 Android 8/9 设备字体渲染

**rollback 触发**：Android font load 失败率 > 1% → fallback 到 Godot default。

---

## 附录 A：变更日志

### v0.1 (2026-10-06, Christine)

D+11 周四下午 TTF font research。

**新增**：
- §0 TL;DR
- §1 4 个候选字体对比（Fredoka / Nunito / Baloo 2 / 排除）
- §2 OFL license 对比
- §3 字符子集策略（v1.1 Latin + v2.0 CJK 估算）
- §4 Godot 4.7 集成方式（Dynamic Font 推荐）
- §5 推荐 + 理由 + 风险评估
- §6 实施工作量估算
- §7 实施 checklist for Jacob
- §8 决策点 for Mark
- §9 风险监控

**决策**：
- ✅ 推荐 Fredoka Bold + Nunito Regular/Medium/Bold
- ✅ v1.1 Latin only subset（~95 KB total）
- ❌ 不做 v1.1 CJK prep（推 v2.0）

**未决**：
- 字符子集是否包含额外符号（emoji / 特殊数学符号）—— 默认包含 Latin Extended-A 已足够
- weights 数量最终拍板（Mark §8.1）

---

## 附录 B：参考链接

- [Fredoka on Google Fonts](https://fonts.google.com/specimen/Fredoka)
- [Nunito on Google Fonts](https://fonts.google.com/specimen/Nunito)
- [Baloo 2 on Google Fonts](https://fonts.google.com/specimen/Baloo+2)
- [SIL OFL 1.1 License](https://scripts.sil.org/OFL)
- [fontTools / pyftsubset](https://github.com/fonttools/fonttools)
- [Godot 4 FontFile docs](https://docs.godotengine.org/en/stable/classes/class_fontfile.html)

---

**TTF font research 完。** Mark verify 后 commit 到 main。v1.1 实施前需要 Mark 拍板 §8.1 + Jacob 拍板时间表。