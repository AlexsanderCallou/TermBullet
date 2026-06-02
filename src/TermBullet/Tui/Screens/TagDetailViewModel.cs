namespace TermBullet.Tui.Screens;

public sealed class TagDetailViewModel
{
    private TagDetailViewModel(
        string tag,
        IReadOnlyList<string> summaryLines,
        IReadOnlyList<string> timelineLines,
        IReadOnlyList<string> taskLines,
        IReadOnlyList<string> noteLines,
        IReadOnlyList<string> eventLines,
        IReadOnlyList<ItemDisplayRow> selectableItems)
    {
        Tag = tag;
        SummaryLines = summaryLines;
        TimelineLines = timelineLines;
        TaskLines = taskLines;
        NoteLines = noteLines;
        EventLines = eventLines;
        SelectableItems = selectableItems;
    }

    public string Tag { get; }

    public IReadOnlyList<string> SummaryLines { get; }

    public IReadOnlyList<string> TimelineLines { get; }

    public IReadOnlyList<string> TaskLines { get; }

    public IReadOnlyList<string> NoteLines { get; }

    public IReadOnlyList<string> EventLines { get; }

    public IReadOnlyList<ItemDisplayRow> SelectableItems { get; }

    public static TagDetailViewModel Build(string tag, IReadOnlyCollection<ItemDisplayRow> rows)
    {
        var items = rows
            .Where(item => string.Equals(item.Tag, tag, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Type)
            .ThenBy(item => item.Collection)
            .ThenBy(item => item.PublicRef, StringComparer.Ordinal)
            .ToArray();
        var tasks = items.Where(item => item.Type.Equals("task", StringComparison.OrdinalIgnoreCase)).ToArray();
        var notes = items.Where(item => item.Type.Equals("note", StringComparison.OrdinalIgnoreCase)).ToArray();
        var events = items.Where(item => item.Type.Equals("event", StringComparison.OrdinalIgnoreCase)).ToArray();

        return new TagDetailViewModel(
            tag,
            [
                $"name: {tag}",
                $"items: {items.Length}",
                $"tasks: {tasks.Length}",
                $"notes: {notes.Length}",
                $"events: {events.Length}"
            ],
            BuildTimeline(items),
            BuildTaskLines(tasks),
            BuildItemLines(notes),
            BuildItemLines(events),
            items);
    }

    private static string[] BuildTimeline(IReadOnlyCollection<ItemDisplayRow> items)
    {
        if (items.Count == 0)
        {
            return ["(no activity)"];
        }

        var oldest = items.Min(item => item.CreatedAt);
        var latest = items.Max(item => item.UpdatedAt);
        return
        [
            $"oldest: {oldest:yyyy-MM-dd}",
            $"latest: {latest:yyyy-MM-dd}",
            $"last activity: {items.OrderByDescending(item => item.UpdatedAt).First().PublicRef}"
        ];
    }

    private static string[] BuildTaskLines(IReadOnlyCollection<ItemDisplayRow> tasks)
    {
        if (tasks.Count == 0)
        {
            return ["(no tasks)"];
        }

        var lines = new List<string>();
        foreach (var collection in new[] { "today", "week", "month", "backlog" })
        {
            var collectionTasks = tasks
                .Where(item => item.Collection.Equals(collection, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.PublicRef, StringComparer.Ordinal)
                .ToArray();
            if (collectionTasks.Length == 0)
            {
                continue;
            }

            lines.Add(collection[..1].ToUpperInvariant() + collection[1..]);
            lines.AddRange(BuildItemLines(collectionTasks));
            lines.Add(" ");
        }

        return lines.Count > 0 ? [.. lines] : ["(no tasks)"];
    }

    private static string[] BuildItemLines(IEnumerable<ItemDisplayRow> items) =>
        [.. items.Select(item => $"{item.Symbol} {item.PublicRef} {item.Content}")];
}
