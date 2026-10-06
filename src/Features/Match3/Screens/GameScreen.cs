using Godot;
using GameFramework;

namespace GodotTemplate.Features.Match3.Screens;

/// <summary>
/// Game screen — owns the live algorithm + view for a single Match3 session.
///
/// Inherits <see cref="Screen{TOpenArg, TCloseResult}"/> with typed open arg
/// <see cref="GameStartArgs"/> (carries level) and typed close result
/// <see cref="GameResult"/> (carries win/lose + final score).
///
/// Algorithm ownership: GameScreen owns its own <see cref="Board"/> /
/// <see cref="MatchEngine"/> / <see cref="ScoreManager"/> / <see cref="GameState"/>
/// instances. These are created fresh in <see cref="OnShow"/> so each game
/// starts from a clean state regardless of previous games (no shared
/// mutable state across the title→game→end loop).
///
/// Scene tree (built in constructor, no .tscn):
/// <code>
///   GameScreen (Control, FullRect anchors)
///   ├── Background (TextureRect, bg_game.png)
///   ├── HUD (HBoxContainer, top-wide)
///   │   ├── ScoreLabel ("Score: 0")
///   │   └── MovesLabel ("Moves: 20")
///   └── BoardView (Match3BoardView, grid)
/// </code>
///
/// Lifecycle:
///   1. <see cref="OnShow"/> creates fresh algorithm + rebuilds HUD + binds
///      <see cref="Match3BoardView"/>, then subscribes to its MoveCommitted.
///   2. User swipes cells → Match3BoardView commits swap → fires MoveCommitted.
///   3. <see cref="OnMoveCommitted"/> checks win/out-of-moves and calls
///      <see cref="CloseScreen"/> with <see cref="GameResult"/> when the
///      game is over.
///   4. ScreenManager awaits → Match3Feature resumes the loop → ShowAsync&lt;EndScreen&gt;.
/// </summary>
public sealed partial class GameScreen : Screen<GameStartArgs, GameResult>
{
    // ---- algorithm layer (recreated each OnShow) ----
    private Board _board = null!;
    private MatchEngine _engine = null!;
    private ScoreManager _scores = null!;
    private GameState _state = null!;

    // ---- view layer ----
    private Match3BoardView? _boardView;
    private Label _scoreLabel = null!;
    private Label _movesLabel = null!;

    public GameScreen()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        // Background (reuses Christine's bg_game.png per SpritePaths)
        var bg = new TextureRect
        {
            Name = "Background",
            Texture = SpritePaths.LoadBackground("game"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
        };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);

        // HUD on top (Score + Moves labels). Labels themselves are initialised
        // in OnShow (need a ScoreManager instance to read initial values).
        var hud = new HBoxContainer
        {
            Name = "HUD",
        };
        hud.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        hud.AddThemeConstantOverride("separation", 40);
        AddChild(hud);

        _scoreLabel = new Label
        {
            Name = "ScoreLabel",
            Text = "Score: 0",
        };
        _scoreLabel.AddThemeFontSizeOverride("font_size", 28);
        _scoreLabel.AddThemeColorOverride("font_color", Colors.White);
        hud.AddChild(_scoreLabel);

        _movesLabel = new Label
        {
            Name = "MovesLabel",
            Text = "Moves: 0",
        };
        _movesLabel.AddThemeFontSizeOverride("font_size", 28);
        _movesLabel.AddThemeColorOverride("font_color", Colors.White);
        hud.AddChild(_movesLabel);

        // Board view placeholder — bound to algorithm in OnShow (need fresh
        // Board + Engine + ScoreManager per game).
        _boardView = new Match3BoardView { Name = "BoardView" };
        AddChild(_boardView);
    }

    protected override void OnShow(GameStartArgs args)
    {
        GD.Print($"[GameScreen] OnShow(level={args.Level})");

        // Fresh algorithm per game — no shared state across the loop.
        _board = new Board();
        _engine = new MatchEngine();
        _scores = new ScoreManager();
        _state = new GameState();

        // Level config (W3: level 0 fixed. Future: pull from level DB per args.Level).
        const int movesMax = 20;
        const int target = 500;

        _board.FillRandomNoOpeningMatch();
        _scores.Reset(level: args.Level, movesMax: movesMax, target: target);
        _state.StartLevel(level: args.Level);

        // Bind the board view to the new algorithm instances.
        // Note: re-binding a Match3BoardView also clears its cells + refreshes.
        _boardView!.Bind(_board, _engine, _scores);
        _boardView.MoveCommitted -= OnMoveCommitted; // guard against double-sub
        _boardView.MoveCommitted += OnMoveCommitted;

        UpdateHud();
    }

    protected override void OnClose(GameResult result)
    {
        GD.Print($"[GameScreen] OnClose(won={result.Won} score={result.Score} " +
                 $"moves={result.MovesUsed}/{result.MovesMax})");

        // Unsubscribe to avoid leaks if ScreenManager keeps the instance alive
        // (currently force-removes, but explicit unsubscribe is defensive).
        if (_boardView != null) _boardView.MoveCommitted -= OnMoveCommitted;
    }

    private void OnMoveCommitted(int points, int cleared, bool outOfMoves)
    {
        UpdateHud();

        // Win check first (post-cascade score crossing target wins even if
        // the same move also exhausts moves).
        if (_scores.IsWon() && _state.Phase == GamePhase.Playing)
        {
            GD.Print($"[GameScreen] Score {_scores.Score} >= Target {_scores.Target} → Won");
            CloseScreen(new GameResult(
                Won: true,
                Score: _scores.Score,
                MovesUsed: _scores.MovesMax - _scores.Moves,
                MovesMax: _scores.MovesMax));
            return;
        }

        if (outOfMoves)
        {
            GD.Print("[GameScreen] Out of moves → GameOver");
            CloseScreen(new GameResult(
                Won: false,
                Score: _scores.Score,
                MovesUsed: _scores.MovesMax,
                MovesMax: _scores.MovesMax));
        }
    }

    private void UpdateHud()
    {
        if (_scoreLabel != null) _scoreLabel.Text = $"Score: {_scores.Score}";
        if (_movesLabel != null) _movesLabel.Text = $"Moves: {_scores.Moves}";
    }
}
