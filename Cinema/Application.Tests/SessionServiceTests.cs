using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Sessions;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private readonly Mock<IMovieRepository> _movieRepo = new();
    private readonly Mock<IHallRepository> _hallRepo = new();
    private readonly Mock<IRecommendationService> _recommendation = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ISeatHoldService> _seatHoldService = new();
    private readonly SessionService _sut;

    public SessionServiceTests()
    {
        _sut = new SessionService(_sessionRepo.Object, _movieRepo.Object, _hallRepo.Object,
            _recommendation.Object, _mapper.Object, _currentUser.Object,  _seatHoldService.Object);
    }

    private static readonly CurrentUserInfo AdminUser = new() { IsAuthenticated = true, UserId = 1, Role = UserRole.Admin };
    private static CurrentUserInfo CashierUser(long id = 2) => new() { IsAuthenticated = true, UserId = id, Role = UserRole.Cashier };
    private static CurrentUserInfo CustomerUser => new() { IsAuthenticated = true, UserId = 3, Role = UserRole.Customer };

    [Fact]
    public async Task GetUpcomingAsync_ReturnsSessions()
    {
        var sessions = new List<Session>
        {
            new()
            {
                Id = 1,
                Movie = new Movie { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" },
                Hall = new Hall { Id = 1, Name = "H", Address = "S", Type = "S" }
            }
        };
        _sessionRepo.Setup(x => x.GetAllUpcomingAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sessions);
        _mapper.Setup(x => x.Map<IReadOnlyList<SessionDto>>(sessions))
            .Returns(new List<SessionDto> { new() { Id = 1 } });

        var result = await _sut.GetUpcomingAsync();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsSession()
    {
        var session = new Session
        {
            Id = 1,
            Movie = new Movie { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" },
            Hall = new Hall { Id = 1, Name = "H", Address = "S", Type = "S" }
        };
        _sessionRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mapper.Setup(x => x.Map<SessionDto>(session)).Returns(new SessionDto { Id = 1 });

        var result = await _sut.GetByIdAsync(1);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _sessionRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_WhenCustomer_ThrowsForbidden()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser);
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.CreateAsync(new CreateSessionRequest { MovieId = 1, HallId = 1, StartTime = DateTimeOffset.UtcNow, EndTime = DateTimeOffset.UtcNow.AddHours(2), BasePrice = 100 }));
    }

    [Fact]
    public async Task CreateAsync_WhenMovieNotFound_ThrowsNotFound()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _movieRepo.Setup(x => x.ExistsByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(new CreateSessionRequest { MovieId = 99, HallId = 1, StartTime = DateTimeOffset.UtcNow, EndTime = DateTimeOffset.UtcNow.AddHours(2), BasePrice = 100 }));
    }

    [Fact]
    public async Task CreateAsync_WhenHallNotFound_ThrowsNotFound()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _movieRepo.Setup(x => x.ExistsByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _hallRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Hall?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(new CreateSessionRequest { MovieId = 1, HallId = 99, StartTime = DateTimeOffset.UtcNow, EndTime = DateTimeOffset.UtcNow.AddHours(2), BasePrice = 100 }));
    }

    [Fact]
    public async Task CreateAsync_WithOverlap_ThrowsConflict()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _movieRepo.Setup(x => x.ExistsByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _hallRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Hall { Id = 1, Name = "H", Address = "S", Type = "S" });
        _sessionRepo.Setup(x => x.HasOverlapAsync(1, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateSessionRequest { MovieId = 1, HallId = 1, StartTime = DateTimeOffset.UtcNow, EndTime = DateTimeOffset.UtcNow.AddHours(2), BasePrice = 100 }));
    }

    [Fact]
    public async Task UpdateAsync_CashierCannotUpdateOtherStaffsSession()
    {
        var session = new Session
        {
            Id = 1, CreatedByUserId = 99,
            Movie = new Movie { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" },
            Hall = new Hall { Id = 1, Name = "H", Address = "S", Type = "S" }
        };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CashierUser(2));
        _sessionRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.UpdateAsync(1, new UpdateSessionRequest { MovieId = 1, HallId = 1, StartTime = DateTimeOffset.UtcNow, EndTime = DateTimeOffset.UtcNow.AddHours(2), BasePrice = 100, Status = 0 }));
    }

    [Fact]
    public async Task UpdateAsync_WithHallChange_ThrowsConflict()
    {
        var session = new Session
        {
            Id = 1, CreatedByUserId = 1, HallId = 1,
            Movie = new Movie { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" },
            Hall = new Hall { Id = 1, Name = "H", Address = "S", Type = "S" }
        };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _sessionRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.UpdateAsync(1, new UpdateSessionRequest { MovieId = 1, HallId = 2, StartTime = DateTimeOffset.UtcNow, EndTime = DateTimeOffset.UtcNow.AddHours(2), BasePrice = 100, Status = 0 }));
    }

    [Fact]
    public async Task DeleteAsync_WithSoldTickets_ThrowsConflict()
    {
        var session = new Session
        {
            Id = 1,
            CreatedByUserId = 1,
            Movie = new Movie { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" },
            Hall = new Hall { Id = 1, Name = "H", Address = "S", Type = "S" },
            Tickets = new List<Ticket> { new() { TicketCode = "T1", Status = TicketStatus.Active } }
        };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _sessionRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.DeleteAsync(1));
    }

    [Fact]
    public async Task DeleteAsync_WithOnlyAvailableTickets_Deletes()
    {
        var session = new Session
        {
            Id = 1,
            CreatedByUserId = 1,
            Movie = new Movie { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" },
            Hall = new Hall { Id = 1, Name = "H", Address = "S", Type = "S" },
            Tickets = new List<Ticket> { new() { TicketCode = "T1", Status = TicketStatus.Available } }
        };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _sessionRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _sessionRepo.Setup(x => x.DeleteAsync(session, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mapper.Setup(x => x.Map<SessionDto>(session)).Returns(new SessionDto { Id = 1 });

        var result = await _sut.DeleteAsync(1);
        Assert.Equal(1, result.Id);
    }
}
