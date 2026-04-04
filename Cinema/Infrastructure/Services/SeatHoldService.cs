using Application.Abstractions.Services;
using StackExchange.Redis;

namespace Infrastructure.Services;

public sealed class SeatHoldService(IConnectionMultiplexer multiplexer) : ISeatHoldService
{
    private readonly IDatabase _db = multiplexer.GetDatabase();

    public async Task<bool> TryHoldAsync(long sessionId, IEnumerable<long> seatIds, string holdId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var keys = seatIds.Select(id => Key(sessionId, id)).ToList();
        var setSucceeded = new List<RedisKey>();

        foreach (var key in keys)
        {
            var ok = await _db.StringSetAsync(key, holdId, ttl, When.NotExists);
            if (!ok)
            {
                // rollback previous holds
                if (setSucceeded.Count > 0)
                {
                    await _db.KeyDeleteAsync(setSucceeded.ToArray());
                }
                return false;
            }
            setSucceeded.Add(key);
        }
        return true;
    }

    public async Task<IReadOnlyList<long>> GetHeldSeatIdsAsync(long sessionId, IEnumerable<long> seatIds, CancellationToken cancellationToken = default)
    {
        var seatIdList = seatIds.Distinct().ToArray();
        var keys = seatIdList.Select(id => (RedisKey)Key(sessionId, id)).ToArray();
        var values = await _db.StringGetAsync(keys);

        return seatIdList
            .Zip(values, (seatId, value) => new { seatId, value })
            .Where(x => x.value.HasValue)
            .Select(x => x.seatId)
            .ToArray();
    }

    public async Task RenewAsync(long sessionId, IEnumerable<long> seatIds, string holdId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        foreach (var key in seatIds.Select(id => Key(sessionId, id)))
        {
            var existing = await _db.StringGetAsync(key);
            if (existing.HasValue && existing.ToString() == holdId)
            {
                await _db.KeyExpireAsync(key, ttl);
            }
        }
    }

    public async Task ReleaseAsync(long sessionId, IEnumerable<long> seatIds, string holdId, CancellationToken cancellationToken = default)
    {
        foreach (var key in seatIds.Select(id => Key(sessionId, id)))
        {
            var existing = await _db.StringGetAsync(key);
            if (existing.HasValue && existing.ToString() == holdId)
            {
                await _db.KeyDeleteAsync(key);
            }
        }
    }

    private static string Key(long sessionId, long seatId) => $"seat-hold:{sessionId}:{seatId}";
}
