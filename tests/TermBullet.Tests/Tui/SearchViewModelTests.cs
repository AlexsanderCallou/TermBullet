using TermBullet.Application.Items;
using TermBullet.Core.Items;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class SearchViewModelTests
{
    [Fact]
    public void Query_is_empty_initially()
    {
        var vm = new SearchViewModel();

        Assert.Equal(string.Empty, vm.Query);
    }

    [Fact]
    public void Results_is_empty_initially()
    {
        var vm = new SearchViewModel();

        Assert.Empty(vm.Results);
    }

    [Fact]
    public void SelectedItemIndex_is_minus_one_initially()
    {
        var vm = new SearchViewModel();

        Assert.Equal(-1, vm.SelectedItemIndex);
    }

    [Fact]
    public void SetResults_populates_results_and_resets_selection()
    {
        var vm = new SearchViewModel();
        var items = new[]
        {
            MakeItem("t-0426-1", "Fix auth JWT"),
            MakeItem("n-0426-1", "JWT audience error")
        };

        vm.SetResults(items);

        Assert.Equal(2, vm.Results.Count);
        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void SetResults_with_empty_list_sets_index_to_minus_one()
    {
        var vm = new SearchViewModel();

        vm.SetResults([]);

        Assert.Equal(-1, vm.SelectedItemIndex);
    }

    [Fact]
    public void UpdateQuery_changes_query_and_clears_results()
    {
        var vm = new SearchViewModel();
        vm.SetResults([MakeItem("t-0426-1", "Fix JWT")]);

        vm.UpdateQuery("jwt");

        Assert.Equal("jwt", vm.Query);
        Assert.Empty(vm.Results);
        Assert.Equal(-1, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectNextResult_advances_selection()
    {
        var vm = new SearchViewModel();
        vm.SetResults([MakeItem("t-0426-1", "First"), MakeItem("t-0426-2", "Second")]);

        vm.SelectNextResult();

        Assert.Equal(1, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectNextResult_does_not_advance_past_last_result()
    {
        var vm = new SearchViewModel();
        vm.SetResults([MakeItem("t-0426-1", "Only")]);

        vm.SelectNextResult();

        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectPreviousResult_moves_selection_back()
    {
        var vm = new SearchViewModel();
        vm.SetResults([MakeItem("t-0426-1", "First"), MakeItem("t-0426-2", "Second")]);
        vm.SelectNextResult();

        vm.SelectPreviousResult();

        Assert.Equal(0, vm.SelectedItemIndex);
    }

    [Fact]
    public void SelectedResult_returns_item_at_selected_index()
    {
        var vm = new SearchViewModel();
        vm.SetResults([MakeItem("t-0426-1", "First"), MakeItem("t-0426-2", "Second")]);
        vm.SelectNextResult();

        Assert.Equal("t-0426-2", vm.SelectedResult?.PublicRef);
    }

    [Fact]
    public void SelectedResult_is_null_when_no_results()
    {
        var vm = new SearchViewModel();

        Assert.Null(vm.SelectedResult);
    }

    [Fact]
    public void Results_map_ref_symbol_and_content_correctly()
    {
        var vm = new SearchViewModel();
        vm.SetResults([MakeItem("t-0426-1", "Fix JWT", ItemType.Task, ItemStatus.Open)]);

        Assert.Equal("t-0426-1", vm.Results[0].PublicRef);
        Assert.Equal("[ ]", vm.Results[0].Symbol);
        Assert.Equal("Fix JWT", vm.Results[0].Content);
    }

    private static ItemResult MakeItem(
        string publicRef,
        string content,
        ItemType type = ItemType.Task,
        ItemStatus status = ItemStatus.Open) =>
        new(
            Id: Guid.NewGuid(),
            PublicRef: publicRef,
            Type: type,
            Content: content,
            Description: null,
            Status: status,
            Collection: ItemCollection.Today,
            Priority: Priority.None,
            Tags: [],
            Version: 1,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);
}
