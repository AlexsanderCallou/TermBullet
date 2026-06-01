using TermBullet.Bootstrap;

namespace TermBullet.Tests.Bootstrap;

public sealed class SmokeTests
{
    [Fact]
    public void Test_project_is_configured()
    {
        Assert.True(true);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("-h")]
    [InlineData("--version")]
    [InlineData("-v")]
    [InlineData("add", "--help")]
    public void IsInformationalCliRequest_detects_help_and_version(string first, string? second = null)
    {
        var args = second is null ? [first] : new[] { first, second };

        Assert.True(TermBulletBootstrap.IsInformationalCliRequest(args));
    }

    [Fact]
    public void IsInformationalCliRequest_ignores_operational_commands()
    {
        Assert.False(TermBulletBootstrap.IsInformationalCliRequest(["add", "Fix auth"]));
    }
}
