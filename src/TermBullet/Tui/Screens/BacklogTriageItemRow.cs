namespace TermBullet.Tui.Screens;

public sealed class BacklogTriageItemRow
{
    public required string PublicRef { get; init; }
    public required string Symbol { get; init; }
    public required string Content { get; init; }
    public required string Priority { get; init; }
    public required string Collection { get; init; }
    public required string[] Tags { get; init; }
}
