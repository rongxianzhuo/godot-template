using Godot;
using GameFramework;
using GodotTemplate.Core;

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

        // v1.1 polish #3: apply Fredoka-Bold theme to all child Controls
        // (Title label, Play/Quit buttons). See src/Core/ThemeBuilder.cs.
        Theme = ThemeBuilder.BuildCosmicTheme();

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
