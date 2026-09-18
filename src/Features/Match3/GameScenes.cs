using Godot;

namespace GodotTemplate.Features.Match3;

/// <summary>
/// Static helpers that build the three game screens (title / game / end) as
/// programmatic UI trees — no .tscn files involved. AGENTS.md says don't
/// hand-edit .tscn; building UI in code is the feature-bootstrap way.
///
/// Each builder returns a fully wired Control that's ready to be AddChild'd.
/// The caller (Match3Feature) owns lifecycle (QueueFree on transition).
/// </summary>
internal static class GameScenes
{
    /// <summary>Build the title screen. Buttons wired to <paramref name="onPlay"/> and <paramref name="onQuit"/>.</summary>
    public static Control BuildTitleScreen(System.Action onPlay, System.Action onQuit)
    {
        var root = new Control { Name = "TitleScreen" };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        // Background
        var bg = new TextureRect
        {
            Texture = SpritePaths.LoadBackground("title"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(bg);

        // Centered vbox of title + buttons
        var center = new VBoxContainer
        {
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        center.SetAnchorsPreset(Control.LayoutPreset.Center);
        root.AddChild(center);

        var titleLabel = new Label
        {
            Text = "Magic Match",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 72);
        titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.3f)); // gold-ish
        center.AddChild(titleLabel);

        // Spacer
        var spacer = new Control { CustomMinimumSize = new Vector2(0, 40) };
        center.AddChild(spacer);

        center.AddChild(MakeButton("Play", onPlay));
        var spacer2 = new Control { CustomMinimumSize = new Vector2(0, 20) };
        center.AddChild(spacer2);
        center.AddChild(MakeButton("Quit", onQuit));

        return root;
    }

    /// <summary>Build the end screen. Buttons wired to <paramref name="onPlayAgain"/> and <paramref name="onMainMenu"/>.</summary>
    public static Control BuildEndScreen(bool won, System.Action onPlayAgain, System.Action onMainMenu)
    {
        var root = new Control { Name = "EndScreen" };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        var bg = new TextureRect
        {
            Texture = SpritePaths.LoadBackground("game"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(bg);

        var center = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        center.SetAnchorsPreset(Control.LayoutPreset.Center);
        root.AddChild(center);

        var headline = new Label
        {
            Text = won ? "You Won!" : "Game Over",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        headline.AddThemeFontSizeOverride("font_size", 64);
        headline.AddThemeColorOverride("font_color", won
            ? new Color(0.4f, 1.0f, 0.4f)
            : new Color(1.0f, 0.4f, 0.4f));
        center.AddChild(headline);

        var spacer = new Control { CustomMinimumSize = new Vector2(0, 40) };
        center.AddChild(spacer);

        center.AddChild(MakeButton("Play Again", onPlayAgain));
        var spacer2 = new Control { CustomMinimumSize = new Vector2(0, 20) };
        center.AddChild(spacer2);
        center.AddChild(MakeButton("Main Menu", onMainMenu));

        return root;
    }

    /// <summary>Build the HUD chrome above the board (score + moves labels). Returned Control is a HBox. </summary>
    public static Control BuildHud(ScoreManager scores, out Label scoreLabel, out Label movesLabel)
    {
        var row = new HBoxContainer { Name = "HUD" };
        row.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        row.AddThemeConstantOverride("separation", 40);

        scoreLabel = new Label { Text = "Score: 0", Name = "ScoreLabel" };
        scoreLabel.AddThemeFontSizeOverride("font_size", 28);
        scoreLabel.AddThemeColorOverride("font_color", Colors.White);
        row.AddChild(scoreLabel);

        movesLabel = new Label { Text = $"Moves: {scores.Moves}", Name = "MovesLabel" };
        movesLabel.AddThemeFontSizeOverride("font_size", 28);
        movesLabel.AddThemeColorOverride("font_color", Colors.White);
        row.AddChild(movesLabel);

        return row;
    }

    private static Button MakeButton(string label, System.Action onPressed)
    {
        var btn = new Button
        {
            Text = label,
            CustomMinimumSize = new Vector2(220, 70),
            ClipText = false,
        };
        btn.AddThemeFontSizeOverride("font_size", 28);
        btn.Pressed += onPressed;
        return btn;
    }
}