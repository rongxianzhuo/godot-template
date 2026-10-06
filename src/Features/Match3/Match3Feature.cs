using System;
using Godot;
using GodotTemplate.Core;
using GodotTemplate.Features.Match3.Events;
using GameFramework;

namespace GodotTemplate.Features.Match3;

/// <summary>
/// Match-3 game feature (v0.1 MVP). Wires the pure-C# algorithm layer
/// (Board / MatchEngine / ScoreManager / GameState) to a Godot 4 UI.
///
/// Architecture:
///   - This Node is auto-loaded by the project's <see cref="Bootstrap"/> because of
///     the <see cref="GodotFeatureAttribute"/> decorator (reflection scan).
///   - All UI is built programmatically (no .tscn for game scenes), per AGENTS.md.
///   - Sprite paths come from <see cref="SpritePaths"/> (Bev's domain).
///   - Scene transitions swap a single child Control inside this feature.
///
/// Migration (W2): subscribes to GamePhaseChanged via EventBus.Instance.
/// Old _state.PhaseChanged subscription kept as fallback for one release
/// (will be removed when _state.PhaseChanged event is removed in v1.1).
///
/// Scene flow:
///   title (Play) -> game (Play button)  -> title (Main Menu)
///                  game (out of moves)  -> endscreen (Play Again / Main Menu)
/// </summary>
[GodotFeature(Order = 200, Category = "gameplay")]
public partial class Match3Feature : Node
{
    // ---- algorithm layer (pure C#, no Godot refs) ----
    private readonly Board _board = new();
    private readonly MatchEngine _engine = new();
    private readonly ScoreManager _scores = new();
    private readonly GameState _state = new();

    // ---- view layer ----
    private Node? _currentScreen;
    private Match3BoardView? _boardView;
    private Label? _scoreLabel;
    private Label? _movesLabel;

    // ---- EventBus subscription (W2) ----
    private IDisposable? _phaseSub;

    public override void _Ready()
    {
        GD.Print("[Match3Feature] _Ready — wiring algorithm + UI");

        // Canonical event path (W2+): subscribe via EventBus.
        _phaseSub = EventBus.Instance.Subscribe<GamePhaseChanged>(OnPhaseChangedFromBus);

        // Fallback (back-compat for v1.0 forks still using old event):
#pragma warning disable CS0618 // 'PhaseChanged' is obsolete but kept for one release
        _state.PhaseChanged += OnPhaseChanged;
#pragma warning restore CS0618

        // Start at title screen.
        ShowTitle();
    }

    // ---- Title screen ----
    private void ShowTitle()
    {
        GD.Print("[Match3Feature] -> Title");
        _state.ReturnToTitle();
        SwapScreen(GameScenes.BuildTitleScreen(
            onPlay: StartNewGame,
            onQuit: () => GetTree().Quit()
        ));
    }

    // ---- Game screen ----
    private void StartNewGame()
    {
        GD.Print("[Match3Feature] -> Game (start)");
        _board.FillRandomNoOpeningMatch();   // fresh board every game
        _scores.Reset(level: 0, movesMax: 20, target: 500);
        _state.StartLevel(level: 0);
        // Re-bind board + engine (fresh start).
        RebuildGameScreen();
    }

    private void RebuildGameScreen()
    {
        // Clear any existing screen.
        SwapScreen(null);

        var root = new Control { Name = "GameScreen" };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        // Background
        var bg = new TextureRect
        {
            Texture = SpritePaths.LoadBackground("game"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(bg);

        // HUD on top
        var hud = GameScenes.BuildHud(_scores, out _scoreLabel, out _movesLabel);
        root.AddChild(hud);

        // Board view
        _boardView = new Match3BoardView { Name = "BoardView" };
        _boardView.MoveCommitted += OnMoveCommitted;
        root.AddChild(_boardView);
        _boardView.Bind(_board, _engine, _scores);
        UpdateHud();

        SwapScreen(root);
    }

    private void OnMoveCommitted(int points, int cleared, bool outOfMoves)
    {
        UpdateHud();

        // Win check: post-cascade score crosses target. Win supersedes out-of-moves.
        if (_scores.IsWon() && _state.Phase == GamePhase.Playing)
        {
            GD.Print($"[Match3Feature] Score {_scores.Score} >= Target {_scores.Target} -> Won");
            _state.NotifyWin();
            return;
        }
        if (outOfMoves)
        {
            GD.Print("[Match3Feature] Out of moves -> GameOver");
            _state.NotifyGameOver();
        }
    }

    private void UpdateHud()
    {
        if (_scoreLabel is not null) _scoreLabel.Text = $"Score: {_scores.Score}";
        if (_movesLabel is not null) _movesLabel.Text = $"Moves: {_scores.Moves}";
    }

    // ---- End screen ----
    private void ShowEndScreen()
    {
        GD.Print("[Match3Feature] -> EndScreen");
        SwapScreen(GameScenes.BuildEndScreen(
            won: _state.Phase == GamePhase.Won,
            onPlayAgain: StartNewGame,
            onMainMenu: ShowTitle
        ));
    }

    // ---- GameState -> UI mapping (canonical EventBus handler, W2+) ----
    private void OnPhaseChangedFromBus(GamePhaseChanged evt)
    {
        OnPhaseChanged(evt.NewPhase);
    }

    // ---- GameState -> UI mapping (legacy fallback, [Obsolete] in W2+) ----
    private void OnPhaseChanged(GamePhase newPhase)
    {
        switch (newPhase)
        {
            case GamePhase.Title:
                // Title screen owns itself; we don't auto-show it here.
                break;
            case GamePhase.Playing:
                // Game screen owns itself; rebuilt by StartNewGame.
                break;
            case GamePhase.Won:
            case GamePhase.GameOver:
                ShowEndScreen();
                break;
        }
    }

    // ---- Helpers ----
    private void SwapScreen(Node? newScreen)
    {
        if (_currentScreen is not null)
        {
            _currentScreen.QueueFree();
            _currentScreen = null;
        }
        _boardView = null;
        _scoreLabel = null;
        _movesLabel = null;

        if (newScreen is not null)
        {
            AddChild(newScreen);
            _currentScreen = newScreen;
        }
    }

    public override void _ExitTree()
    {
        _phaseSub?.Dispose();
        _phaseSub = null;
#pragma warning disable CS0618
        _state.PhaseChanged -= OnPhaseChanged;
#pragma warning restore CS0618
        GD.Print("[Match3Feature] _ExitTree — clean");
    }
}
