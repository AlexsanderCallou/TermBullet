using TermBullet.Application.Ports;

namespace TermBullet.Application.Configuration;

public sealed class GetConfigurationPathUseCase(ISettingsStore settingsStore)
{
    public string Execute() => settingsStore.SettingsPath;
}
