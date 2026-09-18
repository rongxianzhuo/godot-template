using Godot;

namespace GodotTemplate.Features.Match3;

/// <summary>
/// Renders the 8×8 <see cref="Board"/> as a GridContainer of TextureRects and
/// handles click+drag (adjacent-cell swipe) input. Pure presentation layer — all
/// game logic lives in the algorithm classes (Board, MatchEngine, ScoreManager).
///
/// The view raises a <see cref="MoveCommitted"/> event after each legal+matched
/// swap so the feature can run cascades, update the ScoreManager, and check
/// win/lose conditions.
/// </summary>
public partial class Match3BoardView : GridContainer
{
    public delegate void MoveCommittedHandler(int pointsGained, int cellsCleared, bool outOfMoves);
    public event MoveCommittedHandler? MoveCommitted;

    private Board? _board;
    private MatchEngine? _engine;
    private ScoreManager? _scores;
    private TextureRect[,] _cells = new TextureRect[Board.Width, Board.Height];

    // Click+swipe state
    private CellPos _pressStart;
    private bool _isPressed;

    /// <summary> Bind the view to the algorithm-layer objects and refresh display. </summary>
    public void Bind(Board board, MatchEngine engine, ScoreManager scores)
    {
        _board = board;
        _engine = engine;
        _scores = scores;
        BuildCells();
        Refresh();
    }

    public override void _Ready()
    {
        Columns = Board.Width;
        CustomMinimumSize = new Vector2(8 * 64, 8 * 64); // 64 px per cell default
        SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        SizeFlagsVertical = SizeFlags.ShrinkCenter;
        // Capture input even though cells are TextureRects (not Buttons).
        MouseFilter = MouseFilterEnum.Stop;
    }

    private void BuildCells()
    {
        for (int y = 0; y < Board.Height; y++)
        {
            for (int x = 0; x < Board.Width; x++)
            {
                var cell = new TextureRect
                {
                    Name = $"Cell_{x}_{y}",
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                    StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                    CustomMinimumSize = new Vector2(64, 64),
                };
                _cells[x, y] = cell;
                AddChild(cell);
            }
        }
    }

    /// <summary> Re-read board state and update every cell's sprite. </summary>
    public void Refresh()
    {
        if (_board is null) return;
        for (int y = 0; y < Board.Height; y++)
        {
            for (int x = 0; x < Board.Width; x++)
            {
                _cells[x, y].Texture = SpritePaths.LoadGem(_board[x, y]);
            }
        }
    }

    // ---- Input: click + drag (adjacent-cell swipe) --------------------------------------
    public override void _GuiInput(InputEvent @event)
    {
        if (_board is null || _engine is null) return;

        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            if (mb.Pressed)
            {
                if (LocalToCell(mb.Position) is CellPos pos)
                {
                    _pressStart = pos;
                    _isPressed = true;
                }
            }
            else if (_isPressed)
            {
                _isPressed = false;
                if (LocalToCell(mb.Position) is CellPos endPos
                    && endPos != _pressStart
                    && Board.IsAdjacent(_pressStart, endPos))
                {
                    CommitSwap(_pressStart, endPos);
                }
            }
            AcceptEvent();
        }
    }

    private void CommitSwap(CellPos a, CellPos b)
    {
        if (_board is null || _engine is null || _scores is null) return;

        var result = _engine.TrySwap(_board, a, b);
        if (!result.IsLegal || !result.HasMatch)
        {
            GD.Print($"[Match3] Illegal swap ({a} <-> {b}) — no match.");
            return;
        }

        // Apply score (sum across cascades)
        int gained = 0;
        int cleared = 0;
        foreach (var c in result.Cascades)
        {
            _scores.AwardCascade(c);
            gained += c.TotalPoints;
            cleared += c.CellsCleared.Count;
        }
        _scores.UseMove();

        bool oom = _scores.IsOutOfMoves();
        GD.Print($"[Match3] Swap {a}<->{b}: +{gained} pts, {cleared} cells cleared. Moves left: {_scores.Moves}");

        Refresh();
        MoveCommitted?.Invoke(gained, cleared, oom);
    }

    private CellPos? LocalToCell(Vector2 local)
    {
        var size = Size;
        if (size.X <= 0 || size.Y <= 0) return null;
        float cellW = size.X / Board.Width;
        float cellH = size.Y / Board.Height;
        int x = (int)(local.X / cellW);
        int y = (int)(local.Y / cellH);
        if (x < 0 || x >= Board.Width || y < 0 || y >= Board.Height) return null;
        return new CellPos(x, y);
    }
}