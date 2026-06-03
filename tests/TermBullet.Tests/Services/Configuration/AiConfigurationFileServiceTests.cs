using TermBullet.Services.Configuration;

namespace TermBullet.Tests.Services.Configuration;

public sealed class AiConfigurationFileServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "termbullet-aiconf-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void Parse_reads_profiles_comments_literal_key_and_timeout()
    {
        var config = AiConfigurationFileService.Parse(
            """
            # local profile
            [local-gemma]
            provider=openai-compatible
            model=gemma3:4b
            base_url=http://localhost:11434/v1
            api_key=ollama
            default=true
            timeout_seconds=180
            """);

        Assert.Equal("local-gemma", config.ActiveProfile);
        var profile = Assert.Single(config.Profiles).Value;
        Assert.Equal("openai-compatible", profile.Provider);
        Assert.Equal("gemma3:4b", profile.Model);
        Assert.Equal("http://localhost:11434/v1", profile.BaseUrl);
        Assert.Equal("literal", profile.ApiKeySource);
        Assert.Equal("ollama", profile.ApiKey);
        Assert.Equal(180, profile.TimeoutSeconds);
    }

    [Fact]
    public async Task SetActiveProfileAsync_rewrites_default_profile()
    {
        var service = new AiConfigurationFileService(_root);
        Directory.CreateDirectory(_root);
        await File.WriteAllTextAsync(
            service.FilePath,
            """
            [local-gemma]
            provider=openai-compatible
            model=gemma3:4b
            base_url=http://localhost:11434/v1
            api_key=ollama
            default=true

            [local-llama-fast]
            provider=openai-compatible
            model=llama3.2:1b
            base_url=http://localhost:11434/v1
            api_key=ollama
            """);

        await service.SetActiveProfileAsync("local-llama-fast");

        var config = await service.LoadConfigAsync();
        Assert.Equal("local-llama-fast", config.Ai?.ActiveProfile);
    }

    [Fact]
    public void Parse_rejects_multiple_profiles_without_default()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => AiConfigurationFileService.Parse(
                """
                [one]
                provider=openai-compatible
                model=one
                base_url=http://localhost:11434/v1

                [two]
                provider=openai-compatible
                model=two
                base_url=http://localhost:11434/v1
                """));

        Assert.Contains("set-ai", exception.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
