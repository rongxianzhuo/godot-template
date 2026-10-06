using Godot;
using GodotTemplate.Core;

namespace GodotTemplate.Features.Settings;

/// <summary>
/// Language picker feature marker. Auto-discovered by
/// <see cref="Bootstrap"/> via the <see cref="GodotFeatureAttribute"/>
/// (per ADR-0004 reflection-based feature loading).
///
/// <para>
/// v1.2 polish #4 (per ADR-0014): this feature is primarily a
/// <i>discovery marker</i> — the actual locale-picking logic lives in
/// <see cref="LocaleManager"/> (autoload, Order = N/A — runs before
/// features) and <see cref="LocalePreferences"/> (persistence). The
/// Settings UI that lets the user actually pick a language lives in
/// <c>SettingsScreen</c> (shown modally from TitleScreen/EndScreen).
/// </para>
///
/// <para>
/// Why have a feature at all if it's mostly empty? Two reasons:
/// <list type="number">
///   <item>
///     <b>Future extension point</b>: v1.3+ will add
///     <see cref="EventBus"/> subscription here (e.g. listen for a
///     <c>LocaleChangedEvent</c> and refresh in-flight screens). Marking
///     Order=850 reserves the load slot now so future PRs don't conflict.
///   </item>
///   <item>
///     <b>Fork-friendly discovery</b>: forks that want to disable the
///     Settings UI can simply delete this file (and <see cref="SettingsScreen"/>)
///     — <see cref="Bootstrap"/> skips them automatically. No code changes
///     to <see cref="LocaleManager"/> needed.
///   </item>
/// </list>
/// </para>
///
/// <para>
/// Order=850 runs after <c>CounterFeature</c> (100) and <c>Match3Feature</c>
/// (200) but before <c>Match3SmokeTest</c> (950). Settings UI itself is
/// not auto-shown — it's a modal <c>Screen</c> triggered from Title/End.
/// </para>
/// </summary>
[GodotFeature(Order = 850, Category = "ui")]
public partial class LanguagePickerFeature : Node
{
    public override void _Ready()
    {
        GD.Print("[LanguagePickerFeature] _Ready — language picker UI is available (open SettingsScreen to use)");
    }
}
