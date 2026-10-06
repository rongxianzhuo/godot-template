namespace GodotTemplate.Features.Match3.Screens;

/// <summary>
/// Argument passed into <see cref="GameScreen"/> on open. Carries the
/// level/difficulty so GameScreen can initialise the algorithm layer.
///
/// Defined as a <c>readonly record struct</c> (per AGENTS.md EventBus
/// migration recipe) — immutable, value-typed, default-constructable
/// so the framework's typed-TCS machinery works without surprises.
/// </summary>
public readonly record struct GameStartArgs(int Level);

/// <summary>
/// Result returned from <see cref="GameScreen"/> on close. Carries the
/// final game state so <see cref="EndScreen"/> can render the headline
/// + score breakdown, and so Match3Feature can decide what to do next.
///
/// <see cref="MovesUsed"/> is included so EndScreen can show "12 of 20
/// moves used" — useful context for the "out of moves" lose state.
/// </summary>
public readonly record struct GameResult(bool Won, int Score, int MovesUsed, int MovesMax, int Level);

/// <summary>
/// Argument passed into <see cref="EndScreen"/> on open. Carries the
/// outcome from GameScreen so EndScreen can render the right headline
/// ("You Won!" green vs "Game Over" red) and the final score.
///
/// <para>
/// v1.2 polish #2: also carries <see cref="Level"/> so <see cref="EndScreen"/>
/// can apply the same per-theme font as the <see cref="GameScreen"/> that
/// just closed (e.g. level 25 → Desert theme on both Game + End screens).
/// </para>
/// </summary>
public readonly record struct EndScreenArgs(bool Won, int FinalScore, int MovesUsed, int MovesMax, int Level);

/// <summary>
/// Choice returned from <see cref="EndScreen"/> on close. Indicates
/// whether the player wants to retry the same level or go back to
/// the title screen.
///
/// Quit-from-end-screen is intentionally NOT an option here — the only
/// way to exit the app is via the title screen's Quit button. This
/// matches v1.0 ship behaviour (no Quit on EndScreen).
/// </summary>
public enum EndChoice
{
    /// <summary>Tap "Play Again" — restart the same level (Title->Game->End loop).</summary>
    PlayAgain,
    /// <summary>Tap "Main Menu" — back to Title.</summary>
    MainMenu,
}
