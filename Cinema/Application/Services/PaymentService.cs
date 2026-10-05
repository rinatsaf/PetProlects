using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Payments;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Serilog;

namespace Application.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    ITicketRepository ticketRepository,
    ITransactionManager transactionManager,
    IYooKassaPaymentGateway yooKassaPaymentGateway,
    ITicketEmailService ticketEmailService,
    ICurrentUserService currentUserService,
    IMapper mapper) : IPaymentService
{
    private static readonly ILogger BusinessLogger = Log.ForContext("BusinessLog", true);

    public async Task<PaymentDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        var payment = await paymentRepository.GetByIdAsync(id, user, cancellationToken)
                      ?? throw new NotFoundException($"Payment {id} not found");
        return mapper.Map<PaymentDto>(payment);
    }

    public async Task<IReadOnlyList<PaymentDto>> GetAccessibleAsync(CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        var payments = await paymentRepository.GetAccessibleAsync(user, cancellationToken);
        return mapper.Map<IReadOnlyList<PaymentDto>>(payments);
    }

    public async Task<PaymentDto> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        var payment = await paymentRepository.GetByExternalIdAsync(externalId, user, cancellationToken)
                      ?? throw new NotFoundException($"Payment with external id {externalId} not found");
        return mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        
        var order = await orderRepository.GetByIdAsync(request.OrderId, user, cancellationToken)
                    ?? throw new NotFoundException($"Order {request.OrderId} not found");

        EnsureCurrentUserCanAccess(order);
        EnsureOrderCanBePaid(order);

        var existingPendingPayment = order.Payments
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => x.Status == PaymentStatus.Pending && !string.IsNullOrWhiteSpace(x.ConfirmationUrl));
        
        if (existingPendingPayment is not null)
        {
            return mapper.Map<PaymentDto>(existingPendingPayment);
        }

        var gatewayResult = await yooKassaPaymentGateway.CreatePaymentAsync(
            order.Id,
            order.TotalAmount,
            "RUB",
            request.ReturnUrl,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var payment = new Payment
        {
            OrderId = order.Id,
            Order = order,
            Provider = "YooKassa",
            ExternalPaymentId = gatewayResult.ExternalPaymentId,
            Amount = order.TotalAmount,
            Currency = "RUB",
            Status = gatewayResult.Status,
            PaymentMethod = gatewayResult.PaymentMethod,
            CreatedByUserId = user.UserId,
            ConfirmationUrl = gatewayResult.ConfirmationUrl,
            RawPayload = gatewayResult.RawPayload,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await paymentRepository.AddAsync(payment, cancellationToken);

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.AwaitingPayment;
            order.UpdatedAt = now;
            await orderRepository.UpdateAsync(order, cancellationToken);
        }

        BusinessLogger.Information(
            "Payment created: PaymentId={PaymentId}, OrderId={OrderId}, ExternalId={ExternalId}, Amount={Amount}, Status={Status}",
            created.Id,
            created.OrderId,
            created.ExternalPaymentId,
            created.Amount,
            created.Status);

        return mapper.Map<PaymentDto>(created);
    }

    public async Task<PaymentDto> UpdateStatusAsync(long id, PaymentStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetByIdAsync(id, GetRepositoryAccessUser(), cancellationToken)
                      ?? throw new NotFoundException($"Payment {id} not found");

        return await ApplyPaymentStateAsync(
            payment,
            request.Status,
            request.ConfirmationUrl,
            request.RawPayload,
            cancellationToken);
    }

    public async Task<PaymentDto> HandleYooKassaWebhookAsync(
        YooKassaWebhookRequest request,
        string rawPayload,
        CancellationToken cancellationToken = default)
    {
        var externalId = request.Payment?.Id;
        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ValidationException("YooKassa webhook does not contain payment id.");
        }

        var payment = await paymentRepository.GetByExternalIdUnsafeAsync(externalId, cancellationToken)
                      ?? throw new NotFoundException($"Payment with external id {externalId} not found");

        var webhookStatus = MapProviderStatus(request.Payment!.Status, request.Event);

        Log.Information(
            "YooKassa webhook received: ExternalId={ExternalId}, RawStatus={RawStatus}, Event={Event}, MappedStatus={MappedStatus}",
            externalId, request.Payment.Status, request.Event, webhookStatus);

        var realStatus = await yooKassaPaymentGateway.GetPaymentStatusAsync(externalId, cancellationToken);

        if (realStatus != webhookStatus)
        {
            BusinessLogger.Warning(
                "YooKassa webhook status mismatch — skipping: ExternalId={ExternalId}, " +
                "WebhookStatus={WebhookStatus}, ApiStatus={ApiStatus}",
                externalId, webhookStatus, realStatus);

            return mapper.Map<PaymentDto>(payment);
        }

        var verifiedUser = new CurrentUserInfo
        {
            IsAuthenticated = true,
            Role = UserRole.Admin
        };

        return await ApplyPaymentStateAsync(
            payment,
            realStatus,
            request.Payment.Confirmation?.ConfirmationUrl,
            rawPayload,
            cancellationToken,
            verifiedUser);
    }

    public async Task<int> SynchronizePendingPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var pendingPayments = await paymentRepository.GetPendingAsync(cancellationToken);
        var syncedCount = 0;

        foreach (var payment in pendingPayments)
        {
            try
            {
                var realStatus = await yooKassaPaymentGateway.GetPaymentStatusAsync(
                    payment.ExternalPaymentId, cancellationToken);

                if (realStatus == payment.Status)
                    continue;

                payment.RawPayload = $"{{\"synchronizedAt\":\"{DateTimeOffset.UtcNow:O}\"}}";
                payment.UpdatedAt = DateTimeOffset.UtcNow;

                var verifiedUser = new CurrentUserInfo
                {
                    IsAuthenticated = true,
                    Role = UserRole.Admin
                };

                await ApplyPaymentStateAsync(
                    payment,
                    realStatus,
                    null,
                    payment.RawPayload,
                    cancellationToken,
                    verifiedUser);

                syncedCount++;
            }
            catch (Exception ex)
            {
                BusinessLogger.Error(ex,
                    "Failed to synchronize payment: ExternalId={ExternalId}",
                    payment.ExternalPaymentId);
            }
        }

        if (syncedCount > 0)
        {
            BusinessLogger.Information(
                "Payment synchronization completed: Synced={SyncedCount}, Pending={PendingCount}",
                syncedCount, pendingPayments.Count);
        }

        return syncedCount;
    }

    private async Task<PaymentDto> ApplyPaymentStateAsync(
        Payment payment,
        PaymentStatus newStatus,
        string? confirmationUrl,
        string? rawPayload,
        CancellationToken cancellationToken,
        CurrentUserInfo? overrideUser = null)
    {
        if (payment.Status == PaymentStatus.Succeeded && newStatus != PaymentStatus.Succeeded)
        {
            return mapper.Map<PaymentDto>(payment);
        }

        var now = DateTimeOffset.UtcNow;
        payment.Status = newStatus;
        payment.ConfirmationUrl = confirmationUrl ?? payment.ConfirmationUrl;
        payment.RawPayload = rawPayload ?? payment.RawPayload;
        payment.UpdatedAt = now;

        if (newStatus == PaymentStatus.Succeeded && payment.ConfirmedAt is null)
        {
            payment.ConfirmedAt = now;
        }

        Order? orderToEmail = null;
        Payment? updatedPayment = null;

        await transactionManager.ExecuteAsync(async ct =>
        {
            updatedPayment = await paymentRepository.UpdateAsync(payment, ct);

            var orderUser = overrideUser ?? GetRepositoryAccessUser();
            var order = await orderRepository.GetByIdAsync(payment.OrderId, orderUser, ct)
                        ?? throw new NotFoundException($"Order {payment.OrderId} not found");

            if (newStatus == PaymentStatus.Succeeded)
            {
                if (order.Status != OrderStatus.Paid)
                {
                    order.Status = OrderStatus.Paid;
                    order.PaidAt = order.PaidAt ?? now;
                    order.UpdatedAt = now;
                }

                foreach (var ticket in order.Tickets.Where(x => x.Status == TicketStatus.Reserved))
                {
                    ticket.Status = TicketStatus.Active;
                    ticket.UpdatedAt = now;
                }

                await ticketRepository.UpdateRangeAsync(order.Tickets, ct);
                await orderRepository.UpdateAsync(order, ct);
                orderToEmail = order;
            }
            else if (newStatus is PaymentStatus.Cancelled or PaymentStatus.Failed)
            {
                if (order.Status != OrderStatus.Cancelled)
                {
                    order.Status = OrderStatus.Cancelled;
                    order.UpdatedAt = now;
                }

                if (order.Tickets.Count > 0)
                {
                    ReleaseTickets(order.Tickets, now);
                    await ticketRepository.UpdateRangeAsync(order.Tickets, ct);
                }

                await orderRepository.UpdateAsync(order, ct);
            }
        }, cancellationToken);

        if (orderToEmail is not null)
        {
            try
            {
                await ticketEmailService.SendTicketsAsync(orderToEmail, cancellationToken);
            }
            catch (Exception ex)
            {
                BusinessLogger.Error(
                    ex,
                    "Failed to send tickets email for order {OrderId}. Payment state is already committed.",
                    orderToEmail.Id);
            }
        }

        BusinessLogger.Information(
            "Payment status applied: PaymentId={PaymentId}, OrderId={OrderId}, NewStatus={NewStatus}",
            payment.Id,
            payment.OrderId,
            newStatus);

        return mapper.Map<PaymentDto>(updatedPayment!);
    }

    private void EnsureCurrentUserCanAccess(Order order)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (!currentUser.IsAuthenticated || currentUser.IsAdmin || currentUser.IsCashier)
        {
            return;
        }

        if (order.UserId != currentUser.UserId)
        {
            throw new ForbiddenException("You cannot pay for another user's order.");
        }
    }

    private static void EnsureOrderCanBePaid(Order order)
    {
        if (order.Status == OrderStatus.Paid)
        {
            throw new ConflictException("Order is already paid.");
        }

        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Expired)
        {
            throw new ConflictException("Order is not available for payment.");
        }
    }

    private static PaymentStatus MapProviderStatus(string status, string eventName)
    {
        var normalizedStatus = status.Trim().ToLowerInvariant();
        return normalizedStatus switch
        {
            "succeeded" => PaymentStatus.Succeeded,
            "canceled" => PaymentStatus.Cancelled,
            "pending" => eventName.Equals("payment.succeeded", StringComparison.OrdinalIgnoreCase)
                ? PaymentStatus.Succeeded
                : PaymentStatus.Pending,
            "waiting_for_capture" => PaymentStatus.Pending,
            _ => PaymentStatus.Failed
        };
    }

    private CurrentUserInfo GetRepositoryAccessUser()
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser.IsAuthenticated)
        {
            return currentUser;
        }

        return new CurrentUserInfo
        {
            IsAuthenticated = true,
            Role = UserRole.Admin
        };
    }

    private static void ReleaseTickets(IEnumerable<Ticket> tickets, DateTimeOffset now)
    {
        foreach (var ticket in tickets)
        {
            ticket.OrderId = null;
            ticket.Order = null;
            ticket.Status = TicketStatus.Available;
            ticket.QrCodeUrl = null;
            ticket.UpdatedAt = now;
        }
    }
}
