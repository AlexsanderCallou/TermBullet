namespace TermBullet.Application.Items;

public sealed class TagItemRequest
{
    public required string PublicRef { get; init; }

    public required string Tag { get; init; }
}
