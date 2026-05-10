using TermBullet.Application.Items;
using TermBullet.Domain.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class MigrateItemViewModelTests
{
    [Fact]
    public void ForCollectionDestination_shows_basic_item_data_and_collection_result()
    {
        var vm = MigrateItemViewModel.ForCollection(MakeItem(), ItemCollection.Month);

        Assert.Contains(vm.ItemLines, line => line.Contains("ref: t-0526-1", StringComparison.Ordinal));
        Assert.Contains(vm.ItemLines, line => line.Contains("content: Fix auth flow", StringComparison.Ordinal));
        Assert.Contains(vm.DestinationLines, line => line.Contains("(x) month", StringComparison.Ordinal));
        Assert.Contains(vm.ResultLines, line => line.Contains("original: t-0526-1 -> migrate", StringComparison.Ordinal));
        Assert.Contains(vm.ResultLines, line => line.Contains("new task: open in month", StringComparison.Ordinal));
    }

    [Fact]
    public void ForBacklogDestination_shows_backlog_result_without_date()
    {
        var vm = MigrateItemViewModel.ForBacklog(MakeItem());

        Assert.Contains(vm.DestinationLines, line => line.Contains("(x) backlog", StringComparison.Ordinal));
        Assert.Contains(vm.ResultLines, line => line.Contains("new task: open in backlog", StringComparison.Ordinal));
    }

    [Fact]
    public void WithDestination_updates_collection_destination()
    {
        var vm = MigrateItemViewModel
            .ForCollection(MakeItem(), ItemCollection.Week)
            .WithDestination(ItemCollection.Today);
        Assert.Contains(vm.ResultLines, line => line.Contains("new task: open in today", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildRequest_maps_collection_destination()
    {
        var request = MigrateItemViewModel
            .ForCollection(MakeItem(), ItemCollection.Month)
            .BuildRequest();

        Assert.Equal("t-0526-1", request.PublicRef);
        Assert.Equal(ItemCollection.Month, request.DestinationCollection);
    }

    [Fact]
    public void BuildRequest_maps_backlog_destination_without_date()
    {
        var request = MigrateItemViewModel
            .ForBacklog(MakeItem())
            .BuildRequest();

        Assert.Equal(ItemCollection.Backlog, request.DestinationCollection);
    }

    private static ItemResult MakeItem() =>
        new(
            Id: Guid.NewGuid(),
            PublicRef: "t-0526-1",
            Type: ItemType.Task,
            Content: "Fix auth flow",
            Description: null,
            Status: ItemStatus.Open,
            Collection: ItemCollection.Today,
            Priority: Priority.High,
            Tags: ["auth", "cli"],
            ScheduledAt: null,
            Version: 1,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);
}
