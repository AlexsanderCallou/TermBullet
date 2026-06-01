using TermBullet.Services.Configuration;

namespace TermBullet.Tests.Services.Configuration;

public sealed class DataDirectoryValidatorTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "termbullet-data-validator-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void ValidateAndPrepare_returns_absolute_path_and_creates_data_directory()
    {
        var dataRoot = Path.Combine(_root, "storage");
        var validator = new DataDirectoryValidator();

        var result = validator.ValidateAndPrepare(dataRoot);

        Assert.Equal(Path.GetFullPath(dataRoot), result);
        Assert.True(Directory.Exists(Path.Combine(result, "data")));
    }

    [Fact]
    public void ValidateAndPrepare_rejects_blank_path()
    {
        var validator = new DataDirectoryValidator();

        var exception = Assert.Throws<ArgumentException>(
            () => validator.ValidateAndPrepare(" "));

        Assert.Contains("Data directory is required", exception.Message);
    }

    [Fact]
    public void ValidateAndPrepare_rejects_existing_file_path()
    {
        Directory.CreateDirectory(_root);
        var filePath = Path.Combine(_root, "not-a-directory");
        File.WriteAllText(filePath, "content");
        var validator = new DataDirectoryValidator();

        var exception = Assert.Throws<InvalidOperationException>(
            () => validator.ValidateAndPrepare(filePath));

        Assert.Contains("Data directory is not writable", exception.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
