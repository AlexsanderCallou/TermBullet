using TermBullet.Application.Configuration;
using TermBullet.Application.Ports;

namespace TermBullet.Tests.Application.Configuration;

public sealed class ConfigurationUseCaseTests
{
    [Fact]
    public async Task ListConfigurationUseCase_returns_values_for_profile()
    {
        var store = new FakeSettingsStore();
        await store.SetAsync("theme", "dark");
        await store.SetAsync("compact_lists", "true");
        var useCase = new ListConfigurationUseCase(store);

        var settings = await useCase.ExecuteAsync();

        Assert.Equal(2, settings.Count);
        Assert.Equal("dark", settings["theme"]);
        Assert.Equal("true", settings["compact_lists"]);
    }

    [Fact]
    public async Task GetConfigurationUseCase_returns_value_for_existing_key()
    {
        var store = new FakeSettingsStore();
        await store.SetAsync("theme", "dark");
        var useCase = new GetConfigurationUseCase(store);

        var value = await useCase.ExecuteAsync("theme");

        Assert.Equal("dark", value);
    }

    [Fact]
    public async Task GetConfigurationUseCase_throws_when_key_is_missing()
    {
        var store = new FakeSettingsStore();
        var useCase = new GetConfigurationUseCase(store);

        var exception = await Assert.ThrowsAsync<ConfigurationKeyNotFoundException>(
            () => useCase.ExecuteAsync("theme"));

        Assert.Equal("theme", exception.Key);
    }

    [Fact]
    public async Task SetConfigurationUseCase_persists_value_for_profile()
    {
        var store = new FakeSettingsStore();
        var useCase = new SetConfigurationUseCase(store);

        await useCase.ExecuteAsync("theme", "dark", "work");

        Assert.Equal("dark", await store.GetAsync("theme", "work"));
    }

    [Fact]
    public async Task GetConfigurationPathUseCase_returns_store_path()
    {
        var store = new FakeSettingsStore();
        var useCase = new GetConfigurationPathUseCase(store);

        var path = useCase.Execute();

        Assert.Equal("C:\\term\\data\\settings.json", path);
    }

    private sealed class FakeSettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, Dictionary<string, string>> _profiles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };

        public string SettingsPath => "C:\\term\\data\\settings.json";

        public Task<string?> GetAsync(string key, string profile = "default", CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            return Task.FromResult(
                _profiles.TryGetValue(profile, out var values) && values.TryGetValue(key, out var value)
                    ? value
                    : null);
        }

        public Task<IReadOnlyDictionary<string, string>> ListAsync(string profile = "default", CancellationToken cancellationToken = default)
        {
            IReadOnlyDictionary<string, string> result =
                _profiles.TryGetValue(profile, out var values)
                    ? new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(result);
        }

        public Task SetAsync(string key, string value, string profile = "default", CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            if (!_profiles.TryGetValue(profile, out var values))
            {
                values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _profiles[profile] = values;
            }

            values[key] = value;
            return Task.CompletedTask;
        }
    }
}
