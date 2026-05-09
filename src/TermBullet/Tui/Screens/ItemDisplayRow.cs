using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public sealed class ItemDisplayRow
{
    public required Guid Id { get; init; }
    public required string PublicRef { get; init; }
    public required string Symbol { get; init; }
    public required string Type { get; init; }
    public required string Status { get; init; }
    public required string Content { get; init; }
    public required string? Description { get; init; }
    public required string Priority { get; init; }
    public required string Collection { get; init; }
    public required string[] Tags { get; init; }
    public required DateOnly? PlannedFor { get; init; }
    public required DateTimeOffset? ScheduledAt { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }

    public static ItemDisplayRow From(ItemResult item) =>
        new()
        {
            Id = item.Id,
            PublicRef = item.PublicRef,
            Symbol = ResolveSymbol(item.Type, item.Status),
            Type = item.Type.ToString().ToLowerInvariant(),
            Status = item.Status.ToString().ToLowerInvariant(),
            Content = item.Content,
            Description = item.Description,
            Priority = item.Priority.ToString().ToLowerInvariant(),
            Collection = item.Collection.ToString().ToLowerInvariant(),
            Tags = [.. item.Tags],
            PlannedFor = item.PlannedFor,
            ScheduledAt = item.ScheduledAt,
            Version = item.Version,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };

    private static string ResolveSymbol(ItemType type, ItemStatus status) =>
        type switch
        {
            ItemType.Note => "(.)",
            ItemType.Event => "(o)",
            _ => status switch
            {
                ItemStatus.Open => "[ ]",
                ItemStatus.Done => "[x]",
                ItemStatus.Cancelled => "[-]",
                ItemStatus.Migrate => "[>]",
                _ => "[ ]"
            }
        };
}
