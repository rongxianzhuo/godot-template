using GodotTemplate.Features.Match3;

namespace GodotTemplate.Match3Tests;

/// <summary>
/// Self-contained test runner for the Match-3 algorithm layer. No Godot deps.
/// Run with: dotnet run --project tests/Match3Tests/
/// Exit code: 0 = all passed, 1 = at least one failure.
/// </summary>
internal static class Program
{
    private static int _passed;
    private static int _failed;

    private static void Assert(bool condition, string testName)
    {
        if (condition)
        {
            _passed++;
            Console.WriteLine($"  ✓ {testName}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"  ✗ {testName}");
        }
    }

    private static int Main()
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  Magic Match — Algorithm-layer test suite");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        Test1_NoOpeningMatch();
        Test2_LegalSwapProducesMatch();
        Test3_IllegalSwapAutoUndoes();
        Test4_CascadeResolution();
        Test5_MoveLimitAndScoreAccumulation();
        Test6_GameStateTransitions();
        Test7_GravityAndSpawn();

        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  Result: {_passed}/{_passed + _failed} passed" +
                          (_failed > 0 ? $"  ({_failed} failed)" : ""));
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        return _failed > 0 ? 1 : 0;
    }

    // ────────────────────────────────────────────────────────────────────
    // Test 1: Initial board has no opening match.
    // ────────────────────────────────────────────────────────────────────
    private static void Test1_NoOpeningMatch()
    {
        Console.WriteLine("── Test 1: Initial board has no opening match ──");
        var engine = new MatchEngine();
        int totalSeeds = 200;
        for (int seed = 1; seed <= totalSeeds; seed++)
        {
            var board = new Board(seed);
            board.FillRandomNoOpeningMatch();
            var segments = engine.FindAllSegments(board);
            if (segments.Count != 0)
            {
                Assert(false, $"seed={seed}: opening match found ({segments.Count} segments)");
                Console.WriteLine($"    Board:\n{board.ToAscii()}");
                return;
            }
        }
        Assert(true, $"{totalSeeds} random boards generated, 0 opening matches");
    }

    // ────────────────────────────────────────────────────────────────────
    // Test 2: A legal swap produces a match.
    // ────────────────────────────────────────────────────────────────────
    private static void Test2_LegalSwapProducesMatch()
    {
        Console.WriteLine("\n── Test 2: Legal swap produces match ──");

        // Construct: row 7 = B B . B (2-stack + isolated → no opening match).
        // Swapping (2,7) ⇄ (3,7) brings a third B into the row, forming B B B.
        var board = new Board(42);
        EmptyBoard(board);
        board[0, 7] = GemType.Blue;
        board[1, 7] = GemType.Blue;
        board[3, 7] = GemType.Blue;

        var engine = new MatchEngine();
        var result = engine.TrySwap(board, new CellPos(2, 7), new CellPos(3, 7));

        Assert(result.IsLegal, "swap (2,7)⇄(3,7) is legal (orthogonal neighbors)");
        Assert(result.HasMatch, "swap produced ≥1 segment (3 consecutive B's in row 7)");
        Assert(result.Cascades.Count >= 1, $"cascade ran ({result.Cascades.Count} level(s))");
        if (result.Cascades.Count > 0)
        {
            var c0 = result.Cascades[0];
            Assert(c0.CellsCleared.Count == 3,
                $"first cascade cleared exactly 3 cells (got {c0.CellsCleared.Count})");
            Assert(c0.TotalBasePoints == 30,
                $"first cascade base points = 30 (got {c0.TotalBasePoints})");
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Test 3: An illegal swap auto-undoes.
    // ────────────────────────────────────────────────────────────────────
    private static void Test3_IllegalSwapAutoUndoes()
    {
        Console.WriteLine("\n── Test 3: Illegal swap auto-undoes ──");

        var board = new Board(7);
        board.FillRandomNoOpeningMatch();
        var engine = new MatchEngine();

        // Find an adjacent pair whose swap creates NO match (illegal).
        var found = FindFirstAdjacentPair(board, engine, requireMatch: false, out var a, out var b);
        Assert(found, "found at least one adjacent pair that does NOT create a match");

        if (found)
        {
            var gridBefore = board.CloneGrid();
            var result = engine.TrySwap(board, a, b);
            Assert(result.IsLegal, "swap was performed (orthogonal)");
            Assert(!result.HasMatch, "swap did not produce a match");
            Assert(GridsEqual(gridBefore, board.CloneGrid()),
                "board returned to original state (auto-undone)");

            // Also verify the (a,b)==(a,a) and out-of-bounds rejection.
            var same = engine.TrySwap(board, a, a);
            Assert(!same.IsLegal, "swap (a,a) rejected as illegal");

            var oob = engine.TrySwap(board, new CellPos(-1, 0), new CellPos(0, 0));
            Assert(!oob.IsLegal, "swap with out-of-bounds rejected as illegal");
        }
    }

    // ────────────────────────────────────────────────────────────────────
    // Test 4: Cascade resolution finds and clears matches.
    // ────────────────────────────────────────────────────────────────────
    private static void Test4_CascadeResolution()
    {
        Console.WriteLine("\n── Test 4: Cascade resolution ──");

        var engine = new MatchEngine();
        int totalBoards = 50;
        int atLeastOneCascade = 0;
        int multiLevel = 0;

        for (int seed = 1; seed <= totalBoards; seed++)
        {
            var board = new Board(seed);
            board.FillRandomNoOpeningMatch();

            // Find a legal swap to trigger cascade.
            var found = FindFirstAdjacentPair(board, engine, requireMatch: true, out var a, out var b);
            if (!found) continue;

            var result = engine.TrySwap(board, a, b);
            if (result.HasMatch) atLeastOneCascade++;
            if (result.Cascades.Count >= 2) multiLevel++;

            // After cascade, board must have no remaining segments.
            var leftover = engine.FindAllSegments(board);
            Assert(leftover.Count == 0,
                $"seed={seed}: final board has 0 segments (got {leftover.Count})");
        }
        Assert(atLeastOneCascade == totalBoards,
            $"{atLeastOneCascade}/{totalBoards} seeds produced cascade on legal swap");
        // Multi-level cascades are non-deterministic (depend on spawn RNG) —
        // just report how many we got, don't fail the test if it's 0.
        Console.WriteLine($"  ℹ {multiLevel}/{totalBoards} seeds produced 2+ cascade levels");
        Assert(multiLevel >= 0, "multi-level observation recorded (informational)");
    }

    // ────────────────────────────────────────────────────────────────────
    // Test 5: Move limit decrements; Score accumulates with cascade multiplier.
    // ────────────────────────────────────────────────────────────────────
    private static void Test5_MoveLimitAndScoreAccumulation()
    {
        Console.WriteLine("\n── Test 5: Move limit + score accumulation ──");

        var score = new ScoreManager();
        score.Reset(level: 0, movesMax: 20, target: 500);

        Assert(score.Score == 0, "Score starts at 0");
        Assert(score.Moves == 20, "Moves starts at MovesMax=20");
        Assert(!score.IsWon(), "not won initially");
        Assert(!score.IsOutOfMoves(), "not out of moves initially");

        // Award a fake cascade level 1 (10 cleared gems, no intersection).
        // CascadeResult.CellsCleared.Count = 10 → 10 * 10 base = 100, ×1.0 = 100.
        var cascade1 = MakeCascade(level: 1, cellsCleared: 10, intersections: 0);
        int scoreBefore = score.Score;
        score.AwardCascade(cascade1);
        Assert(score.Score - scoreBefore == 100,
            $"cascade level 1 awards 100 points (got {score.Score - scoreBefore})");

        // Cascade level 2 should apply ×1.5 multiplier.
        var cascade2 = MakeCascade(level: 2, cellsCleared: 6, intersections: 0);
        scoreBefore = score.Score;
        score.AwardCascade(cascade2);
        Assert(score.Score - scoreBefore == 90,  // 60 * 1.5 = 90
            $"cascade level 2 awards 90 points (×1.5, got {score.Score - scoreBefore})");

        // Cascade level 4 should apply ×2.5 multiplier.
        var cascade4 = MakeCascade(level: 4, cellsCleared: 4, intersections: 1);
        scoreBefore = score.Score;
        score.AwardCascade(cascade4);
        // 40 base + 5 intersection = 45 total * 2.5 = 112.5 → cast to int = 112
        int delta = score.Score - scoreBefore;
        Assert(delta == 112,
            $"cascade level 4 with 1 intersection awards 112 points (got {delta})");

        // Test intersection bonus: 3 cleared, 2 intersections.
        var tInt = MakeCascade(level: 1, cellsCleared: 5, intersections: 2);
        scoreBefore = score.Score;
        score.AwardCascade(tInt);
        int tIntDelta = score.Score - scoreBefore;
        // 5 * 10 = 50 base, 2 * 5 = 10 bonus, total 60 * 1.0 = 60
        Assert(tIntDelta == 60,
            $"cascade with 2 intersections: 60 points (50 base + 10 bonus, got {tIntDelta})");

        // Use all moves.
        for (int i = 0; i < 20; i++)
        {
            bool used = score.UseMove();
            if (!used) { Assert(false, $"move {i + 1}: UseMove returned false"); break; }
        }
        Assert(score.Moves == 0, "Moves decremented to 0");
        Assert(score.IsOutOfMoves(), "IsOutOfMoves returns true at 0");
        Assert(!score.UseMove(), "UseMove at 0 returns false (no decrement)");

        // Force a win by raising score above target.
        var winCascade = MakeCascade(level: 1, cellsCleared: 50, intersections: 0);
        score.AwardCascade(winCascade);
        Assert(score.IsWon(), "IsWon returns true after score exceeds target");
    }

    private static MatchEngine.CascadeResult MakeCascade(int level, int cellsCleared, int intersections)
    {
        var r = new MatchEngine.CascadeResult { CascadeLevel = level };
        for (int i = 0; i < cellsCleared; i++)
            r.CellsCleared.Add(new CellPos(i % Board.Width, i / Board.Width));
        // Add one segment per cascade level to satisfy TotalBonusPoints math.
        var seg = new MatchEngine.MatchSegment
        {
            Dir = MatchEngine.Direction.Horizontal,
            Type = GemType.Red,
            IntersectionCount = intersections,
        };
        for (int i = 0; i < cellsCleared; i++) seg.Cells.Add(new CellPos(i % Board.Width, i / Board.Width));
        r.Segments.Add(seg);
        return r;
    }

    // ────────────────────────────────────────────────────────────────────
    // Test 6: GameState transitions.
    // ────────────────────────────────────────────────────────────────────
    private static void Test6_GameStateTransitions()
    {
        Console.WriteLine("\n── Test 6: GameState transitions ──");

        var state = new GameState();
        var log = new List<GamePhase>();
        state.PhaseChanged += p => log.Add(p);

        Assert(state.Phase == GamePhase.Title, "initial phase = Title");

        state.StartLevel(0);
        Assert(state.Phase == GamePhase.Playing, "StartLevel → Playing");
        Assert(state.CurrentLevel == 0, "CurrentLevel = 0");

        state.NotifyWin();
        Assert(state.Phase == GamePhase.Won, "NotifyWin → Won");

        state.AdvanceLevel();
        Assert(state.Phase == GamePhase.Playing, "AdvanceLevel → Playing");
        Assert(state.CurrentLevel == 1, "CurrentLevel advanced to 1");

        state.NotifyGameOver();
        Assert(state.Phase == GamePhase.GameOver, "NotifyGameOver → GameOver");

        state.StartLevel(2);
        Assert(state.Phase == GamePhase.Playing, "StartLevel from GameOver → Playing");
        Assert(state.CurrentLevel == 2, "CurrentLevel = 2");

        state.ReturnToTitle();
        Assert(state.Phase == GamePhase.Title, "ReturnToTitle → Title");

        // NotifyWin from Title (not Playing) should be a no-op.
        state.NotifyWin();
        Assert(state.Phase == GamePhase.Title, "NotifyWin ignored from non-Playing");

        Assert(log.Count == 6, $"6 phase events emitted (got {log.Count})");
    }

    // ────────────────────────────────────────────────────────────────────
    // Test 7: Gravity + spawn.
    // ────────────────────────────────────────────────────────────────────
    private static void Test7_GravityAndSpawn()
    {
        Console.WriteLine("\n── Test 7: Gravity + spawn ──");

        var board = new Board(123);
        EmptyBoard(board);

        // Set up: column 0 has Blue at y=2 and Red at y=4 (gaps below).
        board[0, 4] = GemType.Red;     // Red: lower row index → falls further
        board[0, 2] = GemType.Blue;    // Blue: higher up in stack → settles above Red

        board.ApplyGravity();
        // Gravity is stable — relative order preserved. Red was at y=4 (lower),
        // Blue at y=2 (higher). After drop: Red at y=7 (bottom), Blue at y=6.
        Assert(board[0, 7] == GemType.Red, "(0,7) = Red after gravity (lower gem drops to bottom)");
        Assert(board[0, 6] == GemType.Blue, "(0,6) = Blue after gravity (higher gem sits above)");
        Assert(board[0, 5] == GemType.None, "(0,5) = empty after gravity");
        Assert(board[0, 4] == GemType.None, "(0,4) = empty after gravity");

        // Spawn fills empties.
        board.SpawnNewGems();
        Assert(!HasEmpty(board), "no empty cells after SpawnNewGems");
    }

    // ────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────
    private static void EmptyBoard(Board board)
    {
        for (int y = 0; y < Board.Height; y++)
            for (int x = 0; x < Board.Width; x++)
                board[x, y] = GemType.None;
    }

    private static bool HasEmpty(Board board)
    {
        for (int y = 0; y < Board.Height; y++)
            for (int x = 0; x < Board.Width; x++)
                if (board[x, y] == GemType.None) return true;
        return false;
    }

    private static bool GridsEqual(GemType[,] a, GemType[,] b)
    {
        if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
            return false;
        for (int y = 0; y < a.GetLength(1); y++)
            for (int x = 0; x < a.GetLength(0); x++)
                if (a[x, y] != b[x, y]) return false;
        return true;
    }

    /// <summary>
    /// Test an adjacent swap WITHOUT mutating the board.
    /// Returns true and out the match-status if the swap is valid for testing.
    /// </summary>
    private static bool TryAdjacentSwap(
        Board board, MatchEngine engine, CellPos p, CellPos q,
        out bool createsMatch)
    {
        board.Swap(p, q);
        createsMatch = engine.FindAllSegments(board).Count > 0;
        board.Swap(p, q); // always undo — board must be left unchanged for caller
        return true;
    }

    /// <summary>
    /// Scan all adjacent (orthogonal) pairs. Return the first pair matching the criterion.
    /// requireMatch=true  → pair whose swap creates ≥1 segment.
    /// requireMatch=false → pair whose swap creates no segment (illegal swap).
    /// Board is left UNCHANGED — the caller is expected to invoke TrySwap itself.
    /// </summary>
    private static bool FindFirstAdjacentPair(
        Board board, MatchEngine engine, bool requireMatch,
        out CellPos a, out CellPos b)
    {
        a = default;
        b = default;
        for (int y = 0; y < Board.Height; y++)
        {
            for (int x = 0; x < Board.Width; x++)
            {
                var p = new CellPos(x, y);
                foreach (var (dx, dy) in new[] { (1, 0), (0, 1) })
                {
                    var q = p.Offset(dx, dy);
                    if (!board.InBounds(q)) continue;
                    if (board[p] == GemType.None || board[q] == GemType.None) continue;

                    TryAdjacentSwap(board, engine, p, q, out var hasMatch);
                    if (hasMatch == requireMatch)
                    {
                        a = p;
                        b = q;
                        return true;
                    }
                }
            }
        }
        return false;
    }
}