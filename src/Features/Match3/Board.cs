namespace GodotTemplate.Features.Match3;

/// <summary>
/// Match-3 gem types. Values 0..5 are valid colors; None is the empty-cell sentinel.
/// Index order is hardcoded for sprite/texture lookup — do NOT reorder.
/// </summary>
public enum GemType : byte
{
    None  = 255,
    Red   = 0,
    Orange = 1,
    Yellow = 2,
    Green = 3,
    Blue  = 4,
    Purple = 5,
}

/// <summary> Constants and helpers for the 6 gem colors. </summary>
public static class GemTypes
{
    public const int Count = 6;
    public static readonly GemType[] AllColors =
    {
        GemType.Red, GemType.Orange, GemType.Yellow,
        GemType.Green, GemType.Blue, GemType.Purple,
    };
}

/// <summary>
/// 2D grid coordinate. Top-left origin: (0,0). Use BOARD bounds via <see cref="Board.InBounds"/>.
/// </summary>
public readonly record struct CellPos(int X, int Y)
{
    public CellPos Offset(int dx, int dy) => new(X + dx, Y + dy);
    // record struct auto-generates == / != / Equals / GetHashCode — no manual defs.
}


/// <summary>
/// 8×8 mutable board. Owns the cell grid and provides swap / gravity / spawn operations.
/// All mutating operations assume the caller has validated inputs (use InBounds).
///
/// Thread-safety: NOT thread-safe. All mutations must come from the game thread.
/// </summary>
public sealed class Board
{
    public const int Width = 8;
    public const int Height = 8;
    public const int CellCount = Width * Height;

    private readonly GemType[,] _cells = new GemType[Width, Height];
    private readonly Random _rng;

    public Board(int seed = 0)
    {
        _rng = seed == 0 ? new Random() : new Random(seed);
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _cells[x, y] = GemType.None;
    }

    public GemType this[int x, int y]
    {
        get => _cells[x, y];
        set => _cells[x, y] = value;
    }

    public GemType this[CellPos p]
    {
        get => _cells[p.X, p.Y];
        set => _cells[p.X, p.Y] = value;
    }

    public bool InBounds(int x, int y) =>
        x >= 0 && x < Width && y >= 0 && y < Height;

    public bool InBounds(CellPos p) => InBounds(p.X, p.Y);

    /// <summary> Exchange cell contents. Caller must ensure both positions are in-bounds. </summary>
    public void Swap(CellPos a, CellPos b)
    {
        if (!InBounds(a) || !InBounds(b))
            throw new ArgumentException($"Swap out of bounds: {a} {b}");
        (_cells[a.X, a.Y], _cells[b.X, b.Y]) = (_cells[b.X, b.Y], _cells[a.X, a.Y]);
    }

    /// <summary> Returns true iff <paramref name="a"/> and <paramref name="b"/> are orthogonally adjacent. </summary>
    public static bool IsAdjacent(CellPos a, CellPos b)
    {
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    /// <summary>
    /// Fill all 64 cells with random colors such that no opening 3-match exists.
    /// Strategy: for each cell (left-to-right, top-to-bottom), pick the first shuffled
    /// color that does NOT create a 2-of-the-same-color chain to the LEFT or UP.
    /// This guarantees no 3-match at any point during construction.
    /// </summary>
    public void FillRandomNoOpeningMatch()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _cells[x, y] = PickTypeNoOpeningMatch(x, y);
            }
        }
    }

    private GemType PickTypeNoOpeningMatch(int x, int y)
    {
        // Build a shuffled list of the 6 colors (Fisher–Yates using the board's RNG).
        Span<GemType> candidates = stackalloc GemType[GemTypes.Count];
        for (int i = 0; i < GemTypes.Count; i++) candidates[i] = GemTypes.AllColors[i];
        for (int i = candidates.Length - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }
        foreach (var t in candidates)
        {
            if (!WouldCreateOpeningMatch(x, y, t)) return t;
        }
        // Theoretically impossible on an 8×8 with only left+up constraints (max 5 colors excluded,
        // leaving at least 1). Fall back to first candidate.
        return candidates[0];
    }

    /// <summary>
    /// Would placing <paramref name="t"/> at (x,y) create a 3-match using already-placed cells?
    /// Only LEFT (x-1, x-2) and UP (y-1, y-2) neighbors exist at construction time.
    /// </summary>
    private bool WouldCreateOpeningMatch(int x, int y, GemType t)
    {
        if (x >= 2 && _cells[x - 1, y] == t && _cells[x - 2, y] == t) return true;
        if (y >= 2 && _cells[x, y - 1] == t && _cells[x, y - 2] == t) return true;
        return false;
    }

    /// <summary>
    /// Drop non-empty gems down each column to fill empty cells. Stable: relative order preserved.
    /// </summary>
    public void ApplyGravity()
    {
        for (int x = 0; x < Width; x++)
        {
            int writeY = Height - 1;
            for (int y = Height - 1; y >= 0; y--)
            {
                if (_cells[x, y] == GemType.None) continue;
                if (writeY != y)
                {
                    _cells[x, writeY] = _cells[x, y];
                    _cells[x, y] = GemType.None;
                }
                writeY--;
            }
            // Empty cells above writeY remain None.
        }
    }

    /// <summary> Fill all empty cells with fresh random colors (used after gravity). </summary>
    public void SpawnNewGems()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_cells[x, y] == GemType.None)
                    _cells[x, y] = GemTypes.AllColors[_rng.Next(GemTypes.Count)];
            }
        }
    }

    /// <summary> Deep copy of the cell grid. </summary>
    public GemType[,] CloneGrid()
    {
        var copy = new GemType[Width, Height];
        Array.Copy(_cells, copy, _cells.Length);
        return copy;
    }

    /// <summary> Replace internal grid with the provided 2D array (must be 8×8). </summary>
    public void LoadGrid(GemType[,] grid)
    {
        if (grid.GetLength(0) != Width || grid.GetLength(1) != Height)
            throw new ArgumentException($"LoadGrid size mismatch: {grid.GetLength(0)}×{grid.GetLength(1)}");
        Array.Copy(grid, _cells, _cells.Length);
    }

    /// <summary> ASCII rendering for debug / tests. 'R'/'O'/'Y'/'G'/'B'/'P' or '.' for empty. </summary>
    public string ToAscii()
    {
        var sb = new System.Text.StringBuilder();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                char c = _cells[x, y] switch
                {
                    GemType.None   => '.',
                    GemType.Red    => 'R',
                    GemType.Orange => 'O',
                    GemType.Yellow => 'Y',
                    GemType.Green  => 'G',
                    GemType.Blue   => 'B',
                    GemType.Purple => 'P',
                    _              => '?',
                };
                sb.Append(c);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}