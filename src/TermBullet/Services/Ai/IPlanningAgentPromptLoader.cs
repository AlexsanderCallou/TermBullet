namespace TermBullet.Services.Ai;

public interface IPlanningAgentPromptLoader
{
    Task<string> LoadAsync(CancellationToken cancellationToken = default);
}
