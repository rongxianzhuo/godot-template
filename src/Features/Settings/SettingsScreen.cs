using Godot;
using GameFramework;
using GodotTemplate.Core;
using GodotTemplate.Features.Match3.Screens;

namespace GodotTemplate.Features.Settings;

/// <summary>
/// Settings screen — modal language picker shown from
/// <see cref="TitleScreen"/> and <see cref="EndScreen"/>. v1.2 polish #4
/// (per ADR-0014).
///
/// <para>
/// Inherits <see cref="Screen{SettingsArgs, SettingsResult}"/> with the
/// parameterless typed args + a typed result that reports whether the
/// locale was changed (so the caller can refresh visible labels) and
/// what locale is now applied.
///
/// </para>
/// <para>Scene tree (built in constructor, no .tscn):</para>
/// <code>
///   SettingsScreen (Control, FullRect anchors)
///   ├── Dim (ColorRect, semi-transparent black overlay 0.6 alpha)
///   └── Modal (VBoxContainer, centered)
///       ├── Title label ("Settings", 48pt)
///       ├── Spacer (20 px)
///       ├── LanguageSection (VBoxContainer)
///       │   ├── LanguageLabel ("Language", 28pt)
///       │   ├── EnglishButton (CheckBox)
///       │   ├── SpanishButton  (CheckBox)
///       │   └── PortugueseButton (CheckBox)
///       ├── Spacer (20 px)
///       ├── ApplyButton ("Apply", primary)
///       ├── Spacer (10 px)
///       └── CancelButton ("Cancel", secondary)
/// </code>
///
/// <para>
/// Flow:
/// <list type="number">
///   <item><see cref="OnShow"/>: snapshot current locale into
///         <see cref="_pendingLocale"/>; populate 3 checkboxes with
///         current selection; show <see cref="_applyButton"/> enabled
///         iff selection differs from current.</item>
///   <item>User toggles checkboxes → <see cref="_pendingLocale"/> updates,
///         apply button enables/disables.</item>
///   <item>Apply → <see cref="LocaleManager.SetLocale"/> + fire-and-forget
///         <see cref="LocalePreferences.SaveAsync"/> → return result.</item>
///   <item>Cancel → return result with <c>LocaleChanged=false</c>.</item>
/// </list>
/// </para>
/// </summary>
public sealed partial class SettingsScreen : Screen<SettingsArgs, SettingsResult>
{
    private const string EnLocale   = "en";
    private const string EsLocale   = "es";
    private const string PtBrLocale = "pt-BR";

    // ─── view references (built in constructor) ───────────────────────
    private CheckBox _englishBox   = null!;
    private CheckBox _spanishBox   = null!;
    private CheckBox _portugueseBox = null!;
    private Button _applyButton    = null!;
    private Button _cancelButton   = null!;

    // ─── state ─────────────────────────────────────────────────────────
    private string _pendingLocale = EnLocale;

    public SettingsScreen()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        // Modal theme = Forest (matches title screen — settings is a
        // neutral utility, not game-specific).
        Theme = ThemeBuilder.BuildTheme(ThemeKind.Forest);

        // Dim overlay so the underlying screen is visible-but-faded
        // (gives the user context: "I'm still in Title/End, just config").
        var dim = new ColorRect
        {
            Name = "Dim",
            Color = new Color(0, 0, 0, 0.6f),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(dim);

        // Modal container (centered)
        var modal = new VBoxContainer
        {
            Name = "Modal",
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        modal.SetAnchorsPreset(Control.LayoutPreset.Center);
        modal.CustomMinimumSize = new Vector2(480, 0);
        AddChild(modal);

        // Title
        var titleLabel = new Label
        {
            Name = "TitleLabel",
            Text = Tr("settings.title"),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 48);
        titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.3f));
        modal.AddChild(titleLabel);

        modal.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

