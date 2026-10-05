namespace Application.Abstractions.Services;

public interface ISessionLifecycleService
{
    Task<(int Activated, int Finished)> AdvanceStatusesAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
}
