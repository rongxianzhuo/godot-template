using Godot;
using GameFramework;
using GodotTemplate.Core;

namespace GodotTemplate.Features.Match3.Screens;

/// <summary>
/// End screen — shown after <see cref="GameScreen"/> closes with a
/// <see cref="GameResult"/>. Renders the headline (Win/Lose + colour),
/// final score, and lets the player retry or return to title.
///
/// Inherits <see cref="Screen{TOpenArg, TCloseResult}"/> with typed open
/// arg <see cref="EndScreenArgs"/> (carries outcome + score breakdown)
/// and typed close result <see cref="EndChoice"/> (PlayAgain / MainMenu).
///
/// Scene tree (built in constructor, no .tscn):
/// <code>
///   EndScreen (Control, FullRect anchors)
///   ├── Background (TextureRect, bg_game.png — re-used for end state)
///   └── Center (VBoxContainer, centered)
///       ├── Headline label ("You Won!" green / "Game Over" red)
///       ├── Subtitle label ("Score: 1500 — 8 of 20 moves used")
///       ├── Spacer (40 px)
///       ├── Play Again button
///       ├── Spacer (20 px)
///       └── Main Menu button
/// </code>
/// </summary>
public sealed partial class EndScreen : Screen<EndScreenArgs, EndChoice>
{
    private Label _headlineLabel = null!;
    private Label _subtitleLabel = null!;
    private Button _playAgainButton = null!;

    public EndScreen()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        // v1.1 polish #3: apply Fredoka-Bold theme to all child Controls
        // (Background, Headline + Subtitle labels, Play Again / Main Menu
        // buttons). See src/Core/ThemeBuilder.cs.
        Theme = ThemeBuilder.BuildCosmicTheme();

        var bg = new TextureRect
        {
            Name = "Background",
            Texture = SpritePaths.LoadBackground("game"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);

        var center = new VBoxContainer
        {
            Name = "Center",
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        center.SetAnchorsPreset(Control.LayoutPreset.Center);
        AddChild(center);

        _headlineLabel = new Label
        {
            Name = "HeadlineLabel",
            Text = Tr("Game Over"),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _headlineLabel.AddThemeFontSizeOverride("font_size", 64);
        center.AddChild(_headlineLabel);

        _subtitleLabel = new Label
        {
            Name = "SubtitleLabel",
            Text = string.Format(Tr("Score: {0}"), 0),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _subtitleLabel.AddThemeFontSizeOverride("font_size", 28);
        _subtitleLabel.AddThemeColorOverride("font_color", Colors.White);
        center.AddChild(_subtitleLabel);

        var spacer = new Control { CustomMinimumSize = new Vector2(0, 40) };
        center.AddChild(spacer);

        _playAgainButton = MakeButton(Tr("Play Again"));
        _playAgainButton.Pressed += OnPlayAgainPressed;
        center.AddChild(_playAgainButton);

        var spacer2 = new Control { CustomMinimumSize = new Vector2(0, 20) };
        center.AddChild(spacer2);

        var mainMenuButton = MakeButton(Tr("Main Menu"));
        mainMenuButton.Pressed += OnMainMenuPressed;
        center.AddChild(mainMenuButton);
    }

    protected override void OnShow(EndScreenArgs args)
    {
        GD.Print($"[EndScreen] OnShow(won={args.Won} score={args.FinalScore} " +
                 $"moves={args.MovesUsed}/{args.MovesMax})");

        _headlineLabel.Text = args.Won ? Tr("You Won!") : Tr("Game Over");
        _headlineLabel.AddThemeColorOverride("font_color", args.Won
            ? new Color(0.4f, 1.0f, 0.4f) // green for win
            : new Color(1.0f, 0.4f, 0.4f)); // red for game over
        _subtitleLabel.Text = string.Format(
            Tr("Score: {0} — {1} of {2} moves used"),
            args.FinalScore, args.MovesUsed, args.MovesMax);

        _playAgainButton.GrabFocus();
    }

    protected override void OnClose(EndChoice result)
    {
        GD.Print($"[EndScreen] OnClose(choice={result})");
    }

    private void OnPlayAgainPressed()
    {
        GD.Print("[EndScreen] Play Again pressed → CloseScreen(PlayAgain)");
        CloseScreen(EndChoice.PlayAgain);
    }

    private void OnMainMenuPressed()
    {
        GD.Print("[EndScreen] Main Menu pressed → CloseScreen(MainMenu)");
        CloseScreen(EndChoice.MainMenu);
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
