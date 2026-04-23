using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Core.Refs;

public sealed class PublicRefGeneratorTests
{
    [Theory]
    [InlineData(ItemType.Task, 0, "t-0426-1")]
    [InlineData(ItemType.Note, 2, "n-0426-3")]
    [InlineData(ItemType.Event, 9, "e-0426-10")]
    public void Next_returns_public_ref_after_current_sequence(
        ItemType itemType,
        int currentSequence,
        string expected)
    {
        var publicRef = PublicRefGenerator.Next(itemType, month: 4, year: 2026, currentSequence);

        Assert.Equal(expected, publicRef.Value);
        Assert.Equal(currentSequence + 1, publicRef.Sequence);
    }

    [Fact]
    public void Next_rejects_negative_current_sequence()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => PublicRefGenerator.Next(ItemType.Task, month: 4, year: 2026, currentSequence: -1));

        Assert.Equal("currentSequence", exception.ParamName);
    }

    [Fact]
    public void Next_rejects_invalid_item_type()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => PublicRefGenerator.Next((ItemType)99, month: 4, year: 2026, currentSequence: 0));

        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void Next_rejects_invalid_month()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => PublicRefGenerator.Next(ItemType.Task, month: 13, year: 2026, currentSequence: 0));

        Assert.Equal("month", exception.ParamName);
    }
}
