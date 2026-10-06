using System;
#if !DISABLE_EVENTBUS
using GameFramework;
using GodotTemplate.Features.Match3.Events;
#endif

namespace GodotTemplate.Features.Match3;

/// <summary>
/// Per-level score + move counter. One per level. Mutates <see cref="Score"/>
/// via <see cref="AwardCascade"/> (idempotent on zero cascades), decrements
/// <see cref="Moves"/> via <see cref="UseMove"/> (rejects at zero).
///
/// Cascade multiplier: level 1 × 1.0, level 2 × 1.5, level 3 × 2.0, level 4 × 2.5, ...
/// (multiplier = 1.0 + 0.5 * (cascadeLevel - 1))
///
/// Migration note (W2): score/move changes also flow through GameFramework.EventBus
/// (<see cref="ScoreChanged"/> / <see cref="MovesChanged"/>). Old events are kept with
/// <c>[Obsolete]</c> for back-compat. New consumers should subscribe via
/// <c>EventBus.Instance.Subscribe<...></c>.
///
/// DISABLE_EVENTBUS: when defined (e.g. in the standalone Match3Tests console
/// project), the EventBus publish is skipped and the GameFramework/Events
/// imports are omitted — keeps the test csproj pure .NET 9.
/// </summary>
public sealed class ScoreManager
{
    public int Score { get; private set; }
    public int Moves { get; private set; }
    public int MovesMax { get; private set; }
    public int Target { get; private set; }
    public int CurrentLevel { get; private set; }

    /// <summary> Fires with the new score when <see cref="Score"/> changes. </summary>
    [Obsolete("Subscribe to ScoreChanged via EventBus.Instance.Subscribe<T> instead. Event will be removed in v1.1.")]
    public event Action<int>? ScoreChanged;
    /// <summary> Fires with the new remaining move count when <see cref="Moves"/> changes. </summary>
    [Obsolete("Subscribe to MovesChanged via EventBus.Instance.Subscribe<T> instead. Event will be removed in v1.1.")]
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
#pragma warning disable CS0618 // 'ScoreChanged/MovesChanged' are obsolete but still fired for back-compat
        ScoreChanged?.Invoke(Score);
        MovesChanged?.Invoke(Moves);
#pragma warning restore CS0618
#if !DISABLE_EVENTBUS
        // Canonical event path: EventBus. New consumers should use this.
        EventBus.Instance.Publish(new ScoreChanged(Score, Delta: 0));
        EventBus.Instance.Publish(new MovesChanged(Moves));
#endif
    }

    /// <summary>
    /// Award points from one cascade level. Cascade multiplier is applied internally.
    /// Mutates <see cref="Score"/> and fires score-changed events.
    /// </summary>
    public void AwardCascade(MatchEngine.CascadeResult cascade)
    {
        float multiplier = 1.0f + (cascade.CascadeLevel - 1) * 0.5f;
        int points = (int)(cascade.TotalPoints * multiplier);
        Score += points;
#pragma warning disable CS0618
        ScoreChanged?.Invoke(Score);
#pragma warning restore CS0618
#if !DISABLE_EVENTBUS
        EventBus.Instance.Publish(new ScoreChanged(Score, Delta: points));
#endif
    }

    /// <summary> Decrement remaining moves. Returns false if already 0 (no decrement). </summary>
    public bool UseMove()
    {
        if (Moves <= 0) return false;
        Moves--;
#pragma warning disable CS0618
        MovesChanged?.Invoke(Moves);
#pragma warning restore CS0618
#if !DISABLE_EVENTBUS
        EventBus.Instance.Publish(new MovesChanged(Moves));
#endif
        return true;
    }

    public bool IsWon() => Score >= Target;
    public bool IsOutOfMoves() => Moves <= 0;
}
