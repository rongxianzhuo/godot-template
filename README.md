# Godot C# Android Template

[中文](#中文) · [English](#english)

---

## 中文

一个开箱即用的 **Godot 4 + C# + Android** 工程模板。

启动后屏幕中央显示一个数字，每帧 +1。代码只有十几行，但工程本身已经配好
了所有"看起来简单、做起来一堆坑"的东西 —— Godot Mono 编辑器、.NET 9 SDK、
Android SDK / NDK、Gradle 路径选择、签名、导出预设 —— 你 clone 下来改两行
就能当新项目用。

### ✨ 已经配好的东西

- **Godot 4.7.2-stable (Mono)**
- **.NET 9 SDK**（`net9.0` TFM）
- **Android 导出预设**：`arm64-v8a` 单架构、min SDK 24、target SDK 36、debug keystore 签名
- **Android 构建路径**：用老的非-Gradle 直接打包，绕开 Gradle 内存问题（见下方"踩过的坑"）
- **`.gitignore`**：缓存、构建产物、keystore 全部不进仓库
- **MIT License**

### 📁 目录结构

```
.
├── Main.cs                  # 业务逻辑（计数器）
├── Main.tscn                # 主场景
├── Main.cs.uid              # Godot UID 映射
├── project.godot            # Godot 工程配置
├── icon.svg                 # 应用图标
├── icon.svg.import          # 图标导入元数据
├── export_presets.cfg       # Android 导出预设
├── EmptyGodot.csproj        # C# 项目文件（Godot.NET.Sdk / net9.0）
├── EmptyGodot.sln           # Visual Studio / Rider 解决方案
├── .gitignore
├── LICENSE
├── android/                 # Android 导出模板（被 .gitignore，不入库）
└── .godot/                  # Godot 编辑器缓存（被 .gitignore，不入库）
```

### 🚀 本地开发（桌面）

需要装 Godot Mono 编辑器（[官网下载](https://godotengine.org/download/)，
Linux 上选 `...mono_linux.x86_64.zip`）。

```bash
# 打开工程
godot --path . --editor

# 或直接跑（headless 也行）
godot --path . --quit-after 60
```

修改 `Main.cs` 里的逻辑即可。Godot 4 的 C# 编辑器支持热重载。

### 📱 打包 Android APK

#### 前置依赖（一次性）

```bash
# 1. JDK 17
apt install openjdk-17-jdk-headless

# 2. Android SDK（cmdline-tools + platform-tools + platforms;android-36 + build-tools;36.1.0）
#    见 https://developer.android.com/studio

# 3. Android NDK r26d
#    下载 https://dl.google.com/android/repository/android-ndk-r26d-linux.zip
#    解压到任意目录

# 4. Godot Mono 编辑器（同上）
# 5. .NET 9 SDK
#    https://dotnet.microsoft.com/download/dotnet/9.0
```

#### 配置 Editor Settings

第一次打包前，把 Android SDK / NDK / JDK 路径告诉 Godot：

```
菜单：Editor → Editor Settings → Export → Android
  Android SDK Path:    /path/to/Android/sdk
  Android NDK Path:    /path/to/Android/sdk/ndk/r26d
  JDK Path:            /usr/lib/jvm/java-17-openjdk-amd64
  Debug Keystore:      /path/to/your/debug.keystore
  Debug Keystore User: androiddebugkey
  Debug Keystore Pass: android
```

或者直接编辑 `~/.config/godot/editor_settings-4.7.tres`（CI / Docker 场景）。

#### 执行导出

```bash
godot --headless --path . --export-release "Android" builds/app.apk
```

产物在 `builds/app.apk`（约 98 MB，包含 Mono 运行时）。

### 🪤 踩过的坑

1. **Godot 4.7+ 的 Android 模板要解压到 `android/build/`**，不是 `android/`
2. **`.build_version` 文件内容必须是 `4.7.2.stable`**（不带 hash 后缀）
3. **Keystore 配置项在 4.7 改名了**：用 `keystore/release` 系列，不是 `package/certificate_*`
4. **C# 工程导出模板必须在 `export_templates/<version>.mono/`**（多一个 `.mono` 后缀）
5. **Godot Mono Linux x86_64 包的真实 URL 是 `mono_linux_x86_64.zip`（下划线）**，GitHub release 页面展示的版本 `mono_linux.x86_64.zip`（点）是错的
6. **C# + Android 非 Gradle 路径要求 TFM = `net9.0`**，用 `net8.0` / `net10.0` 都会报错
7. **512 MB 内存限制下 Gradle daemon 必崩**，所以这个模板用 `use_gradle_build=false`（老的 APK 直接打包路径）。如果有 ≥ 2 GB 内存，改回 `use_gradle_build=true` 可以让 AAB / Vulkan / 高级选项可用

### 📜 License

MIT —— 见 [LICENSE](LICENSE)。

---

## English

A ready-to-fork **Godot 4 + C# + Android** project template.

Displays a number in the screen center, incrementing by 1 every frame. The code
is ~15 lines, but the project itself has all the fiddly bits wired up — Godot
Mono editor, .NET 9 SDK, Android SDK / NDK, Gradle path selection, signing,
export presets — so you can clone, edit two lines, and ship.

### ✨ What's pre-configured

- **Godot 4.7.2-stable (Mono)**
- **.NET 9 SDK** (`net9.0` TFM)
- **Android export preset**: `arm64-v8a` only, min SDK 24, target SDK 36, debug keystore signed
- **Android build path**: legacy non-Gradle direct packaging to dodge Gradle memory issues (see "Pitfalls")
- **`.gitignore`**: caches, build outputs, and keystores stay out of the repo
- **MIT License**

### 📁 Project structure

```
.
├── Main.cs                  # Business logic (frame counter)
├── Main.tscn                # Main scene
├── Main.cs.uid              # Godot UID mapping
├── project.godot            # Godot project config
├── icon.svg                 # App icon
├── icon.svg.import          # Icon import metadata
├── export_presets.cfg       # Android export preset
├── EmptyGodot.csproj        # C# project (Godot.NET.Sdk / net9.0)
├── EmptyGodot.sln           # Visual Studio / Rider solution
├── .gitignore
├── LICENSE
├── android/                 # Android export template (gitignored)
└── .godot/                  # Godot editor cache (gitignored)
```

### 🚀 Desktop development

Install the Godot Mono editor from [the official site](https://godotengine.org/download/) —
on Linux pick `...mono_linux_x86_64.zip`.

```bash
# Open the project in the editor
godot --path . --editor

# Or run headless
godot --path . --quit-after 60
```

Edit `Main.cs`. Godot 4's C# editor supports hot reload.

### 📱 Building the Android APK

#### One-time prerequisites

```bash
# 1. JDK 17
apt install openjdk-17-jdk-headless

# 2. Android SDK (cmdline-tools + platform-tools + platforms;android-36 + build-tools;36.1.0)
#    See https://developer.android.com/studio

# 3. Android NDK r26d
#    https://dl.google.com/android/repository/android-ndk-r26d-linux.zip

# 4. Godot Mono editor (see above)
# 5. .NET 9 SDK
#    https://dotnet.microsoft.com/download/dotnet/9.0
```

#### Configure Editor Settings

Before your first export, point Godot at the Android SDK / NDK / JDK:

```
Menu: Editor → Editor Settings → Export → Android
  Android SDK Path:    /path/to/Android/sdk
  Android NDK Path:    /path/to/Android/sdk/ndk/r26d
  JDK Path:            /usr/lib/jvm/java-17-openjdk-amd64
  Debug Keystore:      /path/to/your/debug.keystore
  Debug Keystore User: androiddebugkey
  Debug Keystore Pass: android
```

Or edit `~/.config/godot/editor_settings-4.7.tres` directly (for CI / Docker).

#### Export

```bash
godot --headless --path . --export-release "Android" builds/app.apk
```

Output: `builds/app.apk` (~98 MB, includes Mono runtime).

### 🪤 Pitfalls (we hit these so you don't have to)

1. **Godot 4.7+ Android template must be extracted to `android/build/`**, not `android/`
2. **`.build_version` file contents must be `4.7.2.stable`** — no hash suffix
3. **Keystore field names changed in 4.7**: use `keystore/release*`, not `package/certificate_*`
4. **C# export templates must live in `export_templates/<version>.mono/`** — note the `.mono` suffix
5. **The real URL for Godot Mono Linux x86_64 is `mono_linux_x86_64.zip` (underscores)**, even though the GitHub release page displays `mono_linux.x86_64.zip` (dots)
6. **C# + Android non-Gradle path requires TFM = `net9.0`**; `net8.0` / `net10.0` will fail validation
7. **Gradle daemon will OOM-crash under a 512 MB container memory limit**, so this template uses `use_gradle_build=false` (legacy direct APK packaging). If you have ≥ 2 GB RAM, set it back to `true` to unlock AAB / Vulkan / advanced options

### 📜 License

MIT — see [LICENSE](LICENSE).
