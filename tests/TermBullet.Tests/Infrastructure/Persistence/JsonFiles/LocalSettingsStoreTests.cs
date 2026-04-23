using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Tests.Infrastructure.Persistence.JsonFiles;

public sealed class LocalSettingsStoreTests
{
    [Fact]
    public async Task SetAsync_persists_value_and_GetAsync_returns_it()
    {
        var root = CreateRoot();
        var store = new LocalSettingsStore(root, new SafeJsonFileStore());

        await store.SetAsync("theme", "dark");
        var value = await store.GetAsync("theme");

        Assert.Equal("dark", value);
    }

    [Fact]
    public async Task SetAsync_separates_values_by_profile()
    {
        var root = CreateRoot();
        var store = new LocalSettingsStore(root, new SafeJsonFileStore());

        await store.SetAsync("theme", "dark");
        await store.SetAsync("theme", "light", profile: "work");

        Assert.Equal("dark", await store.GetAsync("theme"));
        Assert.Equal("light", await store.GetAsync("theme", profile: "work"));
    }

    [Fact]
    public async Task ListAsync_returns_values_for_selected_profile()
    {
        var root = CreateRoot();
        var store = new LocalSettingsStore(root, new SafeJsonFileStore());
        await store.SetAsync("theme", "dark");
        await store.SetAsync("compact_lists", "true");

        var settings = await store.ListAsync();

        Assert.Equal(2, settings.Count);
        Assert.Equal("dark", settings["theme"]);
        Assert.Equal("true", settings["compact_lists"]);
    }

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "TermBullet.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
