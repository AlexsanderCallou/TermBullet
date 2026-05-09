using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class MigrateItemViewModelTests
{
    [Fact]
    public void ForDateDestination_shows_basic_item_data_and_date_result()
    {
        var vm = MigrateItemViewModel.ForDate(MakeItem(), new DateOnly(2026, 5, 12));

        Assert.Contains(vm.ItemLines, line => line.Contains("ref: t-0526-1", StringComparison.Ordinal));
        Assert.Contains(vm.ItemLines, line => line.Contains("content: Fix auth flow", StringComparison.Ordinal));
        Assert.Contains(vm.ItemLines, line => line.Contains("planned_for: 2026-05-09", StringComparison.Ordinal));
        Assert.Contains(vm.DestinationLines, line => line.Contains("(x) Date", StringComparison.Ordinal));
        Assert.Contains(vm.DestinationLines, line => line.Contains("planned_for: 2026-05-12", StringComparison.Ordinal));
        Assert.Contains(vm.ResultLines, line => line.Contains("original: t-0526-1 -> migrate", StringComparison.Ordinal));
        Assert.Contains(vm.ResultLines, line => line.Contains("new task: open at 2026-05-12", StringComparison.Ordinal));
    }

    [Fact]
    public void ForBacklogDestination_shows_backlog_result_without_date()
    {
        var vm = MigrateItemViewModel.ForBacklog(MakeItem());

        Assert.Contains(vm.DestinationLines, line => line.Contains("(x) Backlog", StringComparison.Ordinal));
        Assert.Contains(vm.DestinationLines, line => line.Contains("planned_for: -", StringComparison.Ordinal));
        Assert.Contains(vm.ResultLines, line => line.Contains("new task: open in backlog", StringComparison.Ordinal));
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
            PlannedFor: new DateOnly(2026, 5, 9),
            ScheduledAt: null,
            Version: 1,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);
}
