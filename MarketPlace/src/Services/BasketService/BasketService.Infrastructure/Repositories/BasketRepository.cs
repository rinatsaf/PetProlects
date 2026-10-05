using System.Text.Json;
using BasketService.Application.Common.Abstractions;
using BasketService.Domain;
using StackExchange.Redis;

namespace BasketService.Infrastructure.Repositories;

public sealed class BasketRepository(IConnectionMultiplexer redis) : IBasketRepository
{
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(7);
    
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true
    };

    public async Task<Basket?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(GetKey(userId)).ConfigureAwait(false);
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Basket>(value!, _jsonOptions);
    }

    public async Task UpsertAsync(Basket basket, CancellationToken ct = default)
    {
        var value = JsonSerializer.Serialize(basket, _jsonOptions);
        await _db.StringSetAsync(GetKey(basket.UserId), value, Ttl).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(GetKey(userId)).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.KeyExistsAsync(GetKey(userId)).ConfigureAwait(false);
    }
    
    private static string GetKey(Guid userId) => $"basket:{userId}";
}