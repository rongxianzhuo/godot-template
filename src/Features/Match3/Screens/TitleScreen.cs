using Godot;
using GameFramework;
using GodotTemplate.Core;
using GodotTemplate.Features.Settings;

namespace GodotTemplate.Features.Match3.Screens;

/// <summary>
/// Title screen — shown when Match3 starts or after EndScreen "Main Menu".
///
/// Inherits <see cref="Screen"/> (no typed open arg, no typed close result).
/// Play button calls <see cref="Screen.CloseScreen()"/> to resolve the
/// ShowAsync Task on the caller; Quit button calls <c>GetTree().Quit()</c>
/// to terminate the app directly (no return-to-loop).
///
/// Scene tree (built in constructor, no .tscn):
/// <code>
///   TitleScreen (Control, FullRect anchors)
///   ├── Background (TextureRect, bg_title.png)
///   └── Center (VBoxContainer, centered)
///       ├── Title label ("Magic Match", 72pt gold)
///       ├── Spacer (40 px)
///       ├── Play button
///       ├── Spacer (20 px)
///       ├── Settings button  (v1.2 polish #4, opens SettingsScreen modal)
///       ├── Spacer (10 px)
///       └── Quit button
/// </code>
/// </summary>
public sealed partial class TitleScreen : Screen
{
    private Button _playButton = null!;

    public TitleScreen()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop; // catch clicks so unfocused UI doesn't leak

        // v1.2 polish #2: per-theme font (Forest as default — game hasn't
        // started, so no level-based theme context yet). EndScreen uses
        // the same Forest theme for visual continuity on Main Menu return.
        // v1.1 used BuildCosmicTheme() (single DefaultFont = Fredoka-Bold);
        // v1.2 BuildTheme(Forest) is equivalent for Forest (Fredoka-Bold
        // is the Forest Primary font per Christine font-per-theme-spec §3).
        Theme = ThemeBuilder.BuildTheme(ThemeKind.Forest);

        // Background (reuses Christine's bg_title.png per SpritePaths)
        var bg = new TextureRect
        {
            Name = "Background",
            Texture = SpritePaths.LoadBackground("title"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);

        // Centered VBox of title + buttons
        var center = new VBoxContainer
        {
            Name = "Center",
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        center.SetAnchorsPreset(Control.LayoutPreset.Center);
        AddChild(center);

        var titleLabel = new Label
        {
            Name = "TitleLabel",
            Text = Tr("Magic Match"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 72);
        titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.3f)); // gold-ish
        center.AddChild(titleLabel);

        var spacer = new Control { CustomMinimumSize = new Vector2(0, 40) };
        center.AddChild(spacer);

        _playButton = MakeButton(Tr("Play"));
        _playButton.Pressed += OnPlayPressed;
        center.AddChild(_playButton);

        var spacer2 = new Control { CustomMinimumSize = new Vector2(0, 20) };
        center.AddChild(spacer2);

        // v1.2 polish #4 (per ADR-0014): Settings entry point. Opens
        // SettingsScreen as a modal; on close, title screen stays
        // (no flow change — user can keep playing).
        var settingsButton = MakeButton(Tr("Settings"));
        settingsButton.Pressed += OnSettingsPressed;
        center.AddChild(settingsButton);

        var spacer3 = new Control { CustomMinimumSize = new Vector2(0, 10) };
        center.AddChild(spacer3);

        var quitButton = MakeButton(Tr("Quit"));
        quitButton.Pressed += OnQuitPressed;
        center.AddChild(quitButton);
    }

    protected override void OnShow()
    {
        GD.Print("[TitleScreen] OnShow");
        _playButton.GrabFocus();
    }

    protected override void OnClose()
    {
        GD.Print("[TitleScreen] OnClose");
    }

    private void OnPlayPressed()
    {
        GD.Print("[TitleScreen] Play pressed → CloseScreen()");
        CloseScreen();
    }

    private async void OnSettingsPressed()
    {
        GD.Print("[TitleScreen] Settings pressed → ShowAsync<SettingsScreen>");
        // SettingsScreen is modal — ShowAsync returns after user clicks
        // Apply/Cancel. Title screen stays as underlying, modal is dimmed.
        // We don't do anything with the result — the SettingsScreen itself
        // applied the locale + persisted via LocalePreferences.
        await ScreenManager.Instance.ShowAsync<SettingsScreen, SettingsArgs, SettingsResult>(
            new SettingsArgs());
        GD.Print("[TitleScreen] SettingsScreen closed → returning to title");
    }

    private void OnQuitPressed()
    {
        GD.Print("[TitleScreen] Quit pressed → GetTree().Quit()");
        GetTree().Quit();
    }

    private static Button MakeButton(string label)
    {
        var btn = new Button
        {
            Text = label,
            CustomMinimumSize = new Vector2(220, 70),
            ClipText = false,
        };
        btn.AddThemeFontSizeOverride("font_size", 28);
        return btn;
    }
}
