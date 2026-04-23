namespace TermBullet.Application.Items;

public sealed class DuplicatePublicRefException(string publicRef)
    : InvalidOperationException($"Public ref already exists: {publicRef}.")
{
    public string PublicRef { get; } = publicRef;
}
