namespace TermBullet.Application.Items;

public sealed class EditItemRequest
{
    public required string PublicRef { get; init; }

    public required string Content { get; init; }

    public string? Description { get; init; }
}
