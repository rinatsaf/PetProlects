using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Payments;
using Application.Exceptions;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.Tests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ITicketRepository> _ticketRepo = new();
    private readonly Mock<ITransactionManager> _tm = new();
    private readonly Mock<IYooKassaPaymentGateway> _gateway = new();
    private readonly Mock<ITicketEmailService> _email = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _tm.Setup(x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Callback<Func<CancellationToken, Task>, CancellationToken>((fn, ct) => fn(ct).GetAwaiter().GetResult())
            .Returns(Task.CompletedTask);

        _sut = new PaymentService(_paymentRepo.Object, _orderRepo.Object, _ticketRepo.Object,
            _tm.Object, _gateway.Object, _email.Object, _currentUser.Object, _mapper.Object);
    }

    private static readonly CurrentUserInfo Admin = new() { IsAuthenticated = true, UserId = 1, Role = UserRole.Admin };
    private static CurrentUserInfo Customer(long id = 3) => new() { IsAuthenticated = true, UserId = id, Role = UserRole.Customer };

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsPayment()
    {
        var payment = new Payment { Id = 5, ExternalPaymentId = "e1", Order = new Order { Id = 1 } };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(Admin);
        _paymentRepo.Setup(x => x.GetByIdAsync(5, Admin, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _mapper.Setup(x => x.Map<PaymentDto>(payment)).Returns(new PaymentDto { Id = 5 });

        var result = await _sut.GetByIdAsync(5);
        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(Admin);
        _paymentRepo.Setup(x => x.GetByIdAsync(99, Admin, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(99));
    }

    [Fact]
    public async Task GetAccessibleAsync_ReturnsPayments()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(Admin);
        _paymentRepo.Setup(x => x.GetAccessibleAsync(Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Payment>());
        _mapper.Setup(x => x.Map<IReadOnlyList<PaymentDto>>(It.IsAny<List<Payment>>()))
            .Returns(new List<PaymentDto>());

        var result = await _sut.GetAccessibleAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_WhenOrderNotFound_ThrowsNotFound()
    {
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(Customer(3));
        _orderRepo.Setup(x => x.GetByIdAsync(99, Customer(3), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(new CreatePaymentRequest { OrderId = 99, ReturnUrl = "https://ex.com" }));
    }

    [Fact]
    public async Task CreateAsync_WithExistingPendingPayment_ReturnsExisting()
    {
        var user = Customer(3);
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(user);
        _orderRepo.Setup(x => x.GetByIdAsync(1, user, It.IsAny<CancellationToken>())).ReturnsAsync(
            new Order
            {
                Id = 1, UserId = 3, Status = OrderStatus.Pending, TotalAmount = 100,
                Payments = new List<Payment>
                {
                    new()
                    {
                        Id = 10, ExternalPaymentId = "ext-old", Status = PaymentStatus.Pending,
                        ConfirmationUrl = "https://confirm.me", CreatedAt = DateTimeOffset.UtcNow,
                        Order = new Order { Id = 1 }
                    }
                }
            });
        _mapper.Setup(x => x.Map<PaymentDto>(It.IsAny<Payment>())).Returns(new PaymentDto { Id = 10 });

        var result = await _sut.CreateAsync(new CreatePaymentRequest { OrderId = 1, ReturnUrl = "https://ex.com" });
        Assert.Equal(10, result.Id);
        _gateway.Verify(x => x.CreatePaymentAsync(It.IsAny<long>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_AdminCanPayForAnyUser()
    {
        var order = new Order { Id = 1, UserId = 5, Status = OrderStatus.Pending, TotalAmount = 200 };
        _currentUser.Setup(x => x.GetCurrentUser()).Returns(Admin);
        _orderRepo.Setup(x => x.GetByIdAsync(1, Admin, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _gateway.Setup(x => x.CreatePaymentAsync(1, 200, "RUB", "https://ex.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new YooKassaCreatePaymentResult { ExternalPaymentId = "ext-1", Status = PaymentStatus.Pending, PaymentMethod = "bank_card", RawPayload = "{}" });
        _paymentRepo.Setup(x => x.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Payment { Id = 20, ExternalPaymentId = "ext-new", Order = new Order { Id = 1 } });
        _mapper.Setup(x => x.Map<PaymentDto>(It.IsAny<Payment>())).Returns(new PaymentDto { Id = 20 });

        var result = await _sut.CreateAsync(new CreatePaymentRequest { OrderId = 1, ReturnUrl = "https://ex.com" });
        Assert.Equal(20, result.Id);
    }

    [Fact]
    public async Task HandleYooKassaWebhookAsync_WithoutPaymentId_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.HandleYooKassaWebhookAsync(new YooKassaWebhookRequest { Event = "payment.succeeded" }, "{}"));
    }

    [Fact]
    public async Task HandleYooKassaWebhookAsync_WhenPaymentNotFound_ThrowsNotFound()
    {
        _paymentRepo.Setup(x => x.GetByExternalIdUnsafeAsync("ext-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.HandleYooKassaWebhookAsync(
                new YooKassaWebhookRequest
                {
                    Event = "payment.succeeded",
                    Payment = new YooKassaWebhookPayment { Id = "ext-1", Status = "succeeded" }
                }, "{}"));
    }
}
