namespace TermBullet.Core.Items;

public sealed class MigrationInfo
{
    public MigrationInfo(
        string fromPeriod,
        string toPeriod,
        DateTimeOffset migratedAt,
        string reason)
    {
        FromPeriod = NormalizeRequiredText(fromPeriod, nameof(fromPeriod));
        ToPeriod = NormalizeRequiredText(toPeriod, nameof(toPeriod));
        MigratedAt = migratedAt;
        Reason = NormalizeRequiredText(reason, nameof(reason));
    }

    public string FromPeriod { get; }

    public string ToPeriod { get; }

    public DateTimeOffset MigratedAt { get; }

    public string Reason { get; }

    private static string NormalizeRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value must not be empty.", parameterName);
        }

        return value.Trim();
    }
}
