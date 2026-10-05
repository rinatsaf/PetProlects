using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.DTOs.Halls;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.Tests;

public class HallServiceTests
{
    private readonly Mock<IHallRepository> _repo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly HallService _sut;

    public HallServiceTests()
    {
        _sut = new HallService(_repo.Object, _mapper.Object, _currentUser.Object);
    }

    private static readonly CurrentUserInfo AdminUser = new()
    {
        IsAuthenticated = true,
        UserId = 1,
        Role = Domain.Enums.UserRole.Admin
    };

    private static CurrentUserInfo CashierUser(long id = 2) => new()
    {
        IsAuthenticated = true,
        UserId = id,
        Role = Domain.Enums.UserRole.Cashier
    };

    private static CurrentUserInfo CustomerUser => new()
    {
        IsAuthenticated = true,
        UserId = 3,
        Role = Domain.Enums.UserRole.Customer
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllHalls()
    {
        var halls = new List<Hall> { new() { Id = 1, Name = "Main" } };
        _repo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(halls);
        _mapper.Setup(x => x.Map<IReadOnlyList<HallDto>>(halls))
            .Returns(new List<HallDto> { new() { Id = 1 } });

        var result = await _sut.GetAllAsync();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsHall()
    {
        var hall = new Hall { Id = 1, Name = "Main" };
        _repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(hall);
        _mapper.Setup(x => x.Map<HallDto>(hall)).Returns(new HallDto { Id = 1, Name = "Main" });

        var result = await _sut.GetByIdAsync(1);
        Assert.Equal("Main", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Hall?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_WhenCurrentUserIsCustomer_ThrowsForbidden()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.CreateAsync(new CreateHallRequest { Name = "A", Address = "S", RowsCount = 5, SeatsPerRow = 5, Type = "S" }));
    }

    [Fact]
    public async Task CreateAsync_WhenCurrentUserIsAdmin_CreatesHall()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _repo.Setup(x => x.ExistsByNameAsync("NewHall", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _mapper.Setup(x => x.Map<Hall>(It.IsAny<CreateHallRequest>())).Returns(new Hall { Name = "NewHall" });
        _repo.Setup(x => x.AddAsync(It.IsAny<Hall>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Hall { Id = 10, Name = "NewHall" });
        _mapper.Setup(x => x.Map<HallDto>(It.IsAny<Hall>())).Returns(new HallDto { Id = 10 });

        var result = await _sut.CreateAsync(new CreateHallRequest { Name = "NewHall", Address = "S", RowsCount = 5, SeatsPerRow = 5, Type = "S" });
        Assert.Equal(10, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsConflict()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _repo.Setup(x => x.ExistsByNameAsync("Dup", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateHallRequest { Name = "Dup", Address = "S", RowsCount = 5, SeatsPerRow = 5, Type = "S" }));
    }

    [Fact]
    public async Task UpdateAsync_WhenCreatedByOtherCashier_ThrowsForbidden()
    {
        var hall = new Hall { Id = 1, Name = "Old", CreatedByUserId = 99 };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CashierUser(2));
        _repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(hall);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.UpdateAsync(1, new UpdateHallRequest { Name = "New", Address = "S", RowsCount = 5, SeatsPerRow = 5, Type = "S" }));
    }
    

    [Fact]
    public async Task UpdateAsync_WithLayoutChange_ThrowsConflict()
    {
        var hall = new Hall { Id = 1, CreatedByUserId = 1, RowsCount = 5, SeatsPerRow = 5 };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CashierUser(1));
        _repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(hall);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.UpdateAsync(1, new UpdateHallRequest { Name = "N", Address = "S", RowsCount = 10, SeatsPerRow = 10, Type = "S" }));
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ThrowsNotFound()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _repo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Hall?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(99));
    }

    [Fact]
    public async Task DeleteAsync_NonAdminCannotDeleteOtherStaffsHall()
    {
        var hall = new Hall { Id = 1, CreatedByUserId = 99 };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CashierUser(2));
        _repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(hall);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(1));
    }
}
