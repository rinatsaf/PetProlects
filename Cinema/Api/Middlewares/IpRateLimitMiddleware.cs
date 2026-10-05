using System.Globalization;
using StackExchange.Redis;

namespace Api.Middlewares;

public sealed class IpRateLimitMiddleware(RequestDelegate next, IConnectionMultiplexer db)
{
    private readonly IDatabase _db =  db.GetDatabase();
    private const int IpThreshold = 100;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        var key = $"ratelimit:ip:{ip}";
        var count = await _db.StringIncrementAsync(key);
        
        if (count == 1)
        {
            await _db.KeyExpireAsync(key, Window, 
                ExpireWhen.HasNoExpiry, 
                CommandFlags.FireAndForget);
        }
        
        if (count > IpThreshold)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = Window.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            context.Response.ContentType = "application/json";
            
            await context.Response.WriteAsJsonAsync(new { error = $"Too many requests from ip: {ip}. Try again later." });
            return;
        }
        
        await next(context);
    }
}