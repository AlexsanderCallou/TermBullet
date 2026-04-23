using TermBullet.Application.Ports;

namespace TermBullet.Application.Configuration;

public sealed class SetConfigurationUseCase(ISettingsStore settingsStore)
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
