using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class WeeklyPlanningViewModelTests
{
    [Fact]
    public void Builds_week_buckets_metrics_and_backlog_context()
    {
        var weekItems = new[]
        {
            MakeItem("t-0426-1", ItemCollection.Week, ItemStatus.Open, Priority.High, "Fix auth JWT"),
            MakeItem("e-0426-1", ItemCollection.Week, ItemStatus.Open, Priority.None, "Review 16:00", ItemType.Event)
        };
        var backlogItems = new[]
        {
            MakeItem("t-0426-3", ItemCollection.Backlog, ItemStatus.Open, Priority.Medium, "Adjust compose")
        };

        var vm = new WeeklyPlanningViewModel(weekItems, backlogItems);

        Assert.Equal(4, vm.Buckets.Count);
        Assert.Equal(2, vm.WeekItems.Count);
        Assert.Single(vm.BacklogItems);
        Assert.Contains("open: 2", vm.Metrics);
    }

    private static ItemResult MakeItem(string publicRef, ItemCollection collection, ItemStatus status, Priority priority, string content, ItemType type = ItemType.Task) =>
        new(
            Guid.NewGuid(),
            publicRef,
            type,
            content,
            null,
            status,
            collection,
            priority,
            [],
            1,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
}

