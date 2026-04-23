namespace TermBullet.Infrastructure.Persistence.JsonFiles;

public sealed class MonthlyJsonFilePathResolver(string projectRootPath)
{
    public string ResolveMonthlyFilePath(int year, int month)
    {
        ValidateYear(year);
        ValidateMonth(month);
        return Path.Combine(
            projectRootPath,
            "data",
            year.ToString(),
            $"data_{month:00}_{year:0000}.json");
    }

    public string ResolveBackupFilePath(int year, int month)
    {
        ValidateYear(year);
        ValidateMonth(month);
        return Path.Combine(
            projectRootPath,
            "data",
            year.ToString(),
            $"data_{month:00}_{year:0000}.backup.json");
    }

    private static void ValidateMonth(int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }
    }

    private static void ValidateYear(int year)
    {
        if (year is < 0 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 0 and 9999.");
        }
    }
}
