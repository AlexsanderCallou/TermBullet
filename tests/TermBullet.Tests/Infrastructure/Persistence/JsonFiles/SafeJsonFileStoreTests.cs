using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Tests.Infrastructure.Persistence.JsonFiles;

public sealed class SafeJsonFileStoreTests
{
    [Fact]
    public async Task WriteAsync_creates_file_and_directory_when_missing()
    {
        var dataRoot = CreateTempDirectory();
        var filePath = Path.Combine(dataRoot, "data", "2026", "data_04_2026.json");
        var backupPath = Path.Combine(dataRoot, "data", "2026", "data_04_2026.backup.json");
        var store = new SafeJsonFileStore();

        await store.WriteAsync(filePath, backupPath, """{"version":1}""");

        Assert.True(File.Exists(filePath));
        Assert.Equal("""{"version":1}""", await File.ReadAllTextAsync(filePath));
        Assert.False(File.Exists(backupPath));
    }

    [Fact]
    public async Task WriteAsync_keeps_one_backup_with_previous_version()
    {
        var dataRoot = CreateTempDirectory();
        var filePath = Path.Combine(dataRoot, "data", "2026", "data_04_2026.json");
        var backupPath = Path.Combine(dataRoot, "data", "2026", "data_04_2026.backup.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, """{"version":1}""");
        var store = new SafeJsonFileStore();

        await store.WriteAsync(filePath, backupPath, """{"version":2}""");
        await store.WriteAsync(filePath, backupPath, """{"version":3}""");

        Assert.Equal("""{"version":3}""", await File.ReadAllTextAsync(filePath));
        Assert.Equal("""{"version":2}""", await File.ReadAllTextAsync(backupPath));
    }

    [Fact]
    public async Task ReadOrRecoverAsync_recovers_from_backup_when_main_file_is_corrupted()
    {
        var dataRoot = CreateTempDirectory();
        var filePath = Path.Combine(dataRoot, "data", "2026", "data_04_2026.json");
        var backupPath = Path.Combine(dataRoot, "data", "2026", "data_04_2026.backup.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "{invalid");
        await File.WriteAllTextAsync(backupPath, """{"ok":true}""");
        var store = new SafeJsonFileStore();

        var recovered = await store.ReadOrRecoverAsync(filePath, backupPath);

        Assert.Equal("""{"ok":true}""", recovered);
        Assert.Equal("""{"ok":true}""", await File.ReadAllTextAsync(filePath));
    }

    [Fact]
    public async Task ReadOrRecoverAsync_throws_when_main_file_is_corrupted_and_backup_is_missing()
    {
        var dataRoot = CreateTempDirectory();
        var filePath = Path.Combine(dataRoot, "data", "2026", "data_04_2026.json");
        var backupPath = Path.Combine(dataRoot, "data", "2026", "data_04_2026.backup.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "{invalid");
        var store = new SafeJsonFileStore();

        await Assert.ThrowsAsync<InvalidDataException>(
            () => store.ReadOrRecoverAsync(filePath, backupPath));
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "TermBullet.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
