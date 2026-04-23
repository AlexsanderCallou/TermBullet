namespace TermBullet.Application.Ports;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
