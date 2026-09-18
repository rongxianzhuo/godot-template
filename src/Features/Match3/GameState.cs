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
/// </summary>
public sealed class GameState
{
    public GamePhase Phase { get; private set; } = GamePhase.Title;
    public int CurrentLevel { get; private set; } = 0;

    /// <summary> Fires after the phase changes. New phase is the argument. </summary>
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
        PhaseChanged?.Invoke(newPhase);
    }
}