namespace TermBullet.Application.Ports;

public interface ISettingsStore
{
    string SettingsPath { get; }

    Task<string?> GetAsync(
        string key,
        string profile = "default",
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> ListAsync(
        string profile = "default",
        CancellationToken cancellationToken = default);

    Task SetAsync(
        string key,
        string value,
        string profile = "default",
        CancellationToken cancellationToken = default);
}
