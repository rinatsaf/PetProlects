namespace Application.Abstractions.Services;

public interface ISeatHoldService
{
    Task<bool> TryHoldAsync(long sessionId, IEnumerable<long> seatIds, string holdId, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<long>> GetHeldSeatIdsAsync(long sessionId, IEnumerable<long> seatIds, CancellationToken cancellationToken = default);
    Task RenewAsync(long sessionId, IEnumerable<long> seatIds, string holdId, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task ReleaseAsync(long sessionId, IEnumerable<long> seatIds, string holdId, CancellationToken cancellationToken = default);
}
