using TermBullet.Services.Clock;
namespace TermBullet.Services.Clock;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
