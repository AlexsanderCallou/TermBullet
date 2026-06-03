using TermBullet.Domain.Items;

namespace TermBullet.Application.Ai;

public sealed record AiPlanningContextItem(
    string PublicRef,
    ItemType Type,
    ItemStatus Status,
    ItemCollection Collection,
    string Content,
    string? Description,
    string Tag);
