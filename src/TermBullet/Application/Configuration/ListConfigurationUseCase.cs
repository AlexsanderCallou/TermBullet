using TermBullet.Application.Ports;

namespace TermBullet.Application.Configuration;

public sealed class ListConfigurationUseCase(ISettingsStore settingsStore)
{
    public Task<IReadOnlyDictionary<string, string>> ExecuteAsync(
        string profile = "default",
        CancellationToken cancellationToken = default)
    {
        return settingsStore.ListAsync(profile, cancellationToken);
    }
}
