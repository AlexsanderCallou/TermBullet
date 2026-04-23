using TermBullet.Core.Items;

namespace TermBullet.Core.Refs;

public sealed class PublicRef
{
    private PublicRef(ItemType type, int month, int yearTwoDigits, int sequence)
    {
        Type = type;
        Month = month;
        YearTwoDigits = yearTwoDigits;
        Sequence = sequence;
        Value = $"{GetPrefix(type)}-{month:00}{yearTwoDigits:00}-{sequence}";
    }

    public string Value { get; }

    public ItemType Type { get; }

    public int Month { get; }

    public int YearTwoDigits { get; }

    public int Sequence { get; }

    public static PublicRef Create(ItemType type, int month, int year, int sequence)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        if (year is < 0 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 0 and 9999.");
        }

        if (sequence < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), "Sequence must be greater than zero.");
        }

        return new PublicRef(type, month, year % 100, sequence);
    }

    public static PublicRef Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw Invalid(value);
        }

        var parts = value.Split('-');
        if (parts.Length != 3)
        {
            throw Invalid(value);
        }

        if (!TryGetType(parts[0], out var type))
        {
            throw Invalid(value);
        }

        var period = parts[1];
        if (period.Length != 4 || !period.All(char.IsDigit))
        {
            throw Invalid(value);
        }

        var month = int.Parse(period[..2]);
        var yearTwoDigits = int.Parse(period[2..]);
        if (month is < 1 or > 12)
        {
            throw Invalid(value);
        }

        if (!int.TryParse(parts[2], out var sequence) || sequence < 1)
        {
            throw Invalid(value);
        }

        return new PublicRef(type, month, yearTwoDigits, sequence);
    }

    public override string ToString() => Value;

    private static string GetPrefix(ItemType type) =>
        type switch
        {
            ItemType.Task => "t",
            ItemType.Note => "n",
            ItemType.Event => "e",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported item type.")
        };

    private static bool TryGetType(string prefix, out ItemType type)
    {
        switch (prefix)
        {
            case "t":
                type = ItemType.Task;
                return true;
            case "n":
                type = ItemType.Note;
                return true;
            case "e":
                type = ItemType.Event;
                return true;
            default:
                type = default;
                return false;
        }
    }

    private static ArgumentException Invalid(string? value) =>
        new($"Invalid public ref: '{value}'.", nameof(value));
}
