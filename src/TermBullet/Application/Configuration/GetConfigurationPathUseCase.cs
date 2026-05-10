using TermBullet.Repositories.Interfaces;

namespace TermBullet.Application.Configuration;

public sealed class GetConfigurationPathUseCase(ISettingsRepository settingsStore)
{
    public string Execute() => settingsStore.SettingsPath;
}
