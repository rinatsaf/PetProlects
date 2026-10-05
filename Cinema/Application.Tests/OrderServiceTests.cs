using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Orders;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ISessionRepository> _sessionRepo = new();
    private readonly Mock<ISeatRepository> _seatRepo = new();
    private readonly Mock<ITicketRepository> _ticketRepo = new();
    private readonly Mock<ISeatHoldService> _seatHold = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IRecommendationService> _recommendation = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IYooKassaPaymentGateway> _paymentGateway = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<ITransactionManager> _transactionManager = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _transactionManager.Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> action, CancellationToken ct) => action(ct));

        _sut = new OrderService(
            _orderRepo.Object,
            _userRepo.Object,
            _sessionRepo.Object,
            _seatRepo.Object,
            _ticketRepo.Object,
            _seatHold.Object,
            _paymentRepo.Object,
            _mapper.Object,
            _recommendation.Object,
            _currentUser.Object,
            _paymentGateway.Object,
            _transactionManager.Object);
    }
    
    private static readonly CurrentUserInfo AdminUser = new() { IsAuthenticated = true, UserId = 1, Role = UserRole.Admin };
    private static CurrentUserInfo CustomerUser(long id = 3) => new() { IsAuthenticated = true, UserId = id, Role = UserRole.Customer };

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsOrder()
    {
        var user = new CurrentUserInfo { IsAuthenticated = true, UserId = 3, Role = UserRole.Customer };
        var order = new Order { Id = 1 };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(user);
        _orderRepo.Setup(x => x.GetByIdAsync(1, user, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _mapper.Setup(x => x.Map<OrderDto>(order)).Returns(new OrderDto { Id = 1 });

        var result = await _sut.GetByIdAsync(1);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task CreateOrder_WithVipSeatAndStandartSeat()
    {
        
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        var user = new CurrentUserInfo { IsAuthenticated = true, UserId = 3, Role = UserRole.Customer };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(user);
        _orderRepo.Setup(x => x.GetByIdAsync(99, user, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task GetAccessibleAsync_ReturnsOrders()
    {
        var user = new CurrentUserInfo { IsAuthenticated = true, UserId = 3, Role = UserRole.Customer };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(user);
        _orderRepo.Setup(x => x.GetAccessibleAsync(user, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());
        _mapper.Setup(x => x.Map<IReadOnlyList<OrderDto>>(It.IsAny<List<Order>>()))
            .Returns(new List<OrderDto>());

        var result = await _sut.GetAccessibleAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_CustomerCannotCreateForOtherUser()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser(3));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.CreateAsync(new CreateOrderRequest { UserId = 5, SessionId = 1, SeatIds = new long[] { 1, 2 } }));
    }
    
    [Fact]
    public async Task CreateAsync_WhenSessionFinished_ThrowsConflict()
    {
        var session = new Session
        {
            Id = 1, Status = SessionStatus.Finished, EndTime = DateTimeOffset.UtcNow.AddHours(-1),
            Movie = new Movie { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" },
            Hall = new Hall { Id = 1, Name = "H", Address = "S", Type = "S" }
        };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser(3));
        _userRepo.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 3, Email = "u@t.com", PasswordHash = "h", FirstName = "F", LastName = "L" });
        _sessionRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateOrderRequest { SessionId = 1, SeatIds = new long[] { 1 } }));
    }

    private static User MakeUser(long id) => new()
    {
        Id = id, Email = "u@t.com", PasswordHash = "h", FirstName = "F", LastName = "L"
    };

    private static Hall MakeHall(long id) => new()
    {
        Id = id, Name = "H", Address = "S", Type = "S"
    };

    [Fact]
    public async Task CreateAsync_WithBlockedSeats_ThrowsConflict()
    {
        var session = new Session
        {
            Id = 1, MovieId = 1, Status = SessionStatus.Planned,
            EndTime = DateTimeOffset.UtcNow.AddHours(3), BasePrice = 100,
            Movie = new Movie { Id = 1, Title = "T", Description = "D", AgeRating = "12+", Country = "RU" },
            Hall = MakeHall(1)
        };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(CustomerUser(3));
        _userRepo.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(MakeUser(3));
        _sessionRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _ticketRepo.Setup(x => x.GetBlockingTicketsAsync(1, new long[] { 1 }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Ticket>
            {
                new() { TicketCode = "T1", Status = TicketStatus.Active, Seat = new Seat { Hall = MakeHall(1), SeatType = "S", RowNumber = 1, SeatNumber = 1 } }
            });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(new CreateOrderRequest { SessionId = 1, SeatIds = new long[] { 1 } }));
    }
    

    [Fact]
    public async Task MarkPaidAsync_WhenExists_MarksAsPaid()
    {
        var order = new Order { Id = 1 };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _orderRepo.Setup(x => x.GetByIdAsync(1, AdminUser, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _orderRepo.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _mapper.Setup(x => x.Map<OrderDto>(order)).Returns(new OrderDto { Id = 1 });

        var result = await _sut.MarkPaidAsync(1);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task CancelAsync_PaidOrder_ThrowsConflict()
    {
        var order = new Order { Id = 1, Status = OrderStatus.Paid };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(AdminUser);
        _orderRepo.Setup(x => x.GetByIdAsync(1, AdminUser, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CancelAsync(1));
    }
}
