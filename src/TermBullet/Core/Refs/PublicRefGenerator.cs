using TermBullet.Core.Items;

namespace TermBullet.Core.Refs;

public static class PublicRefGenerator
{
    public static PublicRef Next(ItemType type, int month, int year, int currentSequence)
    {
        if (currentSequence < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentSequence),
                "Current sequence must not be negative.");
        }

        return PublicRef.Create(type, month, year, currentSequence + 1);
    }
}
