using TermBullet.Services.Ids;
using TermBullet.Repositories.Interfaces;

namespace TermBullet.Services.Ids;

public sealed class GuidIdGenerator : IIdGenerator
{
    public Guid NewId() => Guid.NewGuid();
}
