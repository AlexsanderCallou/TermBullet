namespace TermBullet.Services.Configuration;

public sealed record TermBulletConfig(
    string DataRoot,
    AiConfiguration? Ai = null);

public sealed record AiConfiguration(
    string? ActiveProfile,
    IReadOnlyDictionary<string, AiProfile> Profiles);

public sealed record AiProfile(
    string Provider,
    string Model,
    string? BaseUrl = null,
    string ApiKeySource = "environment",
    string? ApiKeyEnv = null,
    string? ApiKey = null,
    int? TimeoutSeconds = null);
