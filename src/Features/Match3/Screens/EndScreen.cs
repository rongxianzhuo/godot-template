using Godot;
using GameFramework;
using GodotTemplate.Core;
using GodotTemplate.Features.Settings;

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
///       ├── Settings button  (v1.2 polish #4, opens SettingsScreen modal)
///       ├── Spacer (10 px)
///       └── Main Menu button
/// </code>
/// </summary>
public sealed partial class EndScreen : Screen<EndScreenArgs, EndChoice>
{
    private Label _headlineLabel = null!;
    private Label _subtitleLabel = null!;
    private Button _playAgainButton = null!;

    // Last shown args — used to re-render labels after a locale change
    // (SettingsScreen returns to this EndScreen; if user picked a new
    // locale, we need to re-read Tr() for headline + subtitle).
    private bool _lastWon;
    private int _lastScore;
    private int _lastMovesUsed;
    private int _lastMovesMax;
    private int _lastLevel;

    public EndScreen()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        // v1.2 polish #2: default theme Forest — overridden in OnShow with
        // args.Level (matches the GameScreen that just closed). See
        // src/Core/ThemeBuilder.cs for the per-theme font mapping.
        Theme = ThemeBuilder.BuildTheme(ThemeKind.Forest);

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

        // v1.2 polish #4 (per ADR-0014): Settings entry point. Opens
        // SettingsScreen as a modal — on close, end screen stays
        // (no flow change).
        var settingsButton = MakeButton(Tr("Settings"));
        settingsButton.Pressed += OnSettingsPressed;
        center.AddChild(settingsButton);

        var spacer3 = new Control { CustomMinimumSize = new Vector2(0, 10) };
        center.AddChild(spacer3);

        var mainMenuButton = MakeButton(Tr("Main Menu"));
        mainMenuButton.Pressed += OnMainMenuPressed;
        center.AddChild(mainMenuButton);
    }

    protected override void OnShow(EndScreenArgs args)
    {
        GD.Print($"[EndScreen] OnShow(won={args.Won} score={args.FinalScore} " +
                 $"moves={args.MovesUsed}/{args.MovesMax} level={args.Level})");

        // v1.2 polish #2: apply per-theme font based on the level the
        // player just finished. Matches GameScreen's theme for visual
        // continuity (e.g. level 25 → Desert on both screens).
        Theme = ThemeBuilder.BuildTheme(ThemeKindUtils.FromLevel(args.Level));

        // Cache args so OnSettingsPressed can re-render after a locale change.
        _lastWon      = args.Won;
        _lastScore    = args.FinalScore;
        _lastMovesUsed = args.MovesUsed;
        _lastMovesMax = args.MovesMax;
        _lastLevel    = args.Level;

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

    private async void OnSettingsPressed()
    {
        GD.Print("[EndScreen] Settings pressed → ShowAsync<SettingsScreen>");
        // SettingsScreen is modal — ShowAsync returns after user clicks
        // Apply/Cancel. End screen stays as underlying, modal is dimmed.
        var result = await ScreenManager.Instance.ShowAsync<SettingsScreen, SettingsArgs, SettingsResult>(
            new SettingsArgs());
        if (result.LocaleChanged)
        {
            // v1.2 polish #4: re-read Tr() for headline + subtitle so the
            // new locale is reflected immediately. Tr() does not auto-refresh
            // already-rendered Labels (per LocaleManager docs caveat) —
            // callers must re-apply manually.
            _headlineLabel.Text = _lastWon ? Tr("You Won!") : Tr("Game Over");
            _subtitleLabel.Text = string.Format(
                Tr("Score: {0} — {1} of {2} moves used"),
                _lastScore, _lastMovesUsed, _lastMovesMax);
            GD.Print($"[EndScreen] locale changed to {result.AppliedLocale}, labels refreshed");
        }
        GD.Print("[EndScreen] SettingsScreen closed → returning to end screen");
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
