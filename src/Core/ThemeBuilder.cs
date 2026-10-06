using Godot;

namespace GodotTemplate.Core;

/// <summary>
/// Visual theme category. 3 values map to the 3 game worlds
/// (Forest levels 1-20, Desert levels 21-40, Ocean levels 41-60).
///
/// <para>
/// Lives in <c>GodotTemplate.Core</c> because <see cref="ThemeBuilder"/>
/// (also Core) needs it. Game-feature layer (<c>Match3</c>) translates
/// <c>Level</c> → <see cref="ThemeKind"/> via <see cref="ThemeKindUtils.FromLevel"/>.
/// </para>
/// </summary>
public enum ThemeKind
{
    Forest,
    Desert,
    Ocean,
}

/// <summary>
/// Helpers that map game concepts to theme categories. Kept separate
/// from the <see cref="ThemeKind"/> enum so Core stays free of game-
/// domain assumptions (level counts are a Match3 product decision, not
/// a theming primitive).
/// </summary>
public static class ThemeKindUtils
{
    /// <summary>
    /// Maps a 0-indexed level number to a <see cref="ThemeKind"/> using
    /// the v1.2 product decision: Forest 1-20, Desert 21-40, Ocean 41-60.
    /// </summary>
    public static ThemeKind FromLevel(int level)
    {
        if (level < 20) return ThemeKind.Forest;
        if (level < 40) return ThemeKind.Desert;
        return ThemeKind.Ocean;
    }
}

/// <summary>
/// Pair of TTF fonts for a theme: <see cref="Primary"/> is the display
/// font (titles, headlines, buttons); <see cref="Secondary"/> is the
/// body font (HUD labels, long-form text). Per Christine
/// <c>docs/design/font-per-theme-spec.md</c> §3 v1.2 weight remap.
///
/// <para>
/// v1.2 ships Primary only — <see cref="ThemeBuilder.BuildTheme"/>
/// assigns it to <c>theme.DefaultFont</c>. <see cref="Secondary"/> is
/// reserved for v1.3+ per-theme-type overrides (Label → Secondary,
/// Button → Primary, etc. via <c>theme.SetFont("Label", secondary)</c>).
/// </para>
/// </summary>
public sealed record FontPair(FontFile? Primary, FontFile? Secondary)
{
    /// <summary>
    /// v1.1 baseline pair: Fredoka-Bold + Nunito-Regular. Used as
    /// fallback when per-theme TTF fails to load (returns null for
    /// individual font files in the pair).
    /// </summary>
    public static FontPair Default => new(
        Primary: ThemeBuilder.LoadFont(ThemeBuilder.FredokaBoldPath),
        Secondary: ThemeBuilder.LoadFont(ThemeBuilder.NunitoRegularPath));
}

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
/// <para>
/// v1.2 polish #2 (per <c>docs/design/font-per-theme-spec.md</c>):
/// <see cref="BuildTheme(ThemeKind)"/> selects per-theme font pair
/// (Forest = Fredoka-Bold + Nunito-Regular; Desert = Fredoka-SemiBold +
/// Nunito-Medium; Ocean = Fredoka-Bold + Nunito-Bold). <c>BuildCosmicTheme()</c>
/// stays as a v1.1 fallback (used by SplashScreen which has no theme context).
/// </para>
///
/// <para>Apply by setting root Control.Theme:</para>
/// <code>
/// public TitleScreen() {
///     SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
///     Theme = ThemeBuilder.BuildTheme(ThemeKind.Forest);
///     // ... add children (inherit theme)
/// }
/// </code>
/// </summary>
public static class ThemeBuilder
{
    // ─── v1.1 font paths (top-level) ──────────────────────────────────────
    internal const string FredokaBoldPath     = "res://assets/fonts/Fredoka-Bold.ttf";
    internal const string FredokaSemiBoldPath = "res://assets/fonts/Fredoka-SemiBold.ttf";
    internal const string NunitoRegularPath   = "res://assets/fonts/Nunito-Regular.ttf";
    internal const string NunitoMediumPath    = "res://assets/fonts/Nunito-Medium.ttf";
    internal const string NunitoBoldPath      = "res://assets/fonts/Nunito-Bold.ttf";

    // ─── v1.2 per-theme font paths (forest/desert/ocean subdirs) ──────────
    private const string ForestFredokaBoldPath   = "res://assets/fonts/forest/Fredoka-Bold.ttf";
    private const string ForestNunitoRegularPath = "res://assets/fonts/forest/Nunito-Regular.ttf";
    private const string DesertFredokaSemiBoldPath = "res://assets/fonts/desert/Fredoka-SemiBold.ttf";
    private const string DesertNunitoMediumPath    = "res://assets/fonts/desert/Nunito-Medium.ttf";
    private const string OceanFredokaBoldPath      = "res://assets/fonts/ocean/Fredoka-Bold.ttf";
    private const string OceanNunitoBoldPath       = "res://assets/fonts/ocean/Nunito-Bold.ttf";

    /// <summary>
    /// v1.1 cosmic theme. Single <c>DefaultFont = Fredoka-Bold</c> for
    /// all Control nodes. Used by <see cref="SplashScreen"/> which has no
    /// game-theme context (splash runs before Match3 features load).
    /// </summary>
    public static Theme BuildCosmicTheme()
    {
        var theme = new Theme();

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
    /// v1.2 per-theme Theme. Selects a <see cref="FontPair"/> based on
    /// <paramref name="kind"/> and assigns the Primary font as the
    /// theme's <c>DefaultFont</c>. Secondary is reserved for v1.3+ per-
    /// type overrides.
    ///
    /// <para>
    /// Per <c>docs/design/font-per-theme-spec.md §3</c> weight remap:
    /// <list type="bullet">
    ///   <item><b>Forest</b>: Fredoka-Bold + Nunito-Regular (gameplay-readable)</item>
    ///   <item><b>Desert</b>: Fredoka-SemiBold + Nunito-Medium (slightly heavier)</item>
    ///   <item><b>Ocean</b>: Fredoka-Bold + Nunito-Bold (heaviest, "deep water" weight)</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// If the per-theme font fails to load, falls back to
    /// <see cref="FontPair.Default"/> (Fredoka-Bold + Nunito-Regular)
    /// so the Theme is never null — caller code can rely on a non-null
    /// <c>DefaultFont</c>.
    /// </para>
    /// </summary>
    public static Theme BuildTheme(ThemeKind kind)
    {
        var theme = new Theme();
        var fonts = GetThemeFonts(kind);

        var primary = fonts.Primary ?? FontPair.Default.Primary;
        if (primary != null)
        {
            theme.DefaultFont = primary;
        }
        else
        {
            GD.PushWarning($"[ThemeBuilder] {kind} primary font failed to load AND fallback Fredoka-Bold failed — controls will use Godot's default font");
        }

        // Pre-register Secondary (load but don't assign yet — v1.3+ will
        // call theme.SetFont("Label", fonts.Secondary) for per-type
        // override). Loading now avoids first-frame stutter.
        _ = fonts.Secondary;

        return theme;
    }

    /// <summary>
    /// Resolves the per-theme <see cref="FontPair"/>. Returns a record
    /// with possibly-null font fields if individual files fail to load.
    /// Per-theme TTF files live in <c>assets/fonts/{forest,desert,ocean}/</c>
    /// (Christine art branch <c>art/v1.2-per-theme-ttf</c>).
    /// </summary>
    private static FontPair GetThemeFonts(ThemeKind kind)
    {
        return kind switch
        {
            ThemeKind.Forest => new FontPair(
                Primary: LoadFont(ForestFredokaBoldPath),
                Secondary: LoadFont(ForestNunitoRegularPath)),
            ThemeKind.Desert => new FontPair(
                Primary: LoadFont(DesertFredokaSemiBoldPath),
                Secondary: LoadFont(DesertNunitoMediumPath)),
            ThemeKind.Ocean => new FontPair(
                Primary: LoadFont(OceanFredokaBoldPath),
                Secondary: LoadFont(OceanNunitoBoldPath)),
            _ => FontPair.Default,
        };
    }

    /// <summary>
    /// Loads a FontFile resource from a <c>res://</c> path. Returns
    /// <c>null</c> and logs a warning if the file is missing or the
    /// loader returns null. Uses static <c>ResourceLoader.Load</c>
    /// (not <c>FontFile.LoadDynamicFont</c>) because the font is a
    /// pre-shipped static asset — no need for dynamic sizing.
    ///
    /// <para>
    /// Marked <c>internal</c> (not <c>private</c>) so <see cref="FontPair.Default"/>
    /// can call it for the v1.1 fallback pair.
    /// </para>
    /// </summary>
    internal static FontFile? LoadFont(string fontPath)
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
