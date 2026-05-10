using TermBullet.Repositories.Interfaces;

namespace TermBullet.Application.Configuration;

public sealed class GetConfigurationUseCase(ISettingsRepository settingsStore)
{
    public async Task<string> ExecuteAsync(
        string key,
        string profile = "default",
        CancellationToken cancellationToken = default)
    {
        var value = await settingsStore.GetAsync(key, profile, cancellationToken);
        return value ?? throw new ConfigurationKeyNotFoundException(key);
    }
}
