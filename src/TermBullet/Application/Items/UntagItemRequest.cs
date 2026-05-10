namespace TermBullet.Application.Items;

public sealed class UntagItemRequest
{
    public required string PublicRef { get; init; }

    public required string Tag { get; init; }
}
