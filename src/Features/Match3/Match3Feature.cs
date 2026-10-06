using Godot;
using GameFramework;
using GodotTemplate.Core;
using GodotTemplate.Features.Match3.Screens;

namespace GodotTemplate.Features.Match3;

/// <summary>
/// Match-3 game feature (W3: ScreenManager orchestrator).
///
/// Previously (W1-W2) this class owned the algorithm layer, the view layer,
/// and the screen-swap machinery (SwapScreen private method). In W3 it
/// becomes a thin orchestrator that just chains ScreenManager ShowAsync
/// calls — Title → Game → End → Title (or Game for PlayAgain).
///
/// Scene flow:
///   TitleScreen (Play)        → GameScreen (show with GameStartArgs)
///   GameScreen (won/oom)      → EndScreen (show with EndScreenArgs)
///   EndScreen (Play Again)    → GameScreen (restart same level)
///   EndScreen (Main Menu)     → TitleScreen (loop)
///   TitleScreen (Quit)        → GetTree().Quit() (app exit)
///
/// Algorithm + view ownership moved to <see cref="GameScreen"/> — each
/// game gets fresh Board / MatchEngine / ScoreManager / GameState
/// instances, so no shared mutable state across the loop.
///
/// W2 EventBus migration: GameState + ScoreManager still publish to
/// EventBus (W2 contract preserved). Match3Feature no longer subscribes
/// — the GameScreen handles win/out-of-moves detection locally via
/// <see cref="Match3BoardView.MoveCommitted"/>. Match3SmokeTest still
/// subscribes directly for headless verification (W2 path preserved).
/// </summary>
[GodotFeature(Order = 200, Category = "gameplay")]
public partial class Match3Feature : Node
{
    public override async void _Ready()
    {
        GD.Print("[Match3Feature] _Ready — starting ScreenManager loop");

        // Top-level loop: Title → Game → End → (Title or Game again).
        // TitleScreen has no typed close result; Quit exits the app via
        // GetTree().Quit() so it never returns to this loop. Play returns
        // here via CloseScreen() (resolves the ShowAsync Task).
        //
        // EndScreen returns EndChoice:
        //   - PlayAgain → fall through to the next GameScreen ShowAsync
        //   - MainMenu  → loop continues back to TitleScreen
        while (true)
        {
            // 1. Title → Play
            await ScreenManager.Instance.ShowAsync<TitleScreen>();

            // 2. Title → Game (always level 0 in v1.0; future: per-level config).
            var gameResult = await ScreenManager.Instance.ShowAsync<GameScreen, GameStartArgs, GameResult>(
                new GameStartArgs(Level: 0));

            // 3. Game → End (always; out-of-moves or win both reach here).
            var choice = await ScreenManager.Instance.ShowAsync<EndScreen, EndScreenArgs, EndChoice>(
                new EndScreenArgs(
                    Won: gameResult.Won,
                    FinalScore: gameResult.Score,
                    MovesUsed: gameResult.MovesUsed,
                    MovesMax: gameResult.MovesMax));

            // 4. End → Title or restart Game.
            //    PlayAgain: loop iteration continues, ShowAsync<GameScreen>() again.
            //    MainMenu:  loop iteration continues, falls back to ShowAsync<TitleScreen>().
            //    (No Quit from EndScreen in v1.0 — Quit is only on TitleScreen.)
            GD.Print($"[Match3Feature] EndScreen returned: {choice} → looping");
        }
    }
}
