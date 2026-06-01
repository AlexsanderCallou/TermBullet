using System.Text.Json;
using TermBullet.Services.Configuration;

namespace TermBullet.Tests.Services.Configuration;

public sealed class TermBulletConfigServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "termbullet-config-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void Config_path_uses_install_directory_conf_json()
    {
        var service = new TermBulletConfigService(_root);

        Assert.Equal(Path.Combine(_root, "conf.json"), service.ConfigPath);
    }

    [Fact]
    public async Task LoadAsync_returns_null_when_config_does_not_exist()
    {
        var service = new TermBulletConfigService(_root);

        var config = await service.LoadAsync();

        Assert.Null(config);
    }

    [Fact]
    public async Task SaveAsync_writes_readable_json_config()
    {
        var dataRoot = Path.Combine(_root, "chosen");
        var service = new TermBulletConfigService(_root);

        await service.SaveAsync(new TermBulletConfig(dataRoot));

        var json = await File.ReadAllTextAsync(service.ConfigPath);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(dataRoot, document.RootElement.GetProperty("data_root").GetString());
    }

    [Fact]
    public async Task LoadAsync_reads_data_root_from_config()
    {
        var dataRoot = Path.Combine(_root, "chosen");
        var service = new TermBulletConfigService(_root);
        await service.SaveAsync(new TermBulletConfig(dataRoot));

        var config = await service.LoadAsync();

        Assert.NotNull(config);
        Assert.Equal(dataRoot, config.DataRoot);
    }

    [Fact]
    public async Task LoadAsync_rejects_malformed_json()
    {
        var service = new TermBulletConfigService(_root);
        Directory.CreateDirectory(_root);
        await File.WriteAllTextAsync(service.ConfigPath, "{");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LoadAsync());

        Assert.Contains("conf.json is malformed", exception.Message);
    }

    [Fact]
    public async Task LoadAsync_rejects_missing_data_root()
    {
        var service = new TermBulletConfigService(_root);
        Directory.CreateDirectory(_root);
        await File.WriteAllTextAsync(service.ConfigPath, "{}");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LoadAsync());

        Assert.Contains("data_root", exception.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
