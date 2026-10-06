# ADR-0014: Language Picker + Settings Screen (v1.2 Polish #4)

> **Status**: ✅ Accepted
> **Date**: 2026-10-10 (D+26 周六)
> **Deciders**: Mark + Christine (designer) + Jacob (implementation)

## Context

v1.1 LocaleManager.cs（D+19 Jacob 已 done）提供了 `SetLocaleAsync(string locale)` 方法，支持 en/es/pt-BR 三语切换。但**没有 UI 暴露**给用户切换语言：
- 用户首次启动时跟随 OS default locale（不能选）
- 想切换语言必须卸载/重装 + 改 OS 设置
- 西方主要 casual mobile market 用户（Latin American, Brazilian）很在意"默认英文" vs "多语言"作为 first-impression

约束：
- v1.2 polish cycle 9 天（D+22-D+31 ship），time budget 紧
- 必须复用 v1.1 已有的 SettingsScreen 模式（`MenuScreen.cs` 是 GameMenu 入口）
- 用户选择必须持久化（重启不重设）
- 切换语言不需重启（realtime 当前 update）

## Decision

采用 **Language Picker + Locale 持久化** 模式：

### 1. LanguagePickerFeature (new)

```csharp
[GodotFeature(Order = 850, Category = "ui")]
public partial class LanguagePickerFeature : Node { ... }
```

- Auto-bootstrap 反射（per ADR-0004 FeatureBootstrap 反射架构）
- Order=850 < Match3Feature Order=2000（先启动）
- 启动时读 `user://locale.cfg` 自动 apply

### 2. SettingsScreen (new)

```csharp
public partial class SettingsScreen : Screen<SettingsArgs> { ... }
```

- 弹窗式 modal
- 3 RadioButton (English / Español / Português)
- Apply 按钮确认
- Cancel 按钮返回 current screen

### 3. LocalePreferences (new)

```csharp
public static class LocalePreferences
{
    public static async Task<string> LoadAsync();
    public static async Task SaveAsync(string locale);
    private static string FilePath = "user://locale.cfg";
}
```

- 持久化到 `user://locale.cfg`（Godot 标准 user dir）
- JSON format: `{ "locale": "es" }` or `{ "locale": "auto" }`
- 异步 IO（不阻塞 main thread）

### 4. LocaleManager.cs 集成（D+26 patch）

```csharp
// Modified LocaleManager._Ready (existing v1.1 code)
public override void _Ready()
{
    var savedLocale = await LocalePreferences.LoadAsync();
    if (savedLocale != null) {
        SetLocale(savedLocale);  // override OS default
    } else {
        SetLocale(OS.GetLocale().Substring(0, 2));  // OS default fallback
    }
}
```

### 5. SettingsScreen 入口

- TitleScreen 加 "Settings" 按钮（icon 按钮，wrench icon）
- Pause 菜单加 "Settings" 选项
- EndScreen 加 "Settings" 选项

## Consequences

**Positive**:
- ✅ 用户可手动切换 3 语言（en/es/pt-BR）
- ✅ 持久化到 `user://locale.cfg`（重启不重设）
- ✅ 实时切换（不需重启）
- ✅ 复用 Settings 架构（Adrian MDR-0006 ScreenManager）
- ✅ 0 bundle 增加（PO files 已有）
- ✅ 集成 LocaleManager + LocalePreferences（解耦模块）

**Negative**:
- ❌ v1.0/v1.1 老用户升级时无 `user://locale.cfg`（fallback to OS default）
- ❌ Settings UI 增加 complexity（+1 screen + 2 PO entries）
- ❌ "Settings" 按钮需要 icon + label i18n（+2 new strings）

**Neutral**:
- 🟢 Auto-detect still works as default（OS locale）
- 🟢 Manual override 优先级高于 auto-detect
- 🟢 3 radio buttons 显示当前 selection

## Alternatives Considered

### Alternative A: Language picker in title screen (污染 UX)

- ❌ Rejected：title screen 应 clean + focused on Play
- ❌ Rejected：3 radio buttons + Apply 太大，占用 title screen space
- ✅ Rejected：modal Settings 更适合 "occasional config"

### Alternative B: Auto-detect only (无手动切换)

- ❌ Rejected：v1.1 i18n value 减少（用户无法 override）
- ❌ Rejected：polyglot 用户（Latin American 在 US 设备 OS Latin American settings）难以切换
- ✅ Rejected：auto-detect is fallback not primary

### Alternative C: per-locale separate app listing (Google Play auto-filter)

- ❌ Rejected：3 个 separate APK 维护成本高
- ❌ Rejected：Google Play discoverability 减少
- ✅ Rejected：不是 first-class UX

### Alternative D: Notification "Switch language?" prompt at first launch

- ❌ Rejected：interruption 是 bad UX
- ❌ Rejected：v1.1 i18n 本来就 auto-detect，prompt 是 redundant
- ✅ Rejected：用户已经接受 OS default

## Cross-references

- `src/Features/Settings/LanguagePickerFeature.cs` (NEW, Jacob D+26-D+28)
- `src/Features/Settings/SettingsScreen.cs` (NEW, Jacob D+26-D+28)
- `src/Core/LocaleManager.cs` (D+19 Jacob, extended D+26-D+28)
- `src/Core/LocalePreferences.cs` (NEW, Jacob D+26-D+28)
- `src/Features/Match3/Screens/MenuScreen.cs` (D+21 existing, "+ Settings" entry)
- `locale/en.po` (D+26 +2 strings: settings.title + settings.language)
- `locale/es.po` (D+26 +2 strings)
- `locale/pt-BR.po` (D+26 +2 strings)
- `docs/design/i18n-strategy.md` §3 caveats
- `docs/adr/0006-screen-manager-two-tier.md` (ScreenManager pattern reuse)
- `docs/adr/0009-i18n-strategy.md` (v1.1 3 langs baseline)
- `docs/adr/0013-splash-i18n.md` (splash i18n parallel)
- `docs/RELEASE_NOTES_v1.1.md` §3 caveat #4 (no language picker = v1.1 known issue)

## Future Work

### v1.3+: Auto-detect from OS locale + manual override

- LocaleManager._Ready 优先用 LocalePreferences（已 in v1.2）
- LocalePreferences null → OS.GetLocale()（已 in v1.2）
- 新增 OS locale change 监听 (rare)
- 后续 polish 可能加 "Reset to OS default" 选项

### v2.0: 8 langs + CJK

- 8 langs = en + es + pt-BR + fr + de + it + ja + zh-CN
- Settings UI 加 scrollable list (8 items 太多)
- CJK fonts 集成（Noto Sans SC/JP/KR subset ~2.7 MB）

### v3.0: per-game language preferences

- 用户可设置游戏文本 + UI 文本不同语言（rare use case）
- 不在 v1.x 路线图