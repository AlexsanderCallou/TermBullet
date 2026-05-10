namespace TermBullet.Tui.Screens;

public sealed class TagSummaryRow
{
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required bool IsCataloged { get; init; }
    public required int UsageCount { get; init; }
    public required int ActiveTaskCount { get; init; }
    public required int NoteCount { get; init; }
    public required int EventCount { get; init; }
    public required DateTimeOffset LastUsed { get; init; }
}
