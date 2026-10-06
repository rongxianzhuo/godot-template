using Godot;

namespace GodotTemplate.Core;

/// <summary>
/// LocaleManager — autoload singleton that loads the 3 .po translation
/// files (en / es / pt-BR) from <c>locale/</c> and registers them with
/// Godot's <see cref="TranslationServer"/>. Provides
/// <see cref="SetLocale(string)"/> for runtime locale switching (e.g.,
/// from a future Settings screen).
///
/// <para>Per <c>docs/design/i18n-strategy.md</c> §6 Phase A:</para>
/// <list type="bullet">
///   <item>v1.1 ships 3 locales: en (default), es, pt-BR</item>
///   <item>v2.0 adds 5 more: zh-CN / zh-TW / ja / ko / fr</item>
///   <item>Tr() helper is the Node-instance method that uses
///         <see cref="TranslationServer"/> — scenes call
///         <c>Tr("Magic Match")</c> directly, no LocaleManager reference
///         needed in scenes</item>
/// </list>
///
/// <para>Autoload order (per <c>project.godot [autoload]</c>):</para>
/// <code>
/// GameFramework → UIManager → EventBus → ScreenManager → LocaleManager
/// </code>
/// <para>LocaleManager is independent — no hard deps on EventBus /
/// ScreenManager. Listed last so it initialises after framework autoloads
/// (in case future versions want to log via framework services).</para>
///
/// <para>Usage from scenes:</para>
/// <code>
/// titleLabel.Text = Tr("Magic Match");           // simple
/// _scoreLabel.Text = string.Format(              // with placeholder
///     Tr("Score: {0}"), _scores.Score);
/// </code>
///
/// <para>Runtime locale switching (e.g., from Settings):</para>
/// <code>
/// LocaleManager.Instance.SetLocale("es");
/// </code>
/// <para>
/// Note: Tr() does NOT auto-refresh already-rendered Labels. Callers
/// that want immediate UI refresh must re-read Tr() values and update
/// their Labels manually (callers do this in their OnShow / UpdateHud
/// methods, which run on the next frame after SetLocale returns).
/// </para>
/// </summary>
public partial class LocaleManager : Node
{
    public static LocaleManager Instance { get; private set; } = null!;

    // Locale codes (per Godot's locale string convention — "en",
    // "es", "pt-BR" with hyphen for region).
    public const string EnLocaleCode   = "en";
    public const string EsLocaleCode   = "es";
    public const string PtBrLocaleCode = "pt-BR";

    // Translation file paths (gettext PO format, Godot 4 native).
    private const string EnTranslationPath   = "res://locale/en.po";
    private const string EsTranslationPath   = "res://locale/es.po";
    private const string PtBrTranslationPath = "res://locale/pt-BR.po";

    private string _currentLocale = EnLocaleCode;

    /// <summary>Currently active locale code (read-only public view).</summary>
    public string CurrentLocale => _currentLocale;

    public override async void _Ready()
    {
        Instance = this;
        GD.Print("[LocaleManager] _Ready — loading locale/*.po translations");

        LoadAndRegister(EnTranslationPath);
        LoadAndRegister(EsTranslationPath);
        LoadAndRegister(PtBrTranslationPath);

        // v1.2 polish #4 (per ADR-0014): locale priority order:
        //   1. LocalePreferences saved value (user manual override)
        //   2. OS locale (TranslationServer default / OS.GetLocale())
        //   3. English (final fallback)
        //
        // The save is async so we don't block _Ready on disk I/O.
        // The async-void signature is the Godot convention for fire-and-
        // forget autoload init (per Match3Feature._Ready precedent).
        string chosen = EnLocaleCode;
        try
        {
            var saved = await LocalePreferences.LoadAsync();
            if (saved.Locale != null && saved.Locale != LocalePreferences.AutoLocaleSentinel)
            {
                chosen = saved.Locale;
                GD.Print($"[LocaleManager] using saved preference: {chosen}");
            }
            else
            {
                chosen = ResolveOsLocale();
                GD.Print($"[LocaleManager] no saved preference, using OS locale: {chosen}");
            }
        }
        catch (System.Exception e)
        {
            GD.PushWarning($"[LocaleManager] locale resolution failed: {e.Message}, falling back to {EnLocaleCode}");
            chosen = EnLocaleCode;
        }

        TranslationServer.SetLocale(chosen);
        _currentLocale = chosen;

        GD.Print($"[LocaleManager] loaded, active locale = {_currentLocale}");
    }

    /// <summary>
    /// Picks the best-matching locale from the OS-reported locale string
    /// (e.g. <c>"en_US"</c>, <c>"es_MX"</c>, <c>"pt_BR"</c>) against
    /// the 3 supported locales. Returns <see cref="EnLocaleCode"/> when
    /// no match.
    /// </summary>
    private static string ResolveOsLocale()
    {
        var osLocale = OS.GetLocale(); // e.g. "en_US", "es_MX", "pt_BR"
        if (string.IsNullOrEmpty(osLocale)) return EnLocaleCode;

        // Match by 2-letter primary subtag first ("es_MX" → "es").
        var primary = osLocale.Length >= 2 ? osLocale.Substring(0, 2) : osLocale;
        return primary switch
        {
            "es" => EsLocaleCode,
            "pt" => PtBrLocaleCode,
            _    => EnLocaleCode,
        };
    }

    /// <summary>
    /// Switches the active locale at runtime. Affects subsequent Tr()
    /// calls in scenes (callers must refresh their Labels manually — Tr()
    /// does not auto-refresh already-rendered text).
    /// </summary>
    /// <param name="localeCode">One of "en" / "es" / "pt-BR" (or any
    /// future locale with a registered Translation).</param>
    public void SetLocale(string localeCode)
    {
        if (localeCode == _currentLocale)
        {
            GD.Print($"[LocaleManager] SetLocale({localeCode}) — already active");
            return;
        }
        GD.Print($"[LocaleManager] SetLocale: {_currentLocale} -> {localeCode}");
        TranslationServer.SetLocale(localeCode);
        _currentLocale = localeCode;
    }

    /// <summary>
    /// Loads a Godot <see cref="Translation"/> resource from a gettext
    /// PO file and registers it with <see cref="TranslationServer"/>.
    /// The PO file's "Language: xx" header determines the locale code
    /// (Godot uses it to match against <see cref="TranslationServer.SetLocale"/>).
    /// </summary>
    private static void LoadAndRegister(string translationPath)
    {
        if (!ResourceLoader.Exists(translationPath))
        {
            GD.PushWarning($"[LocaleManager] translation file not found: {translationPath}");
            return;
        }

        var translation = ResourceLoader.Load<Translation>(translationPath);
        if (translation == null)
        {
            GD.PushWarning($"[LocaleManager] failed to load translation: {translationPath} (loader returned null)");
            return;
        }

        TranslationServer.AddTranslation(translation);
        GD.Print($"[LocaleManager] registered: {translationPath} (locale={translation.Locale})");
    }
}