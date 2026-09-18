namespace GodotTemplate.Features.Match3;

/// <summary>
/// Match-3 game feature entry point.
///
/// Currently a stub — the algorithm layer (Board, MatchEngine, GameState,
/// ScoreManager) lives in pure .NET 9 and has no engine dependency.
///
/// When the Godot 4.7 Mono toolchain is wired into the project, this file will
/// be expanded into a partial class that hosts the feature, decorated with the
/// framework's feature attribute, and wired to instantiate the algorithm classes.
/// </summary>
public class Match3Feature
{
    /// <summary> Algorithm-layer status string. Used for diagnostics / logging. </summary>
    public string Status => "stub — algorithm layer only, awaiting Godot integration";

    /// <summary> Feature full name, useful for log lines. </summary>
    public string Name => $"{nameof(GodotTemplate)}.{nameof(Match3)}.{nameof(Match3Feature)}";
}