using TermBullet.Services.Configuration;

namespace TermBullet.Services.Ai;

public sealed class AiPlanningProviderFactory(Func<HttpClient> httpClientFactory)
{
    public IAiPlanningProvider Create(TermBulletConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (config.Ai is null || config.Ai.Profiles.Count == 0)
        {
            throw new InvalidOperationException("AI is not configured. Add an AI profile before using planning.");
        }

        if (string.IsNullOrWhiteSpace(config.Ai.ActiveProfile))
        {
            throw new InvalidOperationException("No active AI profile is configured.");
        }

        if (!config.Ai.Profiles.TryGetValue(config.Ai.ActiveProfile, out var profile))
        {
            throw new InvalidOperationException($"AI profile not found: {config.Ai.ActiveProfile}");
        }

        var provider = profile.Provider.Trim().ToLowerInvariant();
        return provider switch
        {
            "openai" or "openai-compatible" => new OpenAiCompatiblePlanningProvider(httpClientFactory(), profile),
            _ => throw new InvalidOperationException($"Unsupported AI provider: {profile.Provider}")
        };
    }
}