        // "Language" section label
        var langLabel = new Label
        {
            Name = "LanguageLabel",
            Text = Tr("settings.language"),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        langLabel.AddThemeFontSizeOverride("font_size", 28);
        langLabel.AddThemeColorOverride("font_color", Colors.White);
        modal.AddChild(langLabel);

        // 3 radio-style checkboxes (CheckBox acts as radio when grouped
        // — only one can be selected at a time because the apply handler
        // un-checks the others in OnAnyCheckBoxToggled).
        _englishBox = MakeLanguageCheckBox(Tr("English"),   EnLocale);
        _spanishBox = MakeLanguageCheckBox(Tr("Español"),   EsLocale);
        _portugueseBox = MakeLanguageCheckBox(Tr("Português"), PtBrLocale);

        modal.AddChild(_englishBox);
        modal.AddChild(_spanishBox);
        modal.AddChild(_portugueseBox);

        modal.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

        _applyButton = new Button
        {
            Text = Tr("Apply"),
            CustomMinimumSize = new Vector2(220, 60),
            Disabled = true, // no change yet
        };
        _applyButton.AddThemeFontSizeOverride("font_size", 24);
        _applyButton.Pressed += OnApplyPressed;
        modal.AddChild(_applyButton);

        modal.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

        _cancelButton = new Button
        {
            Text = Tr("Cancel"),
            CustomMinimumSize = new Vector2(220, 60),
        };
        _cancelButton.AddThemeFontSizeOverride("font_size", 24);
        _cancelButton.Pressed += OnCancelPressed;
        modal.AddChild(_cancelButton);
    }

    protected override void OnShow(SettingsArgs args)
    {
        GD.Print("[SettingsScreen] OnShow");
        // Snapshot current locale → populate checkboxes.
        var current = LocaleManager.Instance?.CurrentLocale ?? EnLocale;
        _pendingLocale = current;
        _englishBox.ButtonPressed   = (current == EnLocale);
        _spanishBox.ButtonPressed   = (current == EsLocale);
        _portugueseBox.ButtonPressed = (current == PtBrLocale);
        UpdateApplyButton();
    }

    protected override void OnClose(SettingsResult result)
    {
        GD.Print($"[SettingsScreen] OnClose(changed={result.LocaleChanged}, applied={result.AppliedLocale})");
    }

    // ─── handlers ──────────────────────────────────────────────────────

    private void OnAnyCheckBoxToggled(string localeCode, bool pressed)
    {
        if (!pressed) return; // ignore the "uncheck" half of a radio toggle
        _pendingLocale = localeCode;
        UpdateApplyButton();
    }

    private void UpdateApplyButton()
    {
        var current = LocaleManager.Instance?.CurrentLocale ?? EnLocale;
        _applyButton.Disabled = (_pendingLocale == current);
    }

    private void OnApplyPressed()
    {
        GD.Print($"[SettingsScreen] Apply: {LocaleManager.Instance?.CurrentLocale} → {_pendingLocale}");
        var changed = (_pendingLocale != LocaleManager.Instance?.CurrentLocale);
        if (changed)
        {
            LocaleManager.Instance?.SetLocale(_pendingLocale);
            // Fire-and-forget save (LocalePreferences swallows errors).
            _ = LocalePreferences.SaveAsync(_pendingLocale);
        }
        CloseScreen(new SettingsResult(LocaleChanged: changed, AppliedLocale: _pendingLocale));
    }

    private void OnCancelPressed()
    {
        GD.Print("[SettingsScreen] Cancel");
        var current = LocaleManager.Instance?.CurrentLocale ?? EnLocale;
        CloseScreen(new SettingsResult(LocaleChanged: false, AppliedLocale: current));
    }

    private CheckBox MakeLanguageCheckBox(string label, string localeCode)
    {
        var box = new CheckBox
        {
            Text = label,
            CustomMinimumSize = new Vector2(280, 50),
        };
        box.AddThemeFontSizeOverride("font_size", 24);
        // Capture localeCode in closure
        var captured = localeCode;
        box.Toggled += pressed => OnAnyCheckBoxToggled(captured, pressed);
        return box;
    }
}
