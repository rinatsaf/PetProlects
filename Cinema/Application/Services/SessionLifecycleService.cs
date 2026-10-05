using Application.Abstractions.Repositories;
using Application.Abstractions.Services;

namespace Application.Services;

public sealed class SessionLifecycleService(ISessionRepository sessionRepository) : ISessionLifecycleService
{
    private readonly ISessionRepository _sessionRepository = sessionRepository;

    public async Task<(int Activated, int Finished)> AdvanceStatusesAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var activated = await _sessionRepository.ActivatePlannedAsync(now, cancellationToken);
        var finished = await _sessionRepository.FinishActiveAsync(now, cancellationToken);
        return (activated, finished);
    }
}
