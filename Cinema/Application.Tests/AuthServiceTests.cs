using System.Security.Claims;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.DTOs.Auth;
using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ILoginRateLimiter> _rateLimiter = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepo.Object, _passwordHasher.Object, _rateLimiter.Object);
    }

    private static User MakeUser(bool isActive = true, UserRole role = UserRole.Customer) => new()
    {
        Id = 1,
        Email = "test@test.com",
        PasswordHash = "hash",
        FirstName = "Test",
        LastName = "User",
        Role = role,
        IsActive = isActive,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task SignInAsync_WithValidCredentials_ReturnsClaimsPrincipal()
    {
        var user = MakeUser();
        _userRepo.Setup(x => x.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(x => x.Verify("pass", user.PasswordHash)).Returns(true);

        var result = await _sut.SignInAsync(new LoginRequest { Email = "test@test.com", Password = "pass" });

        Assert.IsType<ClaimsPrincipal>(result);
        Assert.Equal("Test User", result.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("Customer", result.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public async Task SignInAsync_WithInvalidEmail_ThrowsUnauthorized()
    {
        _userRepo.Setup(x => x.GetByEmailAsync("unknown@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.SignInAsync(new LoginRequest { Email = "unknown@test.com", Password = "pass" }));
    }

    [Fact]
    public async Task SignInAsync_WithWrongPassword_RegistersFailureAndThrows()
    {
        var user = MakeUser();
        _userRepo.Setup(x => x.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(x => x.Verify("wrong", user.PasswordHash)).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.SignInAsync(new LoginRequest { Email = "test@test.com", Password = "wrong" }));
        _rateLimiter.Verify(x => x.RegisterFailureAsync("test@test.com", It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task SignInAsync_WithInactiveUser_RegistersFailureAndThrows()
    {
        var user = MakeUser(isActive: false);
        _userRepo.Setup(x => x.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.SignInAsync(new LoginRequest { Email = "test@test.com", Password = "pass" }));
    }

    [Fact]
    public async Task SignInAsync_OnSuccess_ResetsRateLimiter()
    {
        var user = MakeUser();
        _userRepo.Setup(x => x.GetByEmailAsync("test@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(x => x.Verify("pass", user.PasswordHash)).Returns(true);

        await _sut.SignInAsync(new LoginRequest { Email = "test@test.com", Password = "pass" });
        _rateLimiter.Verify(x => x.ResetAsync("test@test.com", It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task SignInAsync_ChecksRateLimiterBeforeLookup()
    {
        _rateLimiter.Setup(x => x.EnsureNotLimitedAsync("test@test.com", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.SignInAsync(new LoginRequest { Email = "test@test.com", Password = "pass" }));
        _rateLimiter.Verify(x => x.EnsureNotLimitedAsync("test@test.com", It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserAndReturnsPrincipal()
    {
        _userRepo.Setup(x => x.ExistsByEmailAsync("new@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(x => x.Hash("Str0ng!Pass")).Returns("hashed");

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            Email = "new@test.com",
            Password = "Str0ng!Pass",
            FirstName = "New",
            LastName = "User"
        });

        Assert.IsType<ClaimsPrincipal>(result);
        _userRepo.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsConflict()
    {
        _userRepo.Setup(x => x.ExistsByEmailAsync("dup@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.RegisterAsync(new RegisterRequest { Email = "dup@test.com", Password = "Str0ng!Pass", FirstName = "A", LastName = "B" }));
    }

    [Fact]
    public async Task RegisterAsync_TrimsSpaces()
    {
        _userRepo.Setup(x => x.ExistsByEmailAsync("trim@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("h");

        await _sut.RegisterAsync(new RegisterRequest
        {
            Email = "  trim@test.com  ",
            Password = "Str0ng!Pass",
            FirstName = "  John  ",
            LastName = "  Doe  "
        });

        _userRepo.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Email == "trim@test.com" &&
            u.FirstName == "John" &&
            u.LastName == "Doe"), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task RegisterAsync_SetsRoleToCustomer()
    {
        _userRepo.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("h");

        var result = await _sut.RegisterAsync(new RegisterRequest
        { Email = "r@t.com", Password = "Str0ng!Pass", FirstName = "A", LastName = "B" });

        Assert.Equal("Customer", result.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public async Task RegisterAsync_SetsIsActiveClaim()
    {
        _userRepo.Setup(x => x.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>())).Returns("h");

        var result = await _sut.RegisterAsync(new RegisterRequest
        { Email = "a@b.com", Password = "Str0ng!Pass", FirstName = "A", LastName = "B" });

        Assert.Equal("True", result.FindFirst("IsActive")?.Value);
    }
}
