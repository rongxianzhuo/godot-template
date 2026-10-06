using Godot;

namespace GodotTemplate.Core;

/// <summary>
/// Builds the v1.1 cosmic-themed Godot Theme using 5 TTF files in
/// <c>assets/fonts/</c>. See <c>docs/design/font-integration-spec.md</c>
/// + <c>docs/ART_SPEC.md §4.2</c>.
///
/// <para>
/// v1.1 scope (per Christine font-integration-spec.md §3.1):
/// <list type="bullet">
///   <item>Set theme.DefaultFont = Fredoka-Bold (display title)</item>
///   <item>All Control nodes (Label, Button, etc.) inherit Fredoka-Bold
///         unless they have an <c>AddThemeFontOverride()</c> per-element</item>
///   <item>Other 4 TTFs loaded (for ResourceLoader registration) but
///         not assigned to theme types — refinement in v1.2 / v2.0 CJK</item>
/// </list>
/// </para>
///
/// <para>Apply by setting root Control.Theme:</para>
/// <code>
/// public TitleScreen() {
///     SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
///     Theme = ThemeBuilder.BuildCosmicTheme();
///     // ... add children (inherit theme)
/// }
/// </code>
///
/// <para>
/// Why single DefaultFont (not per-element type overrides)?
/// Per Christine spec §3.1 pseudocode, the v1.1 minimum is
/// DefaultFont = Fredoka-Bold. v1.2 / v2.0 refinement would set
/// per-theme-type fonts ("Button" → Nunito-Bold, "Label" → Nunito-
/// Regular, "HUDLabel" → Nunito-Medium) via <c>SetFont(font, type)</c>
/// + scenes opting in with <c>ThemeTypeVariation = "X"</c>.
/// </para>
///
/// <para>
/// Resource cost: 5 TTF subsets × ~33-39 KB = ~182 KB on disk
/// (per Christine spec §9). Loaded at runtime per-scene
/// instantiation (~5 ms total cold load).
/// </para>
/// </summary>
public static class ThemeBuilder
{
    // Font paths (per Christine spec §1.1 + §3.1).
    private const string FredokaBoldPath     = "res://assets/fonts/Fredoka-Bold.ttf";
    private const string FredokaSemiBoldPath = "res://assets/fonts/Fredoka-SemiBold.ttf";
    private const string NunitoRegularPath   = "res://assets/fonts/Nunito-Regular.ttf";
    private const string NunitoMediumPath    = "res://assets/fonts/Nunito-Medium.ttf";
    private const string NunitoBoldPath      = "res://assets/fonts/Nunito-Bold.ttf";

    /// <summary>
    /// Loads the 5 TTF files via ResourceLoader.Load&lt;FontFile&gt;,
    /// assigns Fredoka-Bold as the theme's DefaultFont (per Christine
    /// spec §3.1 pseudocode), and returns the Theme. All 5 fonts are
    /// loaded so Godot imports them on first reference.
    ///
    /// Errors on individual font load are non-fatal warnings. The Theme
    /// is still usable with whatever loaded successfully — the missing
    /// fonts fall back to Godot's built-in default.
    /// </summary>
    public static Theme BuildCosmicTheme()
    {
        var theme = new Theme();

        // Default: Fredoka-Bold (covers TitleScreen title, EndScreen
        // headline, splash title — all "display" use cases per ART_SPEC
        // §4.2 text_xl token). All Controls inherit unless per-element
        // override is set via AddThemeFontOverride().
        var fredokaBold = LoadFont(FredokaBoldPath);
        if (fredokaBold != null)
        {
            theme.DefaultFont = fredokaBold;
        }
        else
        {
            GD.PushWarning("[ThemeBuilder] Fredoka-Bold failed to load — controls will use Godot's default font");
        }

        // Load the other 4 so ResourceLoader registers them as FontFile
        // resources (avoids first-frame stutter when v1.2 / v2.0 starts
        // referencing them per-theme-type).
        _ = LoadFont(FredokaSemiBoldPath);
        _ = LoadFont(NunitoRegularPath);
        _ = LoadFont(NunitoMediumPath);
        _ = LoadFont(NunitoBoldPath);

        return theme;
    }

    /// <summary>
    /// Loads a FontFile resource from a <c>res://</c> path. Returns
    /// <c>null</c> and logs a warning if the file is missing or the
    /// loader returns null. Uses static <c>ResourceLoader.Load</c>
    /// (not <c>FontFile.LoadDynamicFont</c>) because the font is a
    /// pre-shipped static asset — no need for dynamic sizing.
    /// </summary>
    private static FontFile? LoadFont(string fontPath)
    {
        if (!ResourceLoader.Exists(fontPath))
        {
            GD.PushWarning($"[ThemeBuilder] font file not found: {fontPath}");
            return null;
        }
        var font = ResourceLoader.Load<FontFile>(fontPath);
        if (font == null)
        {
            GD.PushWarning($"[ThemeBuilder] failed to load font: {fontPath} (loader returned null)");
        }
        return font;
    }
}