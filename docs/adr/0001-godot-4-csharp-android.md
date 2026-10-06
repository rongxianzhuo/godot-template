# ADR-0001: Godot 4.7.2 + C# (.NET 9) + Android 选型

> **Status**: ✅ Accepted
> **Date**: 2026-09-29 (D+0)
> **Deciders**: Mark (Leader) + Jacob (godot-template maintainer)

## Context

需要一个 cross-platform mobile game engine + language + target stack：
- **Engine candidates**: Unity, Unreal, Godot, custom C++
- **Language candidates**: C#, C++, GDScript, Rust
- **Target candidates**: Android, iOS, Web, Desktop

约束：
- Solo founder（Mark）+ 4 AI agents（Jacob + Francisco + Austin + Christine）
- 14 天 ship deadline（v1.0 release）
- Android-first（Casual mobile market）
- 必须 MIT/Apache license（避免 royalty）
- 必须支持 C#（avoid GDScript 缺乏 IDE/AI 工具链）

## Decision

选 **Godot 4.7.2-stable (Mono)** + **C# (.NET 9)** + **Android arm64-v8a** 作为 foundational stack。

## Consequences

**Positive**:
- ✅ Godot 完全 MIT + 完全 free（vs Unity 收费 tier + Unreal 5% royalty）
- ✅ C# (.NET 9) 强类型 + IDE 支持（Rider / VSCode / VS）
- ✅ Godot 4.7 跨平台（Android / iOS / Web / Desktop）允许未来扩展
- ✅ Godot 反射架构（`[GodotFeature]`）支持 hot reload
- ✅ .NET 9 TFM (`net9.0`) 是 2026 最新 LTS-style
- ✅ Mono runtime < 30 MB（vs Unity IL2CPP 100 MB+）

**Negative**:
- ❌ Godot 4.7 文档零散（很多坑需要踩过才知道，见 README §"踩过的坑"）
- ❌ C# + Android 路径在 Godot 4.7 仍存在 Gradle 兼容问题（需 use_gradle_build=false workaround）
- ❌ Mono runtime 在 Android 上比 IL2CPP 慢约 20%（但 match-3 game 不敏感）

**Neutral**:
- 🟢 TFM 必须 `net9.0`（net8.0/net10.0 报错 per Godot 4.7 校验）
- 🟢 Android export path 必须用 legacy non-Gradle（per `scripts/install-android-templates.sh`）

## Alternatives Considered

### Alternative A: Unity

- ❌ Rejected：Unity 6+ 个人版 200K USD/year revenue limit（match-3 casual game 易超）
- ❌ Rejected：Unity Asset Store 依赖 + Pro features 锁
- ❌ Rejected：14 天 ship 周期 + Unity 学习曲线不 match

### Alternative B: Unreal Engine

- ❌ Rejected：5% royalty（match-3 100K downloads × $1 IAP = $5K royalty 不划算）
- ❌ Rejected：C++ 学习曲线 + AI 工具链（C++ AI assistant 弱于 C#）
- ❌ Rejected：mobile export APK > 80 MB（含 Engine runtime）

### Alternative C: GDScript (Godot native)

- ❌ Rejected：缺乏 IDE 工具链（vscode 插件弱）
- ❌ Rejected：缺乏 AI 友好（dynamic typing AI 难生成正确代码）
- ❌ Rejected：缺乏 type safety（match-3 algorithm 易出错）

### Alternative D: Custom C++ + SDL2

- ❌ Rejected：14 天 ship 不可能（vs C# + Godot 14 天够）
- ❌ Rejected：Android NDK 集成复杂度高
- ❌ Rejected：AI agent 写 C++ 容易出错（memory leak / segfault）

## Cross-references

- README.md §"踩过的坑"（10 个 Godot 4.7 实战坑）
- AGENTS.md §"Toolchain"（toolchain install 步骤）
- scripts/install-toolchain.sh（自动化安装）
- ADR-0002: fork 双身份
