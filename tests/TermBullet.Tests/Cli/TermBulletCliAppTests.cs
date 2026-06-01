using TermBullet.Services.Clock;
using TermBullet.Services.History;
using System.Text;
using TermBullet.Application.History;
using TermBullet.Cli;
using TermBullet.Services.Configuration;

namespace TermBullet.Tests.Cli;

public sealed class TermBulletCliAppTests
{
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

        var exitCode = await app.InvokeAsync(["history", "clear", "--month", "04_2026", "--force"]);

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
        Assert.DoesNotContain("config", dependencies.Output.ToString());
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
        Assert.Contains("1.1.2", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_writes_version_for_short_version_flag()
    {
        var dependencies = CreateDependencies();
        var app = CreateApp(dependencies);

        var exitCode = await app.InvokeAsync(["-v"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("1.1.2", dependencies.Output.ToString());
    }

    [Fact]
    public async Task InvokeAsync_runs_path_command_when_runtime_paths_are_available()
    {
        var dependencies = CreateDependencies();
        var runtimePaths = new TermBulletRuntimePaths(
            @"C:\TermBullet\conf.json",
            @"C:\TermBulletData",
            @"C:\TermBulletData\data");
        var app = CreateApp(dependencies, runtimePaths: runtimePaths);

        var exitCode = await app.InvokeAsync(["path"]);

        Assert.Equal(0, exitCode);
        var output = dependencies.Output.ToString();
        Assert.Contains("config: C:\\TermBullet\\conf.json", output);
        Assert.Contains("data_root: C:\\TermBulletData", output);
        Assert.Contains("data: C:\\TermBulletData\\data", output);
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
        Func<CancellationToken, Task>? startupAction = null,
        TermBulletRuntimePaths? runtimePaths = null)
    {
        return new TermBulletCliApp(
            new ClearStoredHistoryUseCase(
                dependencies.HistoryService,
                new FixedClock(new DateTimeOffset(2026, 4, 23, 12, 0, 0, TimeSpan.Zero))),
            dependencies.Output,
            dependencies.Error,
            runtimePaths: runtimePaths,
            startupAction: startupAction);
    }

    private static TestDependencies CreateDependencies()
    {
        return new TestDependencies(
            new FakeHistoryMaintenanceService(),
            new StringWriter(new StringBuilder()),
            new StringWriter(new StringBuilder()));
    }

    private sealed record TestDependencies(
        FakeHistoryMaintenanceService HistoryService,
        StringWriter Output,
        StringWriter Error);

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
