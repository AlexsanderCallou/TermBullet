using TermBullet.Core.Items;
using TermBullet.Core.Refs;

namespace TermBullet.Tests.Core.Refs;

public sealed class PublicRefTests
{
    [Fact]
    public void Create_returns_task_ref_with_month_year_and_sequence()
    {
        var publicRef = PublicRef.Create(ItemType.Task, month: 4, year: 2026, sequence: 1);

        Assert.Equal("t-0426-1", publicRef.Value);
        Assert.Equal(ItemType.Task, publicRef.Type);
        Assert.Equal(4, publicRef.Month);
        Assert.Equal(26, publicRef.YearTwoDigits);
        Assert.Equal(1, publicRef.Sequence);
    }

    [Theory]
    [InlineData(ItemType.Task, "t-0426-7")]
    [InlineData(ItemType.Note, "n-0426-7")]
    [InlineData(ItemType.Event, "e-0426-7")]
    public void Create_uses_expected_prefix_for_item_type(ItemType itemType, string expected)
    {
        var publicRef = PublicRef.Create(itemType, month: 4, year: 2026, sequence: 7);

        Assert.Equal(expected, publicRef.Value);
    }

    [Fact]
    public void Parse_returns_ref_for_valid_value()
    {
        var publicRef = PublicRef.Parse("n-1226-42");

        Assert.Equal("n-1226-42", publicRef.Value);
        Assert.Equal(ItemType.Note, publicRef.Type);
        Assert.Equal(12, publicRef.Month);
        Assert.Equal(26, publicRef.YearTwoDigits);
        Assert.Equal(42, publicRef.Sequence);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("x-0426-1")]
    [InlineData("t-1326-1")]
    [InlineData("t-0026-1")]
    [InlineData("t-426-1")]
    [InlineData("t-0426-0")]
    [InlineData("t-0426--1")]
    [InlineData("t-0426-abc")]
    [InlineData("t-0426")]
    [InlineData("t-04-26-1")]
    public void Parse_rejects_invalid_values(string value)
    {
        var exception = Assert.Throws<ArgumentException>(() => PublicRef.Parse(value));

        Assert.Contains("Invalid public ref", exception.Message);
    }

    [Theory]
    [InlineData(0, 2026, 1)]
    [InlineData(13, 2026, 1)]
    [InlineData(4, 2026, 0)]
    [InlineData(4, 2026, -1)]
    public void Create_rejects_invalid_parts(int month, int year, int sequence)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => PublicRef.Create(ItemType.Task, month, year, sequence));

        Assert.False(string.IsNullOrWhiteSpace(exception.ParamName));
    }
}
