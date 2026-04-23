using TermBullet.Infrastructure.Persistence.JsonFiles;

namespace TermBullet.Tests.Infrastructure.Persistence.JsonFiles;

public sealed class MonthlyJsonFilePathResolverTests
{
    [Fact]
    public void Resolve_monthly_file_path_uses_year_folder_and_expected_filename()
    {
        var resolver = new MonthlyJsonFilePathResolver(@"C:\term-bullet");

        var path = resolver.ResolveMonthlyFilePath(year: 2026, month: 4);

        Assert.Equal(
            Path.Combine(@"C:\term-bullet", "data", "2026", "data_04_2026.json"),
            path);
    }

    [Fact]
    public void Resolve_backup_file_path_uses_expected_backup_filename()
    {
        var resolver = new MonthlyJsonFilePathResolver(@"C:\term-bullet");

        var path = resolver.ResolveBackupFilePath(year: 2026, month: 4);

        Assert.Equal(
            Path.Combine(@"C:\term-bullet", "data", "2026", "data_04_2026.backup.json"),
            path);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Resolve_rejects_invalid_month(int month)
    {
        var resolver = new MonthlyJsonFilePathResolver(@"C:\term-bullet");

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => resolver.ResolveMonthlyFilePath(2026, month));

        Assert.Equal("month", exception.ParamName);
    }
}
