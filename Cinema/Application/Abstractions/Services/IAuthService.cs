using System.Security.Claims;
using Application.DTOs.Auth;

namespace Application.Abstractions.Services;

public interface IAuthService
{
    Task<ClaimsPrincipal> SignInAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ClaimsPrincipal> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
