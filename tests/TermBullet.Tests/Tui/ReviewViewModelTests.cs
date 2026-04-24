using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class ReviewViewModelTests
{
    [Fact]
    public void Builds_summary_and_review_sections_from_items()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", ItemCollection.Today, ItemStatus.Done, Priority.High, "Fix auth JWT"),
            MakeItem("t-0426-2", ItemCollection.Week, ItemStatus.Migrated, Priority.Medium, "Review migrations"),
            MakeItem("t-0426-3", ItemCollection.Backlog, ItemStatus.Cancelled, Priority.None, "Old blocked task")
        };

        var vm = new ReviewViewModel(items);

        Assert.Equal(3, vm.Collections.Count);
        Assert.Contains("done: 1", vm.Summary);
        Assert.Contains("Fix auth JWT", vm.WhatMoved);
        Assert.Contains("Old blocked task", vm.WhatBlocked);
        Assert.Equal(3, vm.NextCycle.Count);
    }

    private static ItemResult MakeItem(string publicRef, ItemCollection collection, ItemStatus status, Priority priority, string content) =>
        new(
            Guid.NewGuid(),
            publicRef,
            ItemType.Task,
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
