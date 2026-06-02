using TermBullet.Application.Items;
using TermBullet.Domain.Items;

namespace TermBullet.Tui.Screens;

public sealed class MigrateItemViewModel
{
    private MigrateItemViewModel(ItemDisplayRow item, ItemCollection destinationCollection)
    {
        Item = item;
        DestinationCollection = destinationCollection;
        ItemLines =
        [
            $"ref: {item.PublicRef}",
            $"content: {item.Content}",
            $"status: {item.Status}",
            $"collection: {item.Collection}",
            $"priority: {item.Priority}",
            $"tag: {item.Tag}"
        ];
        DestinationLines =
        [
            FormatDestinationLine(ItemCollection.Today),
            FormatDestinationLine(ItemCollection.Week),
            FormatDestinationLine(ItemCollection.Month),
            FormatDestinationLine(ItemCollection.Backlog)
        ];
        ResultLines =
        [
            $"{item.PublicRef}: {item.Collection} -> {FormatCollection(destinationCollection)}",
            "same task, same ref"
        ];
    }

    public ItemDisplayRow Item { get; }

    public ItemCollection DestinationCollection { get; }

    public IReadOnlyList<string> ItemLines { get; }

    public IReadOnlyList<string> DestinationLines { get; }

    public IReadOnlyList<string> ResultLines { get; }

    public static MigrateItemViewModel ForCollection(ItemResult item, ItemCollection collection) =>
        ForCollection(ItemDisplayRow.From(item), collection);

    public static MigrateItemViewModel ForCollection(ItemDisplayRow item, ItemCollection collection) =>
        new(item, collection);

    public static MigrateItemViewModel ForBacklog(ItemResult item) =>
        ForCollection(ItemDisplayRow.From(item), ItemCollection.Backlog);

    public static MigrateItemViewModel ForBacklog(ItemDisplayRow item) =>
        ForCollection(item, ItemCollection.Backlog);

    public MigrateItemViewModel WithDestination(ItemCollection collection) =>
        ForCollection(Item, collection);

    public MigrateItemRequest BuildRequest() =>
        new()
        {
            PublicRef = Item.PublicRef,
            DestinationCollection = DestinationCollection
        };

    private string FormatDestinationLine(ItemCollection collection)
    {
        var selected = DestinationCollection == collection ? "x" : " ";
        return $"({selected}) {FormatCollection(collection)}";
    }

    private static string FormatCollection(ItemCollection collection) =>
        collection.ToString().ToLowerInvariant();
}
