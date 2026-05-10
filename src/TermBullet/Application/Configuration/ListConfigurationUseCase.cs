using TermBullet.Repositories.Interfaces;

namespace TermBullet.Application.Configuration;

public sealed class ListConfigurationUseCase(ISettingsRepository settingsStore)
{
    public Task<IReadOnlyDictionary<string, string>> ExecuteAsync(
        string profile = "default",
        CancellationToken cancellationToken = default)
    {
        return settingsStore.ListAsync(profile, cancellationToken);
    }
}
