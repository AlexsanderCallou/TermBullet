using System.Text;
using TermBullet.Services.Configuration;

namespace TermBullet.Tests.Services.Configuration;

public sealed class DataDirectorySetupServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "termbullet-setup-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ResolveOrPromptAsync_uses_existing_config_without_prompting()
    {
        var installRoot = Path.Combine(_root, "install");
        var dataRoot = Path.Combine(_root, "storage");
        var configService = new TermBulletConfigService(installRoot);
        await configService.SaveAsync(new TermBulletConfig(dataRoot));
        var output = new StringWriter();
        var setup = new DataDirectorySetupService(
            configService,
            new DataDirectoryValidator(),
            () => Path.Combine(_root, "default"));

        var result = await setup.ResolveOrPromptAsync(
            new StringReader("ignored"),
            output);

        Assert.Equal(Path.GetFullPath(dataRoot), result.DataRoot);
        Assert.Equal(configService.ConfigPath, result.ConfigPath);
        Assert.Equal(string.Empty, output.ToString());
    }

    [Fact]
    public async Task ResolveOrPromptAsync_prompts_and_saves_config_when_missing()
    {
        var installRoot = Path.Combine(_root, "install");
        var dataRoot = Path.Combine(_root, "chosen");
        var configService = new TermBulletConfigService(installRoot);
        var output = new StringWriter(new StringBuilder());
        var setup = new DataDirectorySetupService(
            configService,
            new DataDirectoryValidator(),
            () => Path.Combine(_root, "default"));

        var result = await setup.ResolveOrPromptAsync(
            new StringReader($"{dataRoot}{Environment.NewLine}"),
            output);

        Assert.Equal(Path.GetFullPath(dataRoot), result.DataRoot);
        Assert.True(File.Exists(configService.ConfigPath));
        Assert.Contains("Choose TermBullet data directory", output.ToString());
    }

    [Fact]
    public async Task ResolveOrPromptAsync_uses_default_when_prompt_is_empty()
    {
        var installRoot = Path.Combine(_root, "install");
        var defaultRoot = Path.Combine(_root, "default");
        var configService = new TermBulletConfigService(installRoot);
        var setup = new DataDirectorySetupService(
            configService,
            new DataDirectoryValidator(),
            () => defaultRoot);

        var result = await setup.ResolveOrPromptAsync(
            new StringReader(Environment.NewLine),
            new StringWriter());

        Assert.Equal(Path.GetFullPath(defaultRoot), result.DataRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
