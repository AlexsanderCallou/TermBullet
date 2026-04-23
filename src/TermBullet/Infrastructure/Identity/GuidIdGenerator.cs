using TermBullet.Application.Ports;

namespace TermBullet.Infrastructure.Identity;

public sealed class GuidIdGenerator : IIdGenerator
{
    public Guid NewId() => Guid.NewGuid();
}
