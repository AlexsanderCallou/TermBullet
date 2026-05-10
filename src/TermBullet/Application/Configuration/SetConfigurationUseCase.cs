using TermBullet.Repositories.Interfaces;

namespace TermBullet.Application.Configuration;

public sealed class SetConfigurationUseCase(ISettingsRepository settingsStore)
{
    public Task ExecuteAsync(
        string key,
        string value,
        string profile = "default",
        CancellationToken cancellationToken = default)
    {
        return settingsStore.SetAsync(key, value, profile, cancellationToken);
    }
}
