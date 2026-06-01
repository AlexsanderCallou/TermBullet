namespace TermBullet.Services.Configuration;

public sealed record TermBulletRuntimePaths(
    string ConfigPath,
    string DataRoot,
    string DataPath);
