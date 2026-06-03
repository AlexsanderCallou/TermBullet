using TermBullet.Services.Ai;
using TermBullet.Services.Configuration;

namespace TermBullet.Tests.Services.Ai;

public sealed class AiPlanningProviderFactoryTests
{
    [Theory]
    [InlineData("openai")]
    [InlineData("openai-compatible")]
    public void Create_returns_openai_compatible_provider_for_supported_profiles(string providerName)
    {
        var factory = new AiPlanningProviderFactory(() => new HttpClient());
        var config = new TermBulletConfig(
            DataRoot: "C:\\TermBulletData",
            Ai: new AiConfiguration(
                ActiveProfile: "gpt",
                Profiles: new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new(
                        Provider: providerName,
                        Model: "gpt-4.1-mini",
                        BaseUrl: "https://api.example.test/v1",
                        ApiKeySource: "none")
                }));

        var provider = factory.Create(config);

        Assert.IsType<OpenAiCompatiblePlanningProvider>(provider);
    }

    [Fact]
    public void Create_rejects_missing_ai_configuration()
    {
        var factory = new AiPlanningProviderFactory(() => new HttpClient());
        var config = new TermBulletConfig(DataRoot: "C:\\TermBulletData");

        var exception = Record.Exception(() => factory.Create(config));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("AI is not configured", exception.Message);
    }

    [Fact]
    public void Create_rejects_missing_active_profile()
    {
        var factory = new AiPlanningProviderFactory(() => new HttpClient());
        var config = new TermBulletConfig(
            DataRoot: "C:\\TermBulletData",
            Ai: new AiConfiguration(
                ActiveProfile: null,
                Profiles: new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini")
                }));

        var exception = Record.Exception(() => factory.Create(config));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("active AI profile", exception.Message);
    }

    [Fact]
    public void Create_rejects_unknown_active_profile()
    {
        var factory = new AiPlanningProviderFactory(() => new HttpClient());
        var config = new TermBulletConfig(
            DataRoot: "C:\\TermBulletData",
            Ai: new AiConfiguration(
                ActiveProfile: "missing",
                Profiles: new Dictionary<string, AiProfile>
                {
                    ["gpt"] = new("openai", "gpt-4.1-mini")
                }));

        var exception = Record.Exception(() => factory.Create(config));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("AI profile not found", exception.Message);
    }

    [Fact]
    public void Create_rejects_unsupported_provider()
    {
        var factory = new AiPlanningProviderFactory(() => new HttpClient());
        var config = new TermBulletConfig(
            DataRoot: "C:\\TermBulletData",
            Ai: new AiConfiguration(
                ActiveProfile: "bad",
                Profiles: new Dictionary<string, AiProfile>
                {
                    ["bad"] = new("unknown", "model")
                }));

        var exception = Record.Exception(() => factory.Create(config));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("Unsupported AI provider", exception.Message);
    }
}
