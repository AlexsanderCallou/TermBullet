using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;


public sealed class BacklogTriageViewModelTests
{
    [Fact]
    public void Items_contains_all_backlog_items_initially()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.High, []),
            MakeItem("t-0426-2", Priority.Medium, ["jwt"]),
            MakeItem("t-0426-3", Priority.Low, ["docker"])
        };

        var vm = new BacklogTriageViewModel(items);

        Assert.Equal(3, vm.FilteredItems.Count);
    }

    [Fact]
    public void FilterByTag_shows_only_items_with_matching_tag()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.High, ["jwt"]),
            MakeItem("t-0426-2", Priority.Medium, ["docker"]),
            MakeItem("t-0426-3", Priority.Low, ["jwt", "auth"])
        };

        var vm = new BacklogTriageViewModel(items);
        vm.SetTagFilter("jwt");

        Assert.Equal(2, vm.FilteredItems.Count);
        Assert.All(vm.FilteredItems, row => Assert.Contains("jwt", row.Tags));
    }

    [Fact]
    public void FilterByPriority_shows_only_items_with_matching_priority()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.High, []),
            MakeItem("t-0426-2", Priority.Medium, []),
            MakeItem("t-0426-3", Priority.High, [])
        };

        var vm = new BacklogTriageViewModel(items);
        vm.SetPriorityFilter(Priority.High);

        Assert.Equal(2, vm.FilteredItems.Count);
        Assert.All(vm.FilteredItems, row => Assert.Equal("high", row.Priority));
    }

    [Fact]
    public void ClearFilters_restores_all_items()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.High, ["jwt"]),
            MakeItem("t-0426-2", Priority.Medium, ["docker"])
        };

        var vm = new BacklogTriageViewModel(items);
        vm.SetTagFilter("jwt");
        vm.ClearFilters();

        Assert.Equal(2, vm.FilteredItems.Count);
    }

    [Fact]
    public void ActiveTagFilter_is_null_initially()
    {
        var vm = new BacklogTriageViewModel([]);

        Assert.Null(vm.ActiveTagFilter);
    }

    [Fact]
    public void ActivePriorityFilter_is_null_initially()
    {
        var vm = new BacklogTriageViewModel([]);

        Assert.Null(vm.ActivePriorityFilter);
    }

    [Fact]
    public void SelectedItemIndex_starts_at_zero_when_there_are_items()
    {
        var items = new[] { MakeItem("t-0426-1", Priority.None, []) };
        var vm = new BacklogTriageViewModel(items);

        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectedItemIndex_is_minus_one_when_empty()
    {
        var vm = new BacklogTriageViewModel([]);

        Assert.Equal(-1, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectNextItem_advances_selection()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.None, []),
            MakeItem("t-0426-2", Priority.None, [])
        };
        var vm = new BacklogTriageViewModel(items);

        vm.SelectNextItem();

        Assert.Equal(1, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectNextItem_does_not_advance_past_last_item()
    {
        var items = new[] { MakeItem("t-0426-1", Priority.None, []) };
        var vm = new BacklogTriageViewModel(items);

        vm.SelectNextItem();

        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectPreviousItem_moves_selection_back()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.None, []),
            MakeItem("t-0426-2", Priority.None, [])
        };
        var vm = new BacklogTriageViewModel(items);
        vm.SelectNextItem();

        vm.SelectPreviousItem();

        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectedItem_returns_item_at_selected_index_in_filtered_list()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.High, []),
            MakeItem("t-0426-2", Priority.Low, [])
        };
        var vm = new BacklogTriageViewModel(items);
        vm.SelectNextItem();

        Assert.Equal("t-0426-2", vm.SelectedItem?.PublicRef);
    }

    [Fact]
    public void SetTagFilter_resets_selection_to_first_item()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.None, ["jwt"]),
            MakeItem("t-0426-2", Priority.None, ["jwt"]),
            MakeItem("t-0426-3", Priority.None, ["docker"])
        };
        var vm = new BacklogTriageViewModel(items);
        vm.SelectNextItem();

        vm.SetTagFilter("jwt");

        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void AvailableTags_contains_all_distinct_tags_from_items()
    {
        var items = new[]
        {
            MakeItem("t-0426-1", Priority.None, ["jwt", "auth"]),
            MakeItem("t-0426-2", Priority.None, ["docker"]),
            MakeItem("t-0426-3", Priority.None, ["jwt"])
        };

        var vm = new BacklogTriageViewModel(items);

        Assert.Equal(3, vm.AvailableTags.Count);
        Assert.Contains("jwt", vm.AvailableTags);
        Assert.Contains("auth", vm.AvailableTags);
        Assert.Contains("docker", vm.AvailableTags);
    }

    private static ItemResult MakeItem(string publicRef, Priority priority, string[] tags) =>
        new(
            Id: Guid.NewGuid(),
            PublicRef: publicRef,
            Type: ItemType.Task,
            Content: $"Task {publicRef}",
            Description: null,
            Status: ItemStatus.Open,
            Collection: ItemCollection.Backlog,
            Priority: priority,
            Tags: tags,
            Version: 1,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);
}
