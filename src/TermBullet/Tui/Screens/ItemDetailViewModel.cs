using TermBullet.Application.Items;

namespace TermBullet.Tui.Screens;

public sealed class ItemDetailViewModel
{
    private ItemDetailViewModel(
        string publicRef,
        string itemKind,
        string content,
        string summaryTitle,
        IReadOnlyList<string> summaryLines,
        IReadOnlyList<string> contentLines,
        IReadOnlyList<string> historyLines)
    {
        PublicRef = publicRef;
        ItemKind = itemKind;
        Content = content;
        SummaryTitle = summaryTitle;
        SummaryLines = summaryLines;
        ContentLines = contentLines;
        HistoryLines = historyLines;
    }

    public string PublicRef { get; }

    public string ItemKind { get; }

    public string Content { get; }

    public string DetailTitle => $"{ItemKind} {PublicRef}";

    public string SummaryTitle { get; }

    public IReadOnlyList<string> SummaryLines { get; }

    public IReadOnlyList<string> ContentLines { get; }

    public IReadOnlyList<string> HistoryLines { get; }

    public static ItemDetailViewModel FromItem(
        ItemResult item,
        IReadOnlyCollection<ItemHistoryEntryResult>? history = null) =>
        FromRow(ItemDisplayRow.From(item), history);

    public static ItemDetailViewModel FromRow(
        ItemDisplayRow item,
        IReadOnlyCollection<ItemHistoryEntryResult>? history = null)
    {
        var itemKind = FormatKind(item.Type);
        return new ItemDetailViewModel(
            item.PublicRef,
            itemKind,
            item.Content,
            ResolveSummaryTitle(item.Type),
            BuildSummaryLines(item),
            BuildContentLines(item),
            BuildHistoryLines(history));
    }

    private static string ResolveSummaryTitle(string type) =>
        type.ToLowerInvariant() switch
        {
            "note" => "Info",
            "event" => "Schedule",
            _ => "Planning"
        };

    private static string[] BuildSummaryLines(ItemDisplayRow item)
    {
        var lines = new List<string>
        {
            $"status: {item.Status}"
        };

        switch (item.Type.ToLowerInvariant())
        {
            case "task":
                lines.Add($"collection: {item.Collection}");
                lines.Add($"priority: {item.Priority}");
                AddTag(lines, item.Tag);
                break;
            case "event":
                if (item.ScheduledAt is not null)
                {
                    lines.Add($"scheduled: {FormatDate(item.ScheduledAt.Value)}");
                }

                AddTag(lines, item.Tag);
                break;
            default:
                AddTag(lines, item.Tag);
                lines.Add($"updated: {FormatInstant(item.UpdatedAt)}");
                break;
        }

        return [.. lines];
    }

    private static string[] BuildContentLines(ItemDisplayRow item)
    {
        var lines = new List<string>
        {
            $"title: {item.Content}",
            " ",
            "description:"
        };

        lines.Add(string.IsNullOrWhiteSpace(item.Description) ? "-" : item.Description);
        return [.. lines];
    }

    private static void AddTag(List<string> lines, string tag)
    {
        if (!string.IsNullOrWhiteSpace(tag))
        {
            lines.Add($"tag: {tag}");
        }
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

    private static string FormatDate(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("yyyy-MM-dd");

    private static string FormatKind(string type) =>
        type.ToLowerInvariant() switch
        {
            "note" => "Note",
            "event" => "Event",
            _ => "Task"
        };
}
