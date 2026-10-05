namespace Application.Abstractions.Security;

public interface IUserActiveCacheService
{
    Task<bool> GetActiveAsync(long userId, CancellationToken ct = default);
    Task InvalidateAsync(long userId, CancellationToken ct = default);
}