using Application.Abstractions.Security;
using Application.Exceptions;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Security;

public sealed class RedisLoginRateLimiter(
    IConnectionMultiplexer multiplexer,
    IOptionsSnapshot<LoginRateLimitOptions> options)
    : ILoginRateLimiter
{
    private readonly IDatabase _db = multiplexer.GetDatabase();
    private readonly int _emailThreshold = options.Value.EmailThreshold;
    private readonly TimeSpan _window = TimeSpan.FromMinutes(options.Value.WindowMinutes);

    public async Task EnsureNotLimitedAsync(string email, CancellationToken cancellationToken = default)
    {
        if (await IsLimitedAsync(GetEmailKey(email)))
        {
            throw new RateLimitExceededException("Too many login attempts. Try again later.");
        }
    }

    public async Task RegisterFailureAsync(string email, CancellationToken cancellationToken = default)
    {
        await IncrementAsync(GetEmailKey(email));
    }

    public async Task ResetAsync(string email, CancellationToken cancellationToken = default)
    {
        await _db.KeyDeleteAsync(GetEmailKey(email));
    }

    private static string GetEmailKey(string email) => $"login:fail:email:{email.ToLowerInvariant()}";

    private async Task<bool> IsLimitedAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) 
            return false;

        return int.TryParse(value.ToString(), out var count) && count >= _emailThreshold;
    }

    private async Task IncrementAsync(string key)
    {
        var count = await _db.StringIncrementAsync(key);

        if (count == 1)
        {
            await _db.KeyExpireAsync(key, _window, 
                ExpireWhen.HasNoExpiry, 
                CommandFlags.FireAndForget);
        }
    }
}
