using TermBullet.Tui.Screens;

namespace TermBullet.Tests.Tui;

public sealed class PlanningViewModelTests
{
    [Fact]
    public void ForHub_uses_new_project_planning()
    {
        var vm = PlanningViewModel.ForHub();

        Assert.Equal(PlanningScreenMode.NewPlanning, vm.Mode);
        Assert.Contains("Topic: -", vm.PrimaryLines);
        Assert.Contains("Generate draft", vm.SecondaryLines);
    }

    [Fact]
    public void ForNewPlanning_matches_guided_project_planning()
    {
        var vm = PlanningViewModel.ForNewPlanning();

        Assert.Equal(PlanningScreenMode.NewPlanning, vm.Mode);
        Assert.Contains("Topic: -", vm.PrimaryLines);
        Assert.Contains("Project tag: -", vm.PrimaryLines);
        Assert.Contains("Detail level: High", vm.PrimaryLines);
        Assert.Contains("Start today: Yes", vm.PrimaryLines);
        Assert.Contains("Generate draft", vm.SecondaryLines);
        Assert.Contains("Apply plan", vm.SecondaryLines);
        Assert.Contains(vm.PromptLines, line => line.Contains("Setup panel", StringComparison.Ordinal));
    }
}
