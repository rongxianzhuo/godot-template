using Godot;
using System.Text.Json;
using System.Threading.Tasks;
using FileAccess = Godot.FileAccess;  // disambiguate from System.IO.FileAccess

namespace GodotTemplate.Core;

/// <summary>
/// Persists the user's manually-selected locale to Godot's
/// <c>user://</c> directory so the choice survives app restarts.
///
/// <para>
/// Storage location: <c>user://locale.cfg</c> (per ADR-0014 §3 +
/// v1.2 polish #4 spec). On Android this maps to the app's private
/// storage (e.g. <c>/data/data/org.magicstudio.match3/files/locale.cfg</c>),
/// which is automatically cleared by the OS on uninstall.
///
/// </para>
/// <para>
/// Format: a single JSON object with one field:
/// <code>
/// { "locale": "es" }
/// </code>
/// Valid values are <c>"en"</c>, <c>"es"</c>, <c>"pt-BR"</c>.
/// <c>"auto"</c> means "follow OS locale" — used when the user clears
/// their override (not yet wired in v1.2; reserved for v1.3+ per ADR).
/// </para>
///
/// <para>
/// v1.2 uses async I/O (<see cref="LoadAsync"/>, <see cref="SaveAsync"/>)
/// so the LocaleManager autoload doesn't block the engine's main thread on
/// disk access during <c>_Ready</c>. Both methods swallow exceptions and
/// log warnings — a corrupt/missing <c>locale.cfg</c> is non-fatal (the
/// caller falls back to OS locale or English).
/// </para>
/// </summary>
public static class LocalePreferences
{
    /// <summary>
    /// Sentinel value meaning "no manual override; follow OS locale".
    /// Reserved for v1.3+ Settings UI "Reset to OS default" button
    /// (per ADR-0014 Future Work).
    /// </summary>
    public const string AutoLocaleSentinel = "auto";

    private const string FilePath = "user://locale.cfg";

    /// <summary>
    /// Result record for <see cref="LoadAsync"/>. <see cref="Locale"/>
    /// is null when the file doesn't exist or is unreadable — caller
    /// should fall back to OS locale detection in that case.
    /// </summary>
    public sealed record LoadResult(string? Locale);

    /// <summary>
    /// Reads <c>user://locale.cfg</c> from disk asynchronously.
    /// Returns <c>LoadResult(null)</c> on file-not-found or parse error.
    /// Never throws — all errors are logged as warnings.
    /// </summary>
    public static async Task<LoadResult> LoadAsync()
    {
        if (!FileAccess.FileExists(FilePath))
        {
            GD.Print($"[LocalePreferences] no preference file at {FilePath} — first run, will follow OS locale");
            return new LoadResult(null);
        }

        try
        {
            // Use Godot's FileAccess for user:// resolution (Godot's
            // virtual filesystem). Read in a thread to avoid blocking
            // _Ready on slow disks (Android SD cards can be laggy).
            var content = await Task.Run(() =>
            {
                using var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    var err = FileAccess.GetOpenError();
                    GD.PushWarning($"[LocalePreferences] failed to open {FilePath}: error={err}");
                    return null;
                }
                return file.GetAsText();
            });

            if (string.IsNullOrWhiteSpace(content))
            {
                GD.PushWarning($"[LocalePreferences] {FilePath} is empty");
                return new LoadResult(null);
            }

            using var doc = JsonDocument.Parse(content);
            if (!doc.RootElement.TryGetProperty("locale", out var localeEl))
            {
                GD.PushWarning($"[LocalePreferences] {FilePath} missing 'locale' field");
                return new LoadResult(null);
            }

            var locale = localeEl.GetString();
            if (string.IsNullOrEmpty(locale))
            {
                return new LoadResult(null);
            }

            GD.Print($"[LocalePreferences] loaded saved locale: {locale}");
            return new LoadResult(locale);
        }
        catch (JsonException e)
        {
            GD.PushWarning($"[LocalePreferences] {FilePath} is malformed JSON: {e.Message}");
            return new LoadResult(null);
        }
        catch (System.Exception e)
        {
            GD.PushWarning($"[LocalePreferences] unexpected error reading {FilePath}: {e.Message}");
            return new LoadResult(null);
        }
    }

    /// <summary>
    /// Writes the given locale to <c>user://locale.cfg</c> asynchronously.
    /// Creates the file if it doesn't exist; overwrites if it does.
    /// Never throws — all errors are logged as warnings (save failure
    /// doesn't break language switching, just means it won't persist).
    /// </summary>
    /// <param name="localeCode">One of "en" / "es" / "pt-BR" /
    /// <see cref="AutoLocaleSentinel"/> (= clear override).</param>
    public static async Task SaveAsync(string localeCode)
    {
        var json = JsonSerializer.Serialize(new { locale = localeCode });
        try
        {
            await Task.Run(() =>
            {
                using var file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    var err = FileAccess.GetOpenError();
                    GD.PushWarning($"[LocalePreferences] failed to open {FilePath} for write: error={err}");
                    return;
                }
                file.StoreString(json);
            });
            GD.Print($"[LocalePreferences] saved locale: {localeCode}");
        }
        catch (System.Exception e)
        {
            GD.PushWarning($"[LocalePreferences] unexpected error writing {FilePath}: {e.Message}");
        }
    }

    /// <summary>
    /// Deletes the preferences file (used by future "Reset to OS default"
    /// feature — v1.3+ per ADR-0014 Future Work).
    /// </summary>
    public static void Clear()
    {
        if (FileAccess.FileExists(FilePath))
        {
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(FilePath));
            GD.Print($"[LocalePreferences] cleared saved locale (next launch will follow OS locale)");
        }
    }
}
