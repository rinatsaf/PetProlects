using Application.Abstractions.Security;
using Application.Exceptions;
using StackExchange.Redis;

namespace Infrastructure.Security;

public sealed class RedisLoginRateLimiter(IConnectionMultiplexer multiplexer) : ILoginRateLimiter
{
    private readonly IDatabase _db = multiplexer.GetDatabase();
    
    private const int EmailThreshold = 5;
    private const int IpThreshold = 20;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    public async Task EnsureNotLimitedAsync(string email, string ip, CancellationToken cancellationToken = default)
    {
        if (await IsLimitedAsync(GetEmailKey(email), EmailThreshold) ||
            await IsLimitedAsync(GetIpKey(ip), IpThreshold))
        {
            throw new RateLimitExceededException("Too many login attempts. Try again later.");
        }
    }

    public async Task RegisterFailureAsync(string email, string ip, CancellationToken cancellationToken = default)
    {
        await IncrementAsync(GetEmailKey(email));
        await IncrementAsync(GetIpKey(ip));
    }

    public async Task ResetAsync(string email, string ip, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(GetEmailKey(email));
        await _db.KeyDeleteAsync(GetIpKey(ip));
    }

    private static string GetEmailKey(string email) => $"login:fail:email:{email.ToLowerInvariant()}";
    private static string GetIpKey(string ip) => $"login:fail:ip:{ip}";

    private async Task<bool> IsLimitedAsync(string key, int threshold)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) return false;

        return int.TryParse(value.ToString(), out var count) && count >= threshold;
    }

    private async Task IncrementAsync(string key)
    {
        var count = await _db.StringIncrementAsync(key);

        if (count == 1)
        {
            await _db.KeyExpireAsync(key, Window, 
                ExpireWhen.HasNoExpiry, 
                CommandFlags.FireAndForget);
        }
    }
}
