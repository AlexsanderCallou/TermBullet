using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Tests.Infrastructure.Persistence.JsonFiles;

public sealed class LocalSettingsStorePathTests
{
    [Fact]
    public void SettingsPath_returns_expected_location()
    {
        var root = Path.Combine(Path.GetTempPath(), "TermBullet.Tests", Guid.NewGuid().ToString("N"));
        var store = new LocalSettingsStore(root, new SafeJsonFileStore());

        Assert.Equal(Path.Combine(root, "data", "settings.json"), store.SettingsPath);
    }
}
