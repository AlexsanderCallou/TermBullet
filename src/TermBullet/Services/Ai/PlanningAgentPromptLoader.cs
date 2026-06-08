namespace TermBullet.Services.Ai;

public sealed class PlanningAgentPromptLoader(string installDirectory) : IPlanningAgentPromptLoader
{
    public const string RelativeAgentPath = "agents/planning-bulletjournal-agent.md";
    public const string EmbeddedAgentResourceName = "TermBullet.Services.Ai.Agents.planning-bulletjournal-agent.md";

    public string InstallDirectory { get; } = Path.GetFullPath(installDirectory);

    public string AgentPath => Path.Combine(InstallDirectory, "agents", "planning-bulletjournal-agent.md");

    public async Task<string> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(AgentPath))
        {
            return LoadEmbeddedPrompt();
        }

        try
        {
            var prompt = await File.ReadAllTextAsync(AgentPath, cancellationToken);
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new InvalidOperationException(
                    $"AI planning agent prompt is empty: {AgentPath}");
            }

            return prompt;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Cannot read AI planning agent prompt: {AgentPath}",
                exception);
        }
    }

    private static string LoadEmbeddedPrompt()
    {
        var assembly = typeof(PlanningAgentPromptLoader).Assembly;
        using var stream = assembly.GetManifestResourceStream(EmbeddedAgentResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(
                $"AI planning agent prompt is missing: {EmbeddedAgentResourceName}");
        }

        using var reader = new StreamReader(stream);
        var prompt = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new InvalidOperationException(
                $"AI planning agent prompt is empty: {EmbeddedAgentResourceName}");
        }

        return prompt;
    }
}
