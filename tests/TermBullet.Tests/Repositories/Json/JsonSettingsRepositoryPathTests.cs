using TermBullet.Repositories.Json;

namespace TermBullet.Tests.Repositories.Json;

public sealed class JsonSettingsRepositoryPathTests
{
    [Fact]
    public void SettingsPath_returns_expected_location()
    {
        var root = Path.Combine(Path.GetTempPath(), "TermBullet.Tests", Guid.NewGuid().ToString("N"));
        var store = new JsonSettingsRepository(root, new JsonFileStore());

        Assert.Equal(Path.Combine(root, "data", "settings.json"), store.SettingsPath);
    }
}
