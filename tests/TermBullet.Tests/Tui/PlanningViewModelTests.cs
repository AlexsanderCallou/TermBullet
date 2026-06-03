using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class PlanningViewModelTests
{
    [Fact]
    public void ForHub_shows_new_and_revise_planning_options()
    {
        var vm = PlanningViewModel.ForHub();

        Assert.Equal(PlanningScreenMode.Hub, vm.Mode);
        Assert.Contains("> New Planning", vm.PrimaryLines);
        Assert.Contains("  Revise Planning", vm.PrimaryLines);
        Assert.Contains(vm.SecondaryLines, line => line.Contains("fresh AI draft", StringComparison.Ordinal));
    }

    [Fact]
    public void ForNewPlanning_matches_project_and_weekly_modes()
    {
        var vm = PlanningViewModel.ForNewPlanning();

        Assert.Equal(PlanningScreenMode.NewPlanning, vm.Mode);
        Assert.Contains("> Project Plan", vm.PrimaryLines);
        Assert.Contains("  Weekly Plan", vm.PrimaryLines);
        Assert.Contains("Apply plan", vm.SecondaryLines);
        Assert.Contains("Write a message...", vm.PromptLines);
    }

    [Fact]
    public void ForRevisePlanning_matches_review_modes()
    {
        var vm = PlanningViewModel.ForRevisePlanning();

        Assert.Equal(PlanningScreenMode.RevisePlanning, vm.Mode);
        Assert.Contains("> Weekly Review", vm.PrimaryLines);
        Assert.Contains("  Project Review", vm.PrimaryLines);
        Assert.Contains("Apply changes", vm.SecondaryLines);
        Assert.Contains(vm.PrimaryLines, line => line.Contains("allowed actions", StringComparison.Ordinal));
    }
}
