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
<<<<<<< HEAD
    string? ApiKeyEnv = null,
    string? ApiKey = null,
    int? TimeoutSeconds = null);
=======
    string? ApiKeyEnv = null);
>>>>>>> 31d6ba16bacfc3554d22ce88aea847e70d502125
