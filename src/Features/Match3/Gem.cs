namespace GodotTemplate.Features.Match3;

/// <summary>
/// Read-only handle to a single gem on the board. Mutations happen via <see cref="Board"/>.
/// Use this struct to pass gem identity through callbacks / events without exposing the
/// internal cell array.
/// </summary>
public readonly record struct Gem(GemType Type, CellPos Position)
{
    public bool IsEmpty => Type == GemType.None;
    public bool IsColor => Type != GemType.None;

    public static Gem Empty(CellPos pos) => new(GemType.None, pos);

    public override string ToString() => $"Gem({Type}, {Position})";
}