using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using TouristRoutes.Interfaces.Services;

namespace TouristRoutes.Services;

public class LoginRateLimiter : ILoginRateLimiter
{
    private readonly IDistributedCache _cache;
    private readonly int _maxAttempts;
    private readonly int _banSeconds;
    private readonly DistributedCacheEntryOptions _window;

    public LoginRateLimiter(IDistributedCache cache, IOptions<RateLimiterOptions> options)
    {
        _cache = cache;
        _maxAttempts = options.Value.MaxAttempts;
        _banSeconds = options.Value.BanSeconds;

        _window = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_banSeconds)
        };
    }

    private static string BuildKey(string key) => $"login:attempts:{key}";

    private static byte[] Serialize<T>(T value)
        => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));

    private static T? Deserialize<T>(byte[]? bytes)
        => bytes == null ? default : JsonSerializer.Deserialize<T>(bytes);
    
    public async Task<bool> IsAllowedAsync(string key)
    {
        var cacheKey = BuildKey(key);
        var data = await _cache.GetAsync(cacheKey);
        var attempts = Deserialize<int?>(data) ?? 0;

        return attempts < _maxAttempts;
    }

    public async Task RegisterFailureAsync(string key)
    {
        var cacheKey = BuildKey(key);
        var data = await _cache.GetAsync(cacheKey);
        var attempts = Deserialize<int?>(data) ?? 0;

        attempts++;

        await _cache.SetAsync(cacheKey, Serialize(attempts), _window);
    }

    public Task ResetAsync(string key)
    {
        var cacheKey = BuildKey(key);
        return _cache.RemoveAsync(cacheKey);
    }
}