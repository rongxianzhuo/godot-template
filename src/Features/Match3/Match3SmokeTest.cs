using Godot;
using GodotTemplate.Core;

namespace GodotTemplate.Features.Match3;

/// <summary>
/// Optional dev-time smoke test for the match-3 game loop. Only runs when the
/// MATCH3_SMOKE_TEST=1 environment variable is set, so it stays inert in normal
/// play and release builds.
///
/// What it does (all from _Ready, no .tscn):
///   1. Build the algorithm layer directly.
///   2. Verify FillRandomNoOpeningMatch produces a board with zero segments.
///   3. Find a legal swap, apply it, verify cascades settle to zero segments.
///   4. Print PASS/FAIL summary and quit.
///
/// This complements <c>tests/Match3Tests/</c> by exercising the same code in a
/// real Godot 4 runtime context (catches class-load issues that dotnet run
/// wouldn't).
/// </summary>
[GodotFeature(Order = 950, Category = "debug")]
public partial class Match3SmokeTest : Node
{
    public override void _Ready()
    {
        // Default off; enable with MATCH3_SMOKE_TEST=1 godot --headless ...
        var enabled = OS.GetEnvironment("MATCH3_SMOKE_TEST");
        if (enabled != "1" && enabled.ToLower() != "true")
        {
            GD.Print("[Match3SmokeTest] disabled (set MATCH3_SMOKE_TEST=1 to enable)");
            return;
        }

        GD.Print("[Match3SmokeTest] === START ===");
        try
        {
            Run();
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[Match3SmokeTest] FAIL: {e.Message}");
            GD.Print($"[Match3SmokeTest] === END (FAIL) ===");
            GetTree().Quit(1);
            return;
        }
        GD.Print("[Match3SmokeTest] === END (PASS) ===");
        // Defer quit so other features get a clean _Ready.
        CallDeferred(Node.MethodName.QueueFree);
    }

    private void Run()
    {
        var board = new Board(seed: 1);
        var engine = new MatchEngine();
        var scores = new ScoreManager();
        scores.Reset(level: 0, movesMax: 20, target: 500);

        // 1) Initial fill must have no opening match.
        board.FillRandomNoOpeningMatch();
        var initSegments = engine.FindAllSegments(board);
        Require(initSegments.Count == 0,
            $"initial fill has 0 segments (got {initSegments.Count})");
        GD.Print($"[Match3SmokeTest] initial fill clean (64 cells, {CountNonEmpty(board)} non-empty)");

        // 2) Find a legal swap pair via dry-run probe (board is unmodified).
        CellPos? pair = FindLegalSwap(board, engine);
        Require(pair.HasValue, "found at least one legal swap pair");
        CellPos a = pair!.Value;

        // 3) Find the actual partner that creates a match (dry-run).
        CellPos? partner = null;
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        for (int i = 0; i < 4 && !partner.HasValue; i++)
        {
            var probe = new CellPos(a.X + dx[i], a.Y + dy[i]);
            if (probe.X < 0 || probe.X >= Board.Width || probe.Y < 0 || probe.Y >= Board.Height) continue;
            board.Swap(a, probe);
            var segs = engine.FindAllSegments(board);
            board.Swap(a, probe);
            if (segs.Count > 0) partner = probe;
        }
        Require(partner.HasValue, "found a partner that produces a match");
        CellPos b = partner!.Value;

        // 4) Apply real swap; verify cascades settle to zero segments.
        GD.Print($"[Match3SmokeTest] applying swap {a} <-> {b}");
        var result = engine.TrySwap(board, a, b);
        Require(result.IsLegal, "swap was legal (adjacent)");
        Require(result.HasMatch, "swap produced a match");
        Require(result.Cascades.Count >= 1, $"at least 1 cascade (got {result.Cascades.Count})");
        var finalSegments = engine.FindAllSegments(board);
        Require(finalSegments.Count == 0,
            $"final board has 0 segments after cascade (got {finalSegments.Count})");

        // 4) Score should have moved (legal swap with cascade > 0).
        int totalPoints = 0;
        foreach (var c in result.Cascades) totalPoints += c.TotalPoints;
        Require(totalPoints > 0, $"cascade awarded > 0 points (got {totalPoints})");
        foreach (var c in result.Cascades) scores.AwardCascade(c);
        scores.UseMove();
        Require(scores.Score == totalPoints, $"scoreboard shows {totalPoints} (got {scores.Score})");
        Require(scores.Moves == 19, $"moves decremented to 19 (got {scores.Moves})");

        // 5) GameState transitions.
        var state = new GameState();
        Require(state.Phase == GamePhase.Title, "starts at Title");
        state.StartLevel(0);
        Require(state.Phase == GamePhase.Playing, "after StartLevel -> Playing");
        state.NotifyWin();
        Require(state.Phase == GamePhase.Won, "after NotifyWin -> Won");

        GD.Print($"[Match3SmokeTest] score={scores.Score} moves={scores.Moves}/{scores.MovesMax} target={scores.Target}");
        GD.Print("[Match3SmokeTest] ALL CHECKS PASSED");
    }

    private static int CountNonEmpty(Board board)
    {
        int n = 0;
        for (int x = 0; x < Board.Width; x++)
            for (int y = 0; y < Board.Height; y++)
                if (board[x, y] != GemType.None) n++;
        return n;
    }

    private static CellPos? FindLegalSwap(Board board, MatchEngine engine)
    {
        // Probe each cell with a dry-run swap-test: if a swap would match,
        // return that cell. We must NOT mutate board in this probe —
        // TrySwap is legal+match OR illegal-undone. The side effect on the
        // board only happens when IsLegal=false (auto-undo), but for a clean
        // initial board we'd find plenty of legal swaps without undo.
        for (int x = 0; x < Board.Width; x++)
            for (int y = 0; y < Board.Height; y++)
            {
                var a = new CellPos(x, y);
                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };
                for (int i = 0; i < 4; i++)
                {
                    var b = new CellPos(a.X + dx[i], a.Y + dy[i]);
                    if (b.X < 0 || b.X >= Board.Width || b.Y < 0 || b.Y >= Board.Height) continue;
                    // Use FindAllSegments to test: dry-run preview swap, then check segments.
                    // We do this with explicit swap -> check -> undo so board is untouched.
                    board.Swap(a, b);
                    var segs = engine.FindAllSegments(board);
                    board.Swap(a, b);  // undo
                    if (segs.Count > 0) return a;
                }
            }
        return null;
    }

    private static CellPos? FindLegalSwapPartner(Board board, MatchEngine engine, CellPos a)
    {
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            var b = new CellPos(a.X + dx[i], a.Y + dy[i]);
            if (b.X < 0 || b.X >= Board.Width || b.Y < 0 || b.Y >= Board.Height) continue;
            var result = engine.TrySwap(board, a, b);
            if (result.IsLegal && result.HasMatch) return b;
        }
        return null;
    }

    private static void Require(bool cond, string msg)
    {
        if (!cond) throw new System.Exception($"assertion failed: {msg}");
    }
}
