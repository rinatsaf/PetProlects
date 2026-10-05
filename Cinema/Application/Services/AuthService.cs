using System.Security.Claims;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Auth;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILoginRateLimiter loginRateLimiter) : IAuthService
{
    public async Task<ClaimsPrincipal> SignInAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await loginRateLimiter.EnsureNotLimitedAsync(request.Email, cancellationToken);

        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await loginRateLimiter.RegisterFailureAsync(request.Email, cancellationToken);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        await loginRateLimiter.ResetAsync(request.Email, cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("IsActive", user.IsActive.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        return new ClaimsPrincipal(identity);
    }

    public async Task<ClaimsPrincipal> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ConflictException("User with this email already exists");
        }

        var user = new User
        {
            Email = request.Email.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("IsActive", user.IsActive.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        return new ClaimsPrincipal(identity);
    }
}
