using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Users;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUserActiveCacheService> _activeCache = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepo.Object, _currentUser.Object, _activeCache.Object, _mapper.Object);
    }

    private static User MakeUser(long id, string email = "a@b.com", UserRole role = UserRole.Customer) => new()
    {
        Id = id,
        Email = email,
        PasswordHash = "h",
        FirstName = "F",
        LastName = "L",
        Role = role,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task GetProfileAsync_WhenAuthenticated_ReturnsProfile()
    {
        var user = MakeUser(1);
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(new CurrentUserInfo { IsAuthenticated = true, UserId = 1 });
        _userRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mapper.Setup(x => x.Map<UserDto>(user)).Returns(new UserDto { Id = 1, Email = "a@b.com" });

        var result = await _sut.GetProfileAsync(CancellationToken.None);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserNotFound_ThrowsNotFound()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(new CurrentUserInfo { IsAuthenticated = true, UserId = 99 });
        _userRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetProfileAsync(CancellationToken.None));
    }

    [Fact]
    public async Task UpdateRoleAsync_WhenUserExists_UpdatesRole()
    {
        var user = MakeUser(1);
        _userRepo.Setup(x => x.ExistsByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(x => x.UpdateRoleAsync(1, UserRole.Cashier, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mapper.Setup(x => x.Map<UserDto>(user)).Returns(new UserDto { Id = 1, Role = UserRole.Cashier });

        var result = await _sut.UpdateRoleAsync(1, new UpdateUserRoleRequest { Role = UserRole.Cashier });
        Assert.Equal(UserRole.Cashier, result.Role);
    }

    [Fact]
    public async Task UpdateRoleAsync_WhenUserMissing_ThrowsNotFound()
    {
        _userRepo.Setup(x => x.ExistsByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateRoleAsync(99, new UpdateUserRoleRequest { Role = UserRole.Cashier }));
    }

    [Fact]
    public async Task UpdateRoleAsync_WhenRepositoryReturnsFalse_ThrowsConflict()
    {
        _userRepo.Setup(x => x.ExistsByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(x => x.UpdateRoleAsync(1, UserRole.Cashier, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.UpdateRoleAsync(1, new UpdateUserRoleRequest { Role = UserRole.Cashier }));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenUserExists_UpdatesStatus()
    {
        var user = MakeUser(1);
        _userRepo.Setup(x => x.ExistsByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(x => x.UpdateStatusAsync(1, false, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _activeCache.Setup(x => x.InvalidateAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mapper.Setup(x => x.Map<UserDto>(user)).Returns(new UserDto { Id = 1, IsActive = false });

        var result = await _sut.UpdateStatusAsync(1, new UpdateUserStatusRequest { IsActive = false });
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateStatusAsync_InvalidatesActiveCache()
    {
        var user = MakeUser(1);
        _userRepo.Setup(x => x.ExistsByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(x => x.UpdateStatusAsync(1, false, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mapper.Setup(x => x.Map<UserDto>(user)).Returns(new UserDto { Id = 1 });

        await _sut.UpdateStatusAsync(1, new UpdateUserStatusRequest { IsActive = false });
        _activeCache.Verify(x => x.InvalidateAsync(1, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenRepositoryReturnsFalse_ThrowsConflict()
    {
        _userRepo.Setup(x => x.ExistsByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(x => x.UpdateStatusAsync(1, false, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.UpdateStatusAsync(1, new UpdateUserStatusRequest { IsActive = false }));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenUserMissing_ThrowsNotFound()
    {
        _userRepo.Setup(x => x.ExistsByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateStatusAsync(99, new UpdateUserStatusRequest { IsActive = false }));
    }

    [Fact]
    public async Task GetProfileAsync_UsesCurrentUserId()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(new CurrentUserInfo { IsAuthenticated = true, UserId = 42 });
        _userRepo.Setup(x => x.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(MakeUser(42));

        await _sut.GetProfileAsync(CancellationToken.None);
        _userRepo.Verify(x => x.GetByIdAsync(42, It.IsAny<CancellationToken>()));
    }
}
