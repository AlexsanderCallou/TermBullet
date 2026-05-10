namespace TermBullet.Application.Tags;

public sealed class CreateTagRequest
{
    public required string Name { get; init; }

    public string? Description { get; init; }
}
