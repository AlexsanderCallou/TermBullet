using TermBullet.Services.Ai;

namespace TermBullet.Tests.Services.Ai;

public sealed class PlanningAgentPromptLoaderTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "termbullet-agent-loader-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task LoadAsync_reads_installed_planning_agent_prompt()
    {
        var agentPath = Path.Combine(_root, "agents", "planning-bulletjournal-agent.md");
        Directory.CreateDirectory(Path.GetDirectoryName(agentPath)!);
        await File.WriteAllTextAsync(agentPath, "You are TermBullet's planning agent.");
        var loader = new PlanningAgentPromptLoader(_root);

        var prompt = await loader.LoadAsync();

        Assert.Equal("You are TermBullet's planning agent.", prompt);
    }

    [Fact]
    public async Task LoadAsync_uses_embedded_planning_agent_prompt_when_installed_file_is_missing()
    {
        var loader = new PlanningAgentPromptLoader(_root);

        var prompt = await loader.LoadAsync();

        Assert.Contains("TermBullet Planning Bullet Journal Agent", prompt);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
