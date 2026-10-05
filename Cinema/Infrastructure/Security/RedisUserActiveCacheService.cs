using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Security;

public sealed class RedisUserActiveCacheService(
    IConnectionMultiplexer multiplexer,
    IUserRepository userRepository,
    IOptionsSnapshot<ActiveCacheOptions> options)
    : IUserActiveCacheService
{
    private readonly IDatabase _db = multiplexer.GetDatabase();
    private readonly TimeSpan _cacheTtl = TimeSpan.FromSeconds(options.Value.TtlSeconds);

    private static string Key(long userId) => $"user:active:{userId}";

    public async Task<bool> GetActiveAsync(long userId, CancellationToken ct = default)
    {
        var cached = await _db.StringGetAsync(Key(userId));
        if (cached.HasValue)
            return (bool)cached;

        var user = await userRepository.GetByIdAsync(userId, ct);
        var isActive = user?.IsActive ?? false;
        await SetActiveAsync(userId, isActive, ct);
        return isActive;
    }

    public async Task InvalidateAsync(long userId, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(Key(userId));
    }

    private async Task SetActiveAsync(long userId, bool isActive, CancellationToken ct = default)
    {
        await _db.StringSetAsync(Key(userId), isActive, _cacheTtl);
    }
}
