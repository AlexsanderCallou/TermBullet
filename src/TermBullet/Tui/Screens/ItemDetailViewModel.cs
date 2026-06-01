using TermBullet.Application.Items;

namespace TermBullet.Tui.Screens;

public sealed class ItemDetailViewModel
{
    private ItemDetailViewModel(
        string publicRef,
        string content,
        IReadOnlyList<string> identityLines,
        IReadOnlyList<string> planningLines,
        IReadOnlyList<string> contentLines,
        IReadOnlyList<string> migrationLines,
        IReadOnlyList<string> historyLines)
    {
        PublicRef = publicRef;
        Content = content;
        IdentityLines = identityLines;
        PlanningLines = planningLines;
        ContentLines = contentLines;
        MigrationLines = migrationLines;
        HistoryLines = historyLines;
    }

    public string PublicRef { get; }

    public string Content { get; }

    public IReadOnlyList<string> IdentityLines { get; }

    public IReadOnlyList<string> PlanningLines { get; }

    public IReadOnlyList<string> ContentLines { get; }

    public IReadOnlyList<string> MigrationLines { get; }

    public IReadOnlyList<string> HistoryLines { get; }

    public static ItemDetailViewModel FromItem(
        ItemResult item,
        IReadOnlyCollection<ItemHistoryEntryResult>? history = null) =>
        FromRow(ItemDisplayRow.From(item), history);

    public static ItemDetailViewModel FromRow(
        ItemDisplayRow item,
        IReadOnlyCollection<ItemHistoryEntryResult>? history = null)
    {
        var tags = item.Tags.Length > 0 ? string.Join(", ", item.Tags) : "-";
        var scheduledAt = item.ScheduledAt is null ? "-" : FormatInstant(item.ScheduledAt.Value);
        return new ItemDetailViewModel(
            item.PublicRef,
            item.Content,
            [
                $"ref: {item.PublicRef}",
                $"id: {item.Id}",
                $"type: {item.Type}",
                $"status: {item.Status}",
                $"version: {item.Version}",
                $"created: {FormatInstant(item.CreatedAt)}",
                $"updated: {FormatInstant(item.UpdatedAt)}"
            ],
            [
                $"collection: {item.Collection}",
                $"scheduled_at: {scheduledAt}",
                $"priority: {item.Priority}",
                $"tags: {tags}"
            ],
            BuildContentLines(item),
            [
                "migrate changes this task's collection in place",
                "id and public ref stay the same"
            ],
            BuildHistoryLines(history));
    }

    private static string[] BuildContentLines(ItemDisplayRow item)
    {
        var lines = new List<string>
        {
            item.Content,
            " ",
            "Description:"
        };
        lines.Add(string.IsNullOrWhiteSpace(item.Description) ? "-" : item.Description);
        return [.. lines];
    }

    private static string[] BuildHistoryLines(IReadOnlyCollection<ItemHistoryEntryResult>? history)
    {
        if (history is null || history.Count == 0)
        {
            return ["no history entries found"];
        }

        return
        [
            .. history
                .OrderBy(entry => entry.OccurredAt)
                .Select(entry => $"{FormatInstant(entry.OccurredAt)} {entry.Summary}")
        ];
    }

    private static string FormatInstant(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
}
