using TermBullet.Services.Clock;
using TermBullet.Services.History;
using System.Text;
using TermBullet.Application.Configuration;
using TermBullet.Application.History;
using TermBullet.Repositories.Interfaces;
using TermBullet.Cli;

namespace TermBullet.Tests.Cli;

public sealed class TermBulletCliAppTests
{
    [Fact]
    public async Task InvokeAsync_runs_config_list_and_writes_settings()
    {
        var dependencies = CreateDependencies();
        await dependencies.SettingsStore.SetAsync("theme", "dark", "work");
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["config", "list", "--profile", "work"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("theme=dark", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_config_get_and_writes_value()
    {
        var dependencies = CreateDependencies();
        await dependencies.SettingsStore.SetAsync("theme", "dark");
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["config", "get", "theme"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("dark", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_config_set_and_persists_value()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["config", "set", "theme", "dark", "--profile", "work"]);

        Assert.Equal(0, exitCode);
        Assert.Equal("dark", await dependencies.SettingsStore.GetAsync("theme", "work"));
    }

    [Fact]
    public async Task InvokeAsync_runs_config_path_and_writes_store_path()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["config", "path"]);

        Assert.Equal(0, exitCode);
        Assert.Contains(dependencies.SettingsStore.SettingsPath, dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_history_clear_for_specific_month()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["history", "clear", "--month", "04_2026", "--force"]);

        Assert.Equal(0, exitCode);
        Assert.Equal((4, 2026), dependencies.HistoryService.ClearedMonth);
    }

    [Fact]
    public async Task InvokeAsync_runs_history_clear_for_all_months()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["history", "clear", "--all", "--force"]);

        Assert.Equal(0, exitCode);
        Assert.True(dependencies.HistoryService.ClearAllCalled);
    }

    [Fact]
    public async Task InvokeAsync_runs_startup_action_before_command_dispatch()
    {
        var dependencies = CreateDependencies();
        var startupCalled = false;
        var app = CreateApp(dependencies, startupAction: _ =>
        {
            startupCalled = true;
            return Task.CompletedTask;
        });

        var exitCode = await app.InvokeAsync(["config", "path"]);

        Assert.Equal(0, exitCode);
        Assert.True(startupCalled);
    }

    [Fact]
    public async Task InvokeAsync_writes_root_help_to_output()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["--help"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("TermBullet - Local-First Terminal Planner", dependencies.Output.ToString());
        Assert.Contains("config", dependencies.Output.ToString());
        Assert.DoesNotContain("export", dependencies.Output.ToString());
        Assert.DoesNotContain("import", dependencies.Output.ToString());
        Assert.DoesNotContain("Mostrar", dependencies.Output.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_writes_nested_help_to_output()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["history", "clear", "--help"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Clear stored history entries", dependencies.Output.ToString());
        Assert.Contains("--month", dependencies.Output.ToString());
        Assert.Contains("--all", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_version_for_version_flag()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["--version"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("1.0.0", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_version_for_short_version_flag()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["-v"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("1.0.0", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_parse_error_for_unknown_command()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["unknown-command"]);

        Assert.Equal(1, exitCode);
        var errorOutput = dependencies.Error.ToString();
        Assert.False(string.IsNullOrWhiteSpace(errorOutput));
        Assert.True(
            errorOutput.Contains("unrecognized command", StringComparison.OrdinalIgnoreCase)
            || errorOutput.Contains("comando", StringComparison.OrdinalIgnoreCase),
            $"Unexpected error output: {errorOutput}");
    }

    private static TermBulletCliApp CreateApp(
        TestDependencies dependencies,
        Func<CancellationToken, Task>? startupAction = null)
    {
        return new TermBulletCliApp(
            new ListConfigurationUseCase(dependencies.SettingsStore),
            new GetConfigurationUseCase(dependencies.SettingsStore),
            new SetConfigurationUseCase(dependencies.SettingsStore),
            new GetConfigurationPathUseCase(dependencies.SettingsStore),
            new ClearStoredHistoryUseCase(
                dependencies.HistoryService,
                new FixedClock(new DateTimeOffset(2026, 4, 23, 12, 0, 0, TimeSpan.Zero))),
            dependencies.Output,
            dependencies.Error,
            startupAction: startupAction);
    }

    private static TestDependencies CreateDependencies()
    {
        return new TestDependencies(
            new FakeSettingsStore(),
            new FakeHistoryMaintenanceService(),
            new StringWriter(new StringBuilder()),
            new StringWriter(new StringBuilder()));
    }

    private sealed record TestDependencies(
        FakeSettingsStore SettingsStore,
        FakeHistoryMaintenanceService HistoryService,
        StringWriter Output,
        StringWriter Error);

    private sealed class FakeSettingsStore : ISettingsRepository
    {
        private readonly Dictionary<string, Dictionary<string, string>> _profiles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["default"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };

        public string SettingsPath => "C:\\term\\data\\settings.json";

        public Task<string?> GetAsync(string key, string profile = "default", CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _profiles.TryGetValue(profile, out var values) && values.TryGetValue(key, out var value)
                    ? value
                    : null);
        }

        public Task<IReadOnlyDictionary<string, string>> ListAsync(string profile = "default", CancellationToken cancellationToken = default)
        {
            IReadOnlyDictionary<string, string> values =
                _profiles.TryGetValue(profile, out var profileValues)
                    ? new Dictionary<string, string>(profileValues, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(values);
        }

        public Task SetAsync(string key, string value, string profile = "default", CancellationToken cancellationToken = default)
        {
            if (!_profiles.TryGetValue(profile, out var values))
            {
                values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _profiles[profile] = values;
            }

            values[key] = value;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHistoryMaintenanceService : IHistoryMaintenanceService
    {
        public (int Month, int Year)? ClearedMonth { get; private set; }

        public bool ClearAllCalled { get; private set; }

        public Task ClearMonthAsync(int month, int year, CancellationToken cancellationToken = default)
        {
            ClearedMonth = (month, year);
            return Task.CompletedTask;
        }

        public Task ClearAllAsync(CancellationToken cancellationToken = default)
        {
            ClearAllCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }
}
