namespace TermBullet.Application.Items;

public sealed class ItemNotFoundException(string publicRef)
    : InvalidOperationException($"Item not found: {publicRef}.")
{
    public string PublicRef { get; } = publicRef;
}
