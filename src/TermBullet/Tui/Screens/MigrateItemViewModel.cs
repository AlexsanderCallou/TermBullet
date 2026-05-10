using TermBullet.Application.Items;

namespace TermBullet.Tui.Screens;

public sealed class MigrateItemViewModel
{
    private MigrateItemViewModel(
        ItemDisplayRow item,
        bool dateSelected,
        DateOnly? plannedFor)
    {
        Item = item;
        DateSelected = dateSelected;
        PlannedFor = plannedFor;
        ItemLines =
        [
            $"ref: {item.PublicRef}",
            $"content: {item.Content}",
            $"status: {item.Status}",
            $"collection: {item.Collection}",
            $"planned_for: {(item.PlannedFor is null ? "-" : item.PlannedFor.Value.ToString("yyyy-MM-dd"))}",
            $"priority: {item.Priority}",
            $"tags: {(item.Tags.Length > 0 ? string.Join(", ", item.Tags) : "-")}"
        ];
        DestinationLines = dateSelected
            ?
            [
                "(x) Date",
                $"    planned_for: {plannedFor:yyyy-MM-dd}",
                "( ) Backlog"
            ]
            :
            [
                "( ) Date",
                "    planned_for: -",
                "(x) Backlog"
            ];
        ResultLines = dateSelected
            ?
            [
                $"original: {item.PublicRef} -> migrate",
                $"new task: open at {plannedFor:yyyy-MM-dd}"
            ]
            :
            [
                $"original: {item.PublicRef} -> migrate",
                "new task: open in backlog"
            ];
    }

    public ItemDisplayRow Item { get; }

    public bool DateSelected { get; }

    public DateOnly? PlannedFor { get; }

    public IReadOnlyList<string> ItemLines { get; }

    public IReadOnlyList<string> DestinationLines { get; }

    public IReadOnlyList<string> ResultLines { get; }

    public static MigrateItemViewModel ForDate(ItemResult item, DateOnly plannedFor) =>
        ForDate(ItemDisplayRow.From(item), plannedFor);

    public static MigrateItemViewModel ForDate(ItemDisplayRow item, DateOnly plannedFor) =>
        new(item, dateSelected: true, plannedFor);

    public static MigrateItemViewModel ForBacklog(ItemResult item) =>
        ForBacklog(ItemDisplayRow.From(item));

    public static MigrateItemViewModel ForBacklog(ItemDisplayRow item) =>
        new(item, dateSelected: false, plannedFor: null);

    public MigrateItemViewModel ToggleDestination()
    {
        if (DateSelected)
        {
            return ForBacklog(Item);
        }

        return ForDate(Item, PlannedFor ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
    }

    public MigrateItemViewModel WithPlannedFor(DateOnly plannedFor) =>
        ForDate(Item, plannedFor);

    public MigrateItemRequest BuildRequest() =>
        new()
        {
            PublicRef = Item.PublicRef,
            DestinationCollection = DateSelected
                ? TermBullet.Core.Items.ItemCollection.Week
                : TermBullet.Core.Items.ItemCollection.Backlog,
            PlannedFor = DateSelected ? PlannedFor : null
        };
}
