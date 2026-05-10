using TermBullet.Services.Ids;
namespace TermBullet.Services.Ids;

public interface IIdGenerator
{
    Guid NewId();
}
