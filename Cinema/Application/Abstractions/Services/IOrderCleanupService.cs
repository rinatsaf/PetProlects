namespace Application.Abstractions.Services;

public interface IOrderCleanupService
{
    Task<int> CancelExpiredAsync(CancellationToken cancellationToken = default);
}
