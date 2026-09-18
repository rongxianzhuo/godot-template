namespace GodotTemplate.Features.Match3;

/// <summary>
/// Match-3 detection + cascade resolver. Stateless — every method takes a <see cref="Board"/>.
///
/// Algorithm:
///   1. Scan all rows for horizontal runs of ≥3 same-color cells (excluding None).
///   2. Scan all columns for vertical runs of ≥3 same-color cells.
///   3. Each run is a MatchSegment. A cell appearing in both an H and V segment is a
///      "T/L intersection" — the segment records IntersectionCount for bonus scoring.
///   4. Cascade: clear all cells covered by any segment → gravity → spawn → repeat
///      until no more segments exist.
/// </summary>
public sealed class MatchEngine
{
    public const int PointsPerGem = 10;
    public const int IntersectionBonus = 5;

    public enum Direction { Horizontal, Vertical }

    /// <summary> A contiguous run of ≥3 same-color cells in one direction. </summary>
    public sealed class MatchSegment
    {
        public Direction Dir { get; init; }
        public GemType Type { get; init; }
        public List<CellPos> Cells { get; init; } = new();
        /// <summary> Number of cells in this segment that are also part of a perpendicular segment. </summary>
        public int IntersectionCount { get; set; }
        public int Length => Cells.Count;
    }

    /// <summary> All segments and cleared cells from one cascade level. </summary>
    public sealed class CascadeResult
    {
        public int CascadeLevel { get; init; }   // 1 = first clear, 2 = after first gravity, ...
        public List<CellPos> CellsCleared { get; } = new();
        public List<MatchSegment> Segments { get; } = new();
        public int TotalBasePoints => CellsCleared.Count * PointsPerGem;
        public int TotalBonusPoints =>
            Segments.Sum(s => s.IntersectionCount) * IntersectionBonus;
        public int TotalPoints => TotalBasePoints + TotalBonusPoints;
    }

    /// <summary> Result of attempting a swap: legal or not, and any cascades produced. </summary>
    public sealed class SwapResult
    {
        public bool IsLegal { get; init; }       // the two cells were adjacent
        public bool HasMatch { get; init; }      // the swap created ≥1 segment
        public List<CascadeResult> Cascades { get; init; } = new();
    }

    /// <summary> Find all ≥3 segments on the current board (read-only — does NOT mutate). </summary>
    public List<MatchSegment> FindAllSegments(Board board)
    {
        var segments = new List<MatchSegment>();
        FindHorizontalSegments(board, segments);
        FindVerticalSegments(board, segments);
        ComputeIntersections(segments);
        return segments;
    }

    private void FindHorizontalSegments(Board board, List<MatchSegment> outSegments)
    {
        for (int y = 0; y < Board.Height; y++)
        {
            int x = 0;
            while (x < Board.Width)
            {
                var t = board[x, y];
                if (t == GemType.None) { x++; continue; }
                int runStart = x;
                while (x + 1 < Board.Width && board[x + 1, y] == t) x++;
                int len = x - runStart + 1;
                if (len >= 3)
                {
                    var seg = new MatchSegment
                    {
                        Dir = Direction.Horizontal,
                        Type = t,
                    };
                    for (int i = runStart; i <= x; i++)
                        seg.Cells.Add(new CellPos(i, y));
                    outSegments.Add(seg);
                }
                x++;
            }
        }
    }

    private void FindVerticalSegments(Board board, List<MatchSegment> outSegments)
    {
        for (int x = 0; x < Board.Width; x++)
        {
            int y = 0;
            while (y < Board.Height)
            {
                var t = board[x, y];
                if (t == GemType.None) { y++; continue; }
                int runStart = y;
                while (y + 1 < Board.Height && board[x, y + 1] == t) y++;
                int len = y - runStart + 1;
                if (len >= 3)
                {
                    var seg = new MatchSegment
                    {
                        Dir = Direction.Vertical,
                        Type = t,
                    };
                    for (int i = runStart; i <= y; i++)
                        seg.Cells.Add(new CellPos(x, i));
                    outSegments.Add(seg);
                }
                y++;
            }
        }
    }

    /// <summary>
    /// For each cell appearing in ≥2 segments of different directions, increment
    /// IntersectionCount on each of those segments.
    /// </summary>
    private static void ComputeIntersections(List<MatchSegment> segments)
    {
        if (segments.Count < 2) return;
        var cellToSegments = new Dictionary<CellPos, List<MatchSegment>>();
        foreach (var s in segments)
        {
            foreach (var c in s.Cells)
            {
                if (!cellToSegments.TryGetValue(c, out var list))
                    cellToSegments[c] = list = new List<MatchSegment>();
                list.Add(s);
            }
        }
        foreach (var (_, segs) in cellToSegments)
        {
            if (segs.Count < 2) continue;
            // A cell in 2 segments of different directions is an intersection.
            bool hasH = segs.Any(s => s.Dir == Direction.Horizontal);
            bool hasV = segs.Any(s => s.Dir == Direction.Vertical);
            if (hasH && hasV)
                foreach (var s in segs) s.IntersectionCount++;
        }
    }

    /// <summary>
    /// Resolve all cascades: clear matches → gravity → spawn → repeat until stable.
    /// Mutates <paramref name="board"/>.
    /// </summary>
    public List<CascadeResult> ResolveCascade(Board board)
    {
        var results = new List<CascadeResult>();
        int level = 0;
        while (true)
        {
            var segments = FindAllSegments(board);
            if (segments.Count == 0) break;
            level++;
            var result = new CascadeResult { CascadeLevel = level };

            // Add cleared cells (dedupe across segments).
            var cleared = new HashSet<CellPos>();
            foreach (var s in segments)
            {
                result.Segments.Add(s);
                foreach (var c in s.Cells) cleared.Add(c);
            }
            result.CellsCleared.AddRange(cleared);

            // Clear.
            foreach (var c in cleared)
                board[c] = GemType.None;

            // Gravity + spawn.
            board.ApplyGravity();
            board.SpawnNewGems();

            results.Add(result);
        }
        return results;
    }

    /// <summary>
    /// Attempt <paramref name="a"/> ⇄ <paramref name="b"/>. Returns:
    ///   - IsLegal=false if positions are out-of-bounds or not orthogonal neighbors.
    ///   - IsLegal=true, HasMatch=false if the swap auto-undid (no match found).
    ///   - IsLegal=true, HasMatch=true if cascades were triggered (board is mutated).
    /// </summary>
    public SwapResult TrySwap(Board board, CellPos a, CellPos b)
    {
        if (!board.InBounds(a) || !board.InBounds(b))
            return new SwapResult { IsLegal = false };
        if (a == b || !Board.IsAdjacent(a, b))
            return new SwapResult { IsLegal = false };

        board.Swap(a, b);
        var segments = FindAllSegments(board);
        if (segments.Count == 0)
        {
            // Illegal: no match. Undo.
            board.Swap(a, b);
            return new SwapResult { IsLegal = true, HasMatch = false };
        }

        var cascades = ResolveCascade(board);
        return new SwapResult
        {
            IsLegal = true,
            HasMatch = true,
            Cascades = cascades,
        };
    }
}