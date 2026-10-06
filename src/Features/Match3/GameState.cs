using System;
#if !DISABLE_EVENTBUS
using GameFramework;
using GodotTemplate.Features.Match3.Events;
#endif

namespace GodotTemplate.Features.Match3;

/// <summary> High-level game phase. Drives UI (Title / Playing / Won / GameOver screens). </summary>
public enum GamePhase
{
    Title,
    Playing,
    Won,
    GameOver,
}

/// <summary>
/// Pure-C# state machine + level pointer. No Godot dependency.
/// Emits <see cref="PhaseChanged"/> when transitioning. Transitions are idempotent within
/// a phase (calling StartLevel while Playing will still emit the new Playing phase, but
/// callers should guard via <see cref="Phase"/> checks).
///
/// Migration note (W2): events also flow through GameFramework.EventBus
/// (<see cref="GamePhaseChanged"/>). Old <see cref="PhaseChanged"/> is kept with
/// <c>[Obsolete]</c> for back-compat. New consumers should subscribe via
/// <c>EventBus.Instance.Subscribe<GamePhaseChanged>(...)</c>.
///
/// DISABLE_EVENTBUS: when defined (e.g. in the standalone Match3Tests console
/// project), the EventBus publish is skipped and the GameFramework/Events
/// imports are omitted — keeps the test csproj pure .NET 9.
/// </summary>
public sealed class GameState
{
    public GamePhase Phase { get; private set; } = GamePhase.Title;
    public int CurrentLevel { get; private set; } = 0;

    /// <summary>
    /// Fires after the phase changes. New phase is the argument.
    /// Kept for backward compatibility (fork consumers may still subscribe).
    /// New code should subscribe to <see cref="GamePhaseChanged"/> via
    /// <c>EventBus.Instance.Subscribe<GamePhaseChanged></c>.
    /// </summary>
    [Obsolete("Subscribe to GamePhaseChanged via EventBus.Instance.Subscribe<T> instead. Event will be removed in v1.1.")]
    public event Action<GamePhase>? PhaseChanged;

    /// <summary> Begin playing the given level (0-indexed). Transitions Title/GameOver → Playing. </summary>
    public void StartLevel(int level)
    {
        CurrentLevel = level;
        Transition(GamePhase.Playing);
    }

    /// <summary> Mark the current level as won. Only valid from Playing. </summary>
    public void NotifyWin()
    {
        if (Phase == GamePhase.Playing)
            Transition(GamePhase.Won);
    }

    /// <summary> Mark the current level as failed (out of moves or no legal swap). Only from Playing. </summary>
    public void NotifyGameOver()
    {
        if (Phase == GamePhase.Playing)
            Transition(GamePhase.GameOver);
    }

    /// <summary> Return to title screen from any phase. </summary>
    public void ReturnToTitle()
    {
        Transition(GamePhase.Title);
    }

    /// <summary> Advance to the next level (only valid from Won; sets CurrentLevel += 1). </summary>
    public void AdvanceLevel()
    {
        if (Phase != GamePhase.Won) return;
        CurrentLevel++;
        Transition(GamePhase.Playing);
    }

    private void Transition(GamePhase newPhase)
    {
        var old = Phase;
        Phase = newPhase;
#pragma warning disable CS0618 // 'PhaseChanged' is obsolete but still fired for back-compat
        PhaseChanged?.Invoke(newPhase);
#pragma warning restore CS0618
#if !DISABLE_EVENTBUS
        // Canonical event path: EventBus. New consumers should use this.
        EventBus.Instance.Publish(new GamePhaseChanged(newPhase, old));
#endif
    }
}
