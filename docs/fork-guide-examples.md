# Fork Guide — Concrete Walkthroughs

> **作者**：Christine (Artist)
> **Date**：2026-10-06 (D+18 周一)
> **状态**：how-to guide for godot-template fork users
> **配合阅读**：`FORK_CHECKLIST.md`（step-by-step checklist）+ `AGENTS.md`（agent working contract）

---

## 0. TL;DR

This guide answers **"I just forked godot-template — what does each step actually do?"** with concrete walkthroughs, expected output, and annotated code samples.

**Prerequisite**：已经 `git clone` 了 godot-template 的 fork，本地 `cd /path/to/your-fork`。

**Outline**：
1. Initial state — what your fork looks like at `git clone`
2. `scripts/configure.sh` — identity injection (5 min)
3. Delete demo residue — what goes away
4. Edit hardcoded identity — exact field changes
5. Replace icon — 1 min
6. Update LICENSE — 30 sec
7. Run `make verify` — what success looks like
8. Add a new feature — FeatureBootstrap pattern walkthrough
9. Push first commit + open PR

---

## 1. Initial state — what your fork looks like

After `git clone https://github.com/YOUR_ORG/godot-template-fork.git`:

```
your-fork/
├── addons/godot-framework/        # git submodule (Framework: ServiceRegistry, EventBus, ScreenManager)
├── assets/                        # game assets (gems / backgrounds / UI / icons / fonts)
├── docs/                          # design specs (ART_SPEC, splash, i18n, etc.)
├── locale/                        # i18n .po files (en / es / pt-BR)
├── scripts/                       # build pipeline + configure.sh + purple-scanner
├── src/
│   ├── Core/                      # Bootstrap.cs + GodotFeature.cs (auto-discovery)
│   └── Features/
│       ├── Counter/               # ⚠️ demo: frame counter (delete!)
│       └── Match3/                # 🎮 the actual game (keep or replace)
├── tests/                         # algorithm tests
├── AGENTS.md                      # agent working contract (READ FIRST)
├── FORK_CHECKLIST.md              # step-by-step fork checklist
├── LICENSE                        # MIT, copyright rongxianzhuo 2026
├── Makefile                       # make setup / test / smoke / build / verify
├── project.godot                  # Godot project config
└── export_presets.cfg             # Android export presets
```

**Hardcoded identity to replace**：
- `project.godot`: `config/name="GodotTemplate"`, `[android] package/unique_name="com.example.emptygodot"`
- `export_presets.cfg`: `export_path="builds/MagicMatch-debug.apk"`, `package/unique_name="org.magicstudio.match3"`
- `LICENSE`: `Copyright (c) 2026 rongxianzhuo`
- `AGENTS.md` / `README.md`: references to "MagicStudio" / "Magic Match"

---

## 2. `scripts/configure.sh` — identity injection (5 min)

**Command**：

```bash
bash scripts/configure.sh \
    --name "Your Game" \
    --package "com.yourorg.yourgame" \
    --author "Your Name" \
    --description "A short tagline"
```

**Expected output**：

```
configuring app identity:
  name:        Your Game
  package:     com.yourorg.yourgame
  author:      Your Name
  description: A short tagline
  keystore:    /root/.android/debug.keystore
  filename:    your-game

[configure] rewriting export_presets.cfg ... ok
[configure] rewriting project.godot ... ok
[configure] rewriting LICENSE ... ok
[configure] generating keystore at /root/.android/debug.keystore ... ok
[configure] DONE
```

**What changed**（自动完成，无须手动 edit）：

| File | Field | Before | After |
|------|-------|--------|-------|
| `export_presets.cfg` | `[preset.0] export_path` | `builds/MagicMatch-debug.apk` | `builds/your-game-debug.apk` |
| `export_presets.cfg` | `[preset.0.options] package/unique_name` | `org.magicstudio.match3` | `com.yourorg.yourgame` |
| `export_presets.cfg` | `[preset.0.options] package/name` | `Magic Match` | `Your Game` |
| `export_presets.cfg` | `[preset.0.options] keystore/debug` | `/root/build-godot/keystore/debug.keystore` | `/root/.android/debug.keystore` |
| `project.godot` | `[application] config/name` | `GodotTemplate` | `Your Game` |
| `project.godot` | `[application] config/description` | `A Godot 4 + C# + Android project template.` | `A short tagline` |
| `project.godot` | `[android] package/unique_name` | `com.example.emptygodot` | `com.yourorg.yourgame` |
| `project.godot` | `[android] package/name` | `EmptyGodot` | `Your Game` |
| `LICENSE` | Copyright line | `Copyright (c) 2026 rongxianzhuo` | `Copyright (c) 2026 Your Name` |

**Verify**（必须 zero hits）：

```bash
grep -rn 'MagicMatch\|magicstudio\|emptygodot\|EmptyGodot\|build-godot' . \
    --include='*.cfg' --include='*.godot' --include='*.tres' \
    --include='AGENTS.md' --include='README.md'
# should print nothing
```

**Re-run 安全**：脚本是 idempotent —— 重复运行同参数不会破坏。

---

## 3. Delete demo residue (5 min)

**Step 1：删 trivial demo `Counter/`**（仅 frame counter，删掉不留痕）：

```bash
git rm -r src/Features/Counter/
```

**Step 2：删 Match-3 smoke test**（specific to this game）：

```bash
git rm src/Features/Match3/Match3SmokeTest.cs
git rm src/Features/Match3/Match3SmokeTest.cs.uid
```

**Step 3：（optional）删整个 Match-3 game**（if you're building a different genre）：

```bash
git rm -r src/Features/Match3/
git rm -r tests/Match3Tests/
git rm -r assets/gems/ assets/backgrounds/
git rm docs/ART_SPEC.md docs/ASSET_INTEGRATION.md
```

**Step 4：清 import cache**（per `docs/ASSET_INTEGRATION.md`）：

```bash
godot --headless --import
```

**Verify**：

```bash
make test && make smoke   # 期望 exit 0（如果保留 tests/）
# or
ls src/Features/           # 期望只有你自己的 features
```

---

## 4. Edit hardcoded identity (2 min)

虽然 `configure.sh` 已经做了一部分，但有几个字段仍需手动：

### 4.1 `export_presets.cfg`

| Section | Field | Edit |
|---------|-------|------|
| `[preset.0]` | `name` | `"Android Debug"`（保持默认）或 `"Your Game Debug"` |
| `[preset.0.options]` | `version/code` | `1`（每次 release +1）|
| `[preset.0.options]` | `version/name` | `"1.0.0"`（semver）|

### 4.2 `project.godot`

| Section | Field | Edit |
|---------|-------|------|
| `[application]` | `config/version` | `"1.0.0"` |
| `[application]` | `run/main_scene` | `"res://src/Features/YourGame/YourGameMain.tscn"`（替换 Match3）|

### 4.3 `AGENTS.md`（optional）

替换 §"Project Identity" 段落中的 "Magic Match" / "MagicStudio" 引用：

```bash
sed -i 's/Magic Match/Your Game/g; s/MagicStudio/YourOrg/g' AGENTS.md
git add AGENTS.md
git commit -m "docs: replace MagicStudio references with YourOrg"
```

---

## 5. Replace icon (1 min)

```bash
# 1. 准备你的 1024×1024 SVG icon
#    （推荐：hand-drawn SVG → rsvg-convert 或 PIL）
#    规格参考：docs/ART_SPEC.md §10

# 2. 替换 launcher icon
cp path/to/your/icon.svg assets/icons/icon.svg

# 3. 替换 root icon（export_presets.cfg 引用）
cp path/to/your/icon.svg icon.svg

# 4. 重新 import
godot --headless --import
```

**Verify**：

```bash
# Android emulator 应该显示新 icon
make build
adb install builds/your-game-debug.apk
adb shell am start -n com.yourorg.yourgame/.GodotApp
```

---

## 6. Update LICENSE (30 sec)

Edit `LICENSE` 把 copyright 改成你的：

```diff
- Copyright (c) 2026 rongxianzhuo
+ Copyright (c) 2026 Your Name or Your Organization
```

**License 选择**：
- **MIT**（推荐，与 upstream 一致，最 permissive）
- **Apache 2.0**（如果你要 patent grant）
- **Proprietary**（如果你要 closed-source）

---

## 7. Run `make verify` — what success looks like

**Command**：

```bash
make verify
# = make test + dotnet build
```

**Expected output**：

```
godot-template — running test suite...

=== Algorithm tests ===
[Match3Tests] BoardTests.SwapTest ... ok
[Match3Tests] BoardTests.MatchDetection ... ok
[Match3Tests] MatchEngineTests.SimpleMatch ... ok
[Match3Tests] ScoreManagerTests.Increment ... ok
... (93 cases total)
[Match3Tests] 93/93 passed

=== .NET build ===
  Determining projects to restore...
  Restored /root/godot-template/GodotTemplate.csproj
  GodotTemplate -> /root/godot-template/bin/Debug/net9.0/GodotTemplate.dll

Build succeeded.
  0 Warning(s)
  0 Error(s)

=== make verify: PASS ===
```

**Troubleshooting**：

| Error | Cause | Fix |
|-------|-------|-----|
| `godot: command not found` | Godot 未安装 | `make setup-quick` |
| `polib: No module named polib` | Python 依赖缺失 | `pip install --break-system-packages polib` |
| `dotnet: command not found` | .NET SDK 未安装 | `make setup-quick`（自动安装 .NET 9）|
| `Match3SmokeTest.cs not found` | 已删除但 .csproj 仍引用 | `git rm GodotTemplate.csproj` 然后 `git restore GodotTemplate.csproj` |

---

## 8. Add a new feature — FeatureBootstrap pattern walkthrough

**What is FeatureBootstrap**：所有 gameplay/UI 都封装在 `[GodotFeature]` 装饰的 C# class 里。`src/Core/Bootstrap.cs` 用反射自动发现 + instantiate + add child。

**Step-by-step example：加一个 `ScoreTrackerFeature`（demo purposes）**

**Step 1：创建文件 `src/Features/ScoreTracker/ScoreTrackerFeature.cs`**

```csharp
using Godot;
using GodotTemplate.Core;

namespace GodotTemplate.Features.ScoreTracker;

/// <summary>
/// Displays total accumulated score in the top-right corner.
/// Drop-in feature: this file alone is enough to add this behavior.
/// </summary>
[GodotFeature(Order = 150, Category = "ui")]
public partial class ScoreTrackerFeature : Control
{
    private int _totalScore;
    private Label _label = null!;

    public override void _Ready()
    {
        _label = new Label
        {
            Text = "Score: 0",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _label.SetAnchorsPreset(LayoutPreset.TopRight);
        _label.Position = new Vector2(-200, 20);  // offset from top-right
        _label.AddThemeFontSizeOverride("font_size", 32);
        AddChild(_label);
    }

    public void AddScore(int points)
    {
        _totalScore += points;
        _label.Text = $"Score: {_totalScore}";
    }
}
```

**Step 2：不需要其他文件！** `Bootstrap.cs` 自动发现：

```bash
godot --headless --import
make verify
```

**Expected output**（runtime log）：

```
[Bootstrap] Discovered 1 feature(s):
[Bootstrap]   + ScoreTrackerFeature (order=150)
```

**Step 3：调用 feature**

在其他 feature 里 `GetNode<ScoreTrackerFeature>` 然后调 `AddScore`：

```csharp
// In another feature file
public override async void _Ready()
{
    await ToSignal(GetTree().CreateTimer(1.0), "timeout");
    var scoreTracker = GetTree().Root.GetNode<ScoreTrackerFeature>("ScoreTrackerFeature");
    scoreTracker.AddScore(100);
}
```

**Key insights**：
- ❌ 不需要改 `project.godot`
- ❌ 不需要创建 .tscn
- ❌ 不需要 register Node in scene tree
- ✅ 只要 `[GodotFeature]` attribute + partial class + 继承 `Node` 或 `Control`
- ✅ `Order` 控制启动顺序（小的先）
- ✅ `Category` 用于 grouping（仅文档用途）

---

## 9. Push first commit + open PR

**Step 1：commit fork changes**

```bash
git add -A
git status   # review what's staged

git commit -m "fork: configure identity + delete Counter demo

Applied scripts/configure.sh with:
- name: Your Game
- package: com.yourorg.yourgame

Removed:
- src/Features/Counter/ (trivial frame counter demo)
- src/Features/Match3/Match3SmokeTest.cs (Match-3 specific test)

Updated:
- AGENTS.md (MagicStudio -> YourOrg)
- LICENSE (Copyright line)"

git log --oneline -3
# 应该显示 fork commit + upstream commits
```

**Step 2：push to your fork**

```bash
git remote -v   # 应显示你的 fork URL
git push origin main
```

**Step 3：（optional）open PR to upstream godot-template**

如果你的 fork 修了 upstream bug / 加了 generic utility，可以开 PR：

```bash
# 1. 在 GitHub 上 fork → upstream sync（if not auto）
git fetch upstream  # 仅当你 added upstream remote

# 2. branch for PR
git checkout -b feature/your-contribution
# ... make changes ...
git push origin feature/your-contribution

# 3. 在 GitHub 上：Compare & pull request
#    base: rongxianzhuo/godot-template main
#    head: YOUR_ORG/godot-template-fork feature/your-contribution
```

---

## 10. Common pitfalls (踩过的坑)

### Pitfall 1: 忘记 delete Counter demo

**Symptom**：游戏运行时屏幕中央有个 frame counter

**Fix**：`git rm -r src/Features/Counter/ && godot --headless --import`

### Pitfall 2: 包名 invalid

**Symptom**：`configure.sh --package Foo` 报错 "not a valid Java package"

**Fix**：包名必须 lowercase + dot-separated：`com.yourorg.yourgame`，**不能** `com.YourOrg.YourGame`

### Pitfall 3: 漏改 export_presets.cfg

**Symptom**：build 出来 APK 仍是 `MagicMatch-debug.apk`

**Fix**：跑 `configure.sh`（自动处理），或手动 edit `export_presets.cfg` 的 `export_path` 字段

### Pitfall 4: submodule 没 sync

**Symptom**：`error: addons/godot-framework/GameFramework.dll not found`

**Fix**：`git submodule update --init --recursive`（或 `make sync`）

### Pitfall 5: 编辑 `.tscn` 后 merge conflict

**Symptom**：PR 时 Godot scene 文件冲突，UID 报错

**Fix**：
1. **永远不要**手动 edit `.tscn` 文件的 UID 字段
2. 让 Godot 自己 generate（run `godot --headless --import`）
3. 如果 conflict，先 merge 你的 scene changes，再 run import regenerate UIDs

### Pitfall 6: `make smoke` 卡住

**Symptom**：`make smoke` hang 不退出

**Fix**：Android export templates 未安装
```bash
bash scripts/install-android-templates.sh
```

### Pitfall 7: 测试 stale UIDs

**Symptom**：`Test 'EventBusTest' failed: type 'EventBus' not found`

**Fix**：重新 build
```bash
make clean && make verify
```

---

## 11. Next steps after fork

| Action | Reference |
|--------|-----------|
| Add splash screen | `docs/design/splash-screen-build-spec.md` |
| Add i18n | `docs/design/i18n-strategy.md` |
| Replace icons | `docs/ART_SPEC.md` §10 |
| Add font | `docs/design/ttf-font-research.md` |
| Debug keystore | `FORK_CHECKLIST.md` §3 |
| Release keystore | `docs/release-keystore.md`（TODO）|

---

## 附录 A：变更日志

### v0.1 (2026-10-06, Christine D+18)

D+18 周一 fork guide examples:
- 9 章 concrete walkthrough（clone → configure → delete → edit → icon → license → verify → feature → push）
- 5 个 expected output 块（configure.sh, make verify, FeatureBootstrap auto-discovery, etc.）
- 7 个 common pitfalls（per README §"踩过的坑" + Christine's D+1-D+17 experience）

**Audience**：新 fork 用户（developer / student / hobbyist）

**Coexists with**：`FORK_CHECKLIST.md`（root-level, step-by-step）+ `AGENTS.md`（root-level, agent contract）+ `docs/` design specs

---

**fork guide examples 完。** 配合 `FORK_CHECKLIST.md` 使用。