using Application.Abstractions.Security;
using Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static  class DependencyInjectionAuth
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ILoginRateLimiter, RedisLoginRateLimiter>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}
