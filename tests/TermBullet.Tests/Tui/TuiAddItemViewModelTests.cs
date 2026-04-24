using TermBullet.Core.Items;
using TermBullet.Tui.Navigation;
using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class TuiAddItemViewModelTests
{
    [Theory]
    [InlineData(TuiScreen.MainDashboard, ItemCollection.Today)]
    [InlineData(TuiScreen.DailyFocus, ItemCollection.Today)]
    [InlineData(TuiScreen.WeeklyPlanning, ItemCollection.Week)]
    [InlineData(TuiScreen.BacklogTriage, ItemCollection.Backlog)]
    [InlineData(TuiScreen.Review, ItemCollection.Monthly)]
    public void ForSourceScreen_ResolvesExpectedDefaultCollection(
        TuiScreen sourceScreen,
        ItemCollection expectedCollection)
    {
        var viewModel = TuiAddItemViewModel.ForSourceScreen(sourceScreen);

        Assert.Equal(expectedCollection, viewModel.Collection);
    }

    [Fact]
    public void ForSourceScreen_ProvidesTaskNoteAndEventExamples()
    {
        var viewModel = TuiAddItemViewModel.ForSourceScreen(TuiScreen.MainDashboard);

        Assert.Contains(viewModel.Examples, line => line.Contains("- Review pull request", StringComparison.Ordinal));
        Assert.Contains(viewModel.Examples, line => line.Contains(". Investigate stacktrace", StringComparison.Ordinal));
        Assert.Contains(viewModel.Examples, line => line.Contains("o Team sync at 16:00", StringComparison.Ordinal));
    }

    [Fact]
    public void WithError_PreservesSourceCollectionAndExposesError()
    {
        var viewModel = TuiAddItemViewModel
            .ForSourceScreen(TuiScreen.BacklogTriage)
            .WithError("Capture text is required.");

        Assert.Equal(ItemCollection.Backlog, viewModel.Collection);
        Assert.Equal("Capture text is required.", viewModel.Error);
    }
}
