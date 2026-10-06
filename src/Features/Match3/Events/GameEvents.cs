using GameFramework;

namespace GodotTemplate.Features.Match3.Events;

/// <summary>
/// Published by <see cref="GameState"/> when the game phase transitions.
/// Consumers subscribe via <c>EventBus.Instance.Subscribe<GamePhaseChanged>(...)</c>.
/// </summary>
/// <param name="NewPhase">The phase just entered.</param>
/// <param name="OldPhase">The phase before the transition, or <c>null</c> if
/// this is the first transition (initial Title phase is set in the constructor
/// without emitting).</param>
public readonly record struct GamePhaseChanged(GamePhase NewPhase, GamePhase? OldPhase);

/// <summary>
/// Published by <see cref="ScoreManager"/> when the player's score changes.
/// Consumers subscribe via <c>EventBus.Instance.Subscribe<ScoreChanged>(...)</c>.
/// </summary>
/// <param name="NewScore">The new total score after the change.</param>
/// <param name="Delta">The points added to the score. May be 0 on
/// <c>Reset</c> (which sets score to 0 — emitted for UI initial sync).</param>
public readonly record struct ScoreChanged(int NewScore, int Delta);

/// <summary>
/// Published by <see cref="ScoreManager"/> when the remaining move count changes.
/// Consumers subscribe via <c>EventBus.Instance.Subscribe<MovesChanged>(...)</c>.
/// </summary>
/// <param name="Remaining">The new remaining move count after the change.</param>
public readonly record struct MovesChanged(int Remaining);
