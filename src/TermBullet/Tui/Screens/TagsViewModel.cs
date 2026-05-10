using TermBullet.Application.Tags;

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

public sealed class TagsViewModel
{
    private TagsViewModel(IReadOnlyList<TagSummaryRow> tags)
    {
        Tags = tags;
    }

    public IReadOnlyList<TagSummaryRow> Tags { get; }

    public static TagsViewModel Build(
        IReadOnlyCollection<TagCatalogResult> catalogTags,
        IReadOnlyCollection<ItemDisplayRow> rows)
    {
        var usageByTag = rows
            .SelectMany(row => row.Tags.Select(tag => new { Tag = tag, Row = row }))
            .GroupBy(entry => entry.Tag, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Select(entry => entry.Row).ToArray(), StringComparer.OrdinalIgnoreCase);

        var names = catalogTags.Select(tag => tag.Name)
            .Concat(usageByTag.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var tags = names
            .Select(group =>
            {
                var catalogTag = catalogTags.FirstOrDefault(tag => string.Equals(tag.Name, group, StringComparison.OrdinalIgnoreCase));
                usageByTag.TryGetValue(group, out var items);
                items ??= [];
                return new TagSummaryRow
                {
                    Name = catalogTag?.Name ?? group,
                    Description = catalogTag?.Description,
                    IsCataloged = catalogTag is not null,
                    UsageCount = items.Length,
                    ActiveTaskCount = items.Count(item =>
                        item.Type.Equals("task", StringComparison.OrdinalIgnoreCase)
                        && item.Status.Equals("open", StringComparison.OrdinalIgnoreCase)),
                    NoteCount = items.Count(item => item.Type.Equals("note", StringComparison.OrdinalIgnoreCase)),
                    EventCount = items.Count(item => item.Type.Equals("event", StringComparison.OrdinalIgnoreCase)),
                    LastUsed = items.Length > 0
                        ? items.Max(item => item.UpdatedAt)
                        : catalogTag?.UpdatedAt ?? DateTimeOffset.MinValue
                };
            })
            .OrderByDescending(tag => tag.UsageCount)
            .ThenBy(tag => tag.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new TagsViewModel(tags);
    }
}
