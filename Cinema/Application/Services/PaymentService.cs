using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Payments;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    ITicketRepository ticketRepository,
    IYooKassaPaymentGateway yooKassaPaymentGateway,
    ICurrentUserService currentUserService,
    IMapper mapper) : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly ITicketRepository _ticketRepository = ticketRepository;
    private readonly IYooKassaPaymentGateway _yooKassaPaymentGateway = yooKassaPaymentGateway;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<PaymentDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Payment {id} not found");
        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByExternalIdAsync(externalId, cancellationToken)
                      ?? throw new NotFoundException($"Payment with external id {externalId} not found");
        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
                    ?? throw new NotFoundException($"Order {request.OrderId} not found");

        EnsureCurrentUserCanAccess(order);
        EnsureOrderCanBePaid(order);

        var existingPendingPayment = order.Payments
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => x.Status == PaymentStatus.Pending && !string.IsNullOrWhiteSpace(x.ConfirmationUrl));
        if (existingPendingPayment is not null)
        {
            return _mapper.Map<PaymentDto>(existingPendingPayment);
        }

        var gatewayResult = await _yooKassaPaymentGateway.CreatePaymentAsync(
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
            ConfirmationUrl = gatewayResult.ConfirmationUrl,
            RawPayload = gatewayResult.RawPayload,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _paymentRepository.AddAsync(payment, cancellationToken);

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.AwaitingPayment;
            order.UpdatedAt = now;
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        return _mapper.Map<PaymentDto>(created);
    }

    public async Task<PaymentDto> UpdateStatusAsync(long id, PaymentStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken)
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

        var payment = await _paymentRepository.GetByExternalIdAsync(externalId, cancellationToken)
                      ?? throw new NotFoundException($"Payment with external id {externalId} not found");

        var mappedStatus = MapProviderStatus(request.Payment!.Status, request.Event);
        return await ApplyPaymentStateAsync(
            payment,
            mappedStatus,
            request.Payment.Confirmation?.ConfirmationUrl,
            rawPayload,
            cancellationToken);
    }

    private async Task<PaymentDto> ApplyPaymentStateAsync(
        Payment payment,
        PaymentStatus newStatus,
        string? confirmationUrl,
        string? rawPayload,
        CancellationToken cancellationToken)
    {
        if (payment.Status == PaymentStatus.Succeeded && newStatus != PaymentStatus.Succeeded)
        {
            return _mapper.Map<PaymentDto>(payment);
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

        var updatedPayment = await _paymentRepository.UpdateAsync(payment, cancellationToken);
        var order = await _orderRepository.GetByIdAsync(payment.OrderId, cancellationToken)
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

            await _orderRepository.UpdateAsync(order, cancellationToken);
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
                await _ticketRepository.DeleteRangeAsync(order.Tickets, cancellationToken);
                order.Tickets.Clear();
            }

            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        return _mapper.Map<PaymentDto>(updatedPayment);
    }

    private void EnsureCurrentUserCanAccess(Order order)
    {
        var currentUser = _currentUserService.GetCurrentUser();
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
}
