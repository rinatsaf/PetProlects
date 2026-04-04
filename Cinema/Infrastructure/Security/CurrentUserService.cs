using System.Security.Claims;
using Application;
using Application.Abstractions.Security;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public CurrentUserInfo GetCurrentUser()
    {
        var user = accessor.HttpContext?.User;

        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        long.TryParse(userIdClaim, out var userId);

        var roleClaim = user?.FindFirstValue(ClaimTypes.Role);
        Enum.TryParse<UserRole>(roleClaim, out var role);
        
        return new CurrentUserInfo()
        {
            UserId = userId,
            UserName = user?.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
            Role = role
        };
    }
}