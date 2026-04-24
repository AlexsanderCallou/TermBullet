using TermBullet.Application.Items;
using TermBullet.Core.Items;

namespace TermBullet.Tui.Screens;

public sealed class WeeklyPlanningViewModel
{
    public WeeklyPlanningViewModel(IReadOnlyCollection<ItemResult> weekItems, IReadOnlyCollection<ItemResult> backlogItems)
    {
        WeekItems = weekItems.Select(MapToRow).ToList();
        BacklogItems = backlogItems.Select(MapToRow).Take(5).ToList();
        Buckets =
        [
            $"> Must ({WeekItems.Count(item => item.Priority == "high")})",
            $"  Should ({WeekItems.Count(item => item.Priority == "medium")})",
            $"  Could ({WeekItems.Count(item => item.Priority is "low" or "none")})",
            $"  Events ({WeekItems.Count(item => item.Type == "event")})"
        ];
        Metrics =
        [
            $"open: {weekItems.Count(item => item.Status == ItemStatus.Open)}",
            $"in progress: {weekItems.Count(item => item.Status == ItemStatus.InProgress)}",
            $"done: {weekItems.Count(item => item.Status == ItemStatus.Done)}",
            $"events: {weekItems.Count(item => item.Type == ItemType.Event)}"
        ];
        Notes =
        [
            WeekItems.Count > 0 ? $"weekly focus: {WeekItems[0].Content}" : "weekly focus: define first priority",
            BacklogItems.Count > 0 ? $"risk: {BacklogItems[0].Content}" : "risk: backlog still untriaged",
            "suggestion: close high priority work before broad refactor"
        ];
    }

    public IReadOnlyList<string> Buckets { get; }

    public IReadOnlyList<ItemDisplayRow> WeekItems { get; }

    public IReadOnlyList<string> Metrics { get; }

    public IReadOnlyList<ItemDisplayRow> BacklogItems { get; }

    public IReadOnlyList<string> Notes { get; }

    private static ItemDisplayRow MapToRow(ItemResult item) =>
        new()
        {
            PublicRef = item.PublicRef,
            Symbol = item.Type switch
            {
                ItemType.Note => "(.)",
                ItemType.Event => "(o)",
                _ => item.Status switch
                {
                    ItemStatus.Open => "[ ]",
                    ItemStatus.InProgress => "[~]",
                    ItemStatus.Done => "[x]",
                    ItemStatus.Cancelled => "[-]",
                    ItemStatus.Migrated => "[>]",
                    _ => "[ ]"
                }
            },
            Type = item.Type.ToString().ToLowerInvariant(),
            Status = item.Status.ToString().ToLowerInvariant(),
            Content = item.Content,
            Priority = item.Priority.ToString().ToLowerInvariant(),
            Collection = item.Collection.ToString().ToLowerInvariant(),
            Tags = [.. item.Tags]
        };
}

