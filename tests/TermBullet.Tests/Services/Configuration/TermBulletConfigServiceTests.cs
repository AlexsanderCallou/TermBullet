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
    public async Task SaveAsync_writes_ai_profiles_as_readable_json()
    {
        var dataRoot = Path.Combine(_root, "chosen");
        var service = new TermBulletConfigService(_root);
        var config = new TermBulletConfig(
            dataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new(
                        Provider: "openai",
                        Model: "gpt-4.1-mini",
                        BaseUrl: "https://api.openai.com/v1",
                        ApiKeySource: "environment",
                        ApiKeyEnv: "TERMBULLET_OPENAI_API_KEY")
                }));

        await service.SaveAsync(config);

        var json = await File.ReadAllTextAsync(service.ConfigPath);
        using var document = JsonDocument.Parse(json);
        var ai = document.RootElement.GetProperty("ai");
        Assert.Equal("gpt", ai.GetProperty("active_profile").GetString());
        var profile = ai.GetProperty("profiles").GetProperty("gpt");
        Assert.Equal("openai", profile.GetProperty("provider").GetString());
        Assert.Equal("gpt-4.1-mini", profile.GetProperty("model").GetString());
        Assert.Equal("https://api.openai.com/v1", profile.GetProperty("base_url").GetString());
        Assert.Equal("environment", profile.GetProperty("api_key_source").GetString());
        Assert.Equal("TERMBULLET_OPENAI_API_KEY", profile.GetProperty("api_key_env").GetString());
    }

    [Fact]
    public async Task LoadAsync_reads_ai_profiles_from_config()
    {
        var service = new TermBulletConfigService(_root);
        Directory.CreateDirectory(_root);
        await File.WriteAllTextAsync(service.ConfigPath,
            """
            {
              "data_root": "C:\\TermBulletData",
              "ai": {
                "active_profile": "local",
                "profiles": {
                  "local": {
                    "provider": "openai-compatible",
                    "model": "llama3.1",
                    "base_url": "http://localhost:11434/v1",
                    "api_key_source": "none"
                  }
                }
              }
            }
            """);

        var config = await service.LoadAsync();

        Assert.NotNull(config);
        Assert.NotNull(config.Ai);
        Assert.Equal("local", config.Ai.ActiveProfile);
        var profile = Assert.Single(config.Ai.Profiles);
        Assert.Equal("local", profile.Key);
        Assert.Equal("openai-compatible", profile.Value.Provider);
        Assert.Equal("llama3.1", profile.Value.Model);
        Assert.Equal("http://localhost:11434/v1", profile.Value.BaseUrl);
        Assert.Equal("none", profile.Value.ApiKeySource);
        Assert.Null(profile.Value.ApiKeyEnv);
    }

    [Fact]
    public async Task LoadAsync_allows_config_without_ai_section()
    {
        var service = new TermBulletConfigService(_root);
        Directory.CreateDirectory(_root);
        await File.WriteAllTextAsync(service.ConfigPath,
            """
            {
              "data_root": "C:\\TermBulletData"
            }
            """);

        var config = await service.LoadAsync();

        Assert.NotNull(config);
        Assert.Null(config.Ai);
    }

    [Fact]
    public async Task LoadAsync_rejects_malformed_json()
    {
        var dataRoot = Path.Combine(_root, "chosen");
        var service = new TermBulletConfigService(_root);
        var config = new TermBulletConfig(
            dataRoot,
            new AiConfiguration(
                "gpt",
                new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new(
                        Provider: "openai",
                        Model: "gpt-4.1-mini",
                        BaseUrl: "https://api.openai.com/v1",
                        ApiKeySource: "environment",
                        ApiKeyEnv: "TERMBULLET_OPENAI_API_KEY")
                }));

        await service.SaveAsync(config);

        var json = await File.ReadAllTextAsync(service.ConfigPath);
        using var document = JsonDocument.Parse(json);
        var ai = document.RootElement.GetProperty("ai");
        Assert.Equal("gpt", ai.GetProperty("active_profile").GetString());
        var profile = ai.GetProperty("profiles").GetProperty("gpt");
        Assert.Equal("openai", profile.GetProperty("provider").GetString());
        Assert.Equal("gpt-4.1-mini", profile.GetProperty("model").GetString());
        Assert.Equal("https://api.openai.com/v1", profile.GetProperty("base_url").GetString());
        Assert.Equal("environment", profile.GetProperty("api_key_source").GetString());
        Assert.Equal("TERMBULLET_OPENAI_API_KEY", profile.GetProperty("api_key_env").GetString());
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
