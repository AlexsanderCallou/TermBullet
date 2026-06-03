using TermBullet.Application.Ai;

namespace TermBullet.Services.Ai;

public interface IAiPlanningProvider
{
    Task<AiPlanningProviderResponse> SendAsync(
        AiPlanningModelRequest request,
        CancellationToken cancellationToken = default);
}
