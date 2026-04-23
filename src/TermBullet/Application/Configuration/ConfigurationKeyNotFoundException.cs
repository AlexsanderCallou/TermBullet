namespace TermBullet.Application.Configuration;

public sealed class ConfigurationKeyNotFoundException(string key)
    : Exception($"Configuration key not found: {key}.")
{
    public string Key { get; } = key;
}
