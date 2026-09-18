namespace GodotTemplate.Features.Match3;

/// <summary>
/// Pure-C# score / moves / target tracker. No Godot dependency.
///
/// Scoring:
///   - Each gem cleared = <see cref="MatchEngine.PointsPerGem"/> (10) base points.
///   - Each T/L intersection = <see cref="MatchEngine.IntersectionBonus"/> (5) bonus.
///   - Cascade multiplier: level 1 × 1.0, level 2 × 1.5, level 3 × 2.0, level 4 × 2.5, ...
///     (multiplier = 1.0 + 0.5 * (cascadeLevel - 1))
/// </summary>
public sealed class ScoreManager
{
    public int Score { get; private set; }
    public int Moves { get; private set; }
    public int MovesMax { get; private set; }
    public int Target { get; private set; }
    public int CurrentLevel { get; private set; }

    /// <summary> Fires with the new score when <see cref="Score"/> changes. </summary>
    public event Action<int>? ScoreChanged;
    /// <summary> Fires with the new remaining move count when <see cref="Moves"/> changes. </summary>
    public event Action<int>? MovesChanged;

    /// <summary> Reset state for a new level. </summary>
    /// <param name="level">0-indexed level number</param>
    /// <param name="movesMax">Moves allotted for this level (e.g. 20)</param>
    /// <param name="target">Score to reach to win the level</param>
    public void Reset(int level, int movesMax, int target)
    {
        CurrentLevel = level;
        MovesMax = movesMax;
        Target = target;
        Score = 0;
        Moves = movesMax;
        ScoreChanged?.Invoke(Score);
        MovesChanged?.Invoke(Moves);
    }

    /// <summary>
    /// Award points from one cascade level. Cascade multiplier is applied internally.
    /// Mutates <see cref="Score"/> and fires <see cref="ScoreChanged"/>.
    /// </summary>
    public void AwardCascade(MatchEngine.CascadeResult cascade)
    {
        float multiplier = 1.0f + (cascade.CascadeLevel - 1) * 0.5f;
        int points = (int)(cascade.TotalPoints * multiplier);
        Score += points;
        ScoreChanged?.Invoke(Score);
    }

    /// <summary> Decrement remaining moves. Returns false if already 0 (no decrement). </summary>
    public bool UseMove()
    {
        if (Moves <= 0) return false;
        Moves--;
        MovesChanged?.Invoke(Moves);
        return true;
    }

    public bool IsWon() => Score >= Target;
    public bool IsOutOfMoves() => Moves <= 0;
}