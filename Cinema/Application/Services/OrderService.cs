using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Orders;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    ISeatRepository seatRepository,
    ITicketRepository ticketRepository,
    ISeatHoldService seatHoldService,
    IMapper mapper,
    ICurrentUserService currentUserService) : IOrderService
{
    public async Task<OrderDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Order {id} not found");
        return mapper.Map<OrderDto>(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetByUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetByUserAsync(userId, cancellationToken);
        return mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = currentUserService.GetCurrentUser();
        if (currentUser.UserId != request.UserId)
        {
            throw new ForbiddenException("You cannot create orders for another user.");
        }

        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw new NotFoundException($"User {request.UserId} not found");

        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken)
                     ?? throw new NotFoundException($"Session {request.SessionId} not found");

        var seatIds = request.SeatIds.Distinct().ToArray();
        var blockingTickets = await ticketRepository.GetBlockingTicketsAsync(request.SessionId, seatIds, cancellationToken);
        if (blockingTickets.Count > 0)
        {
            throw new ConflictException(BuildSeatUnavailableMessage(blockingTickets));
        }

        var holdTtl = TimeSpan.FromMinutes(10);
        var holdOk = await seatHoldService.TryHoldAsync(request.SessionId, seatIds, request.HoldId, holdTtl, cancellationToken);
        if (!holdOk)
        {
            var heldSeatIds = await seatHoldService.GetHeldSeatIdsAsync(request.SessionId, seatIds, cancellationToken);
            var heldSeats = await seatRepository.GetByIdsAsync(heldSeatIds, cancellationToken);
            throw new ConflictException(BuildHeldSeatsMessage(heldSeats));
        }

        var order = mapper.Map<Order>(request);
        order.UserId = user.Id;
        order.Status = OrderStatus.Pending;
        order.ExpiresAt ??= DateTimeOffset.UtcNow.Add(holdTtl);
        order.CreatedAt = DateTimeOffset.UtcNow;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        var ticketPrice = request.TotalAmount / seatIds.Length;
        var tickets = seatIds.Select(seatId => new Ticket
        {
            Order = order,
            SessionId = session.Id,
            SeatId = seatId,
            Price = ticketPrice,
            TicketCode = Guid.NewGuid().ToString("N"),
            Status = TicketStatus.Reserved,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        }).ToList();

        try
        {
            var created = await orderRepository.AddWithTicketsAsync(order, tickets, cancellationToken);
            return mapper.Map<OrderDto>(created);
        }
        catch
        {
            await seatHoldService.ReleaseAsync(request.SessionId, seatIds, request.HoldId, cancellationToken);
            throw;
        }
    }

    public async Task<OrderDto> MarkPaidAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Order {id} not found");

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTimeOffset.UtcNow;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await orderRepository.UpdateAsync(order, cancellationToken);
        return mapper.Map<OrderDto>(updated);
    }

    public async Task<OrderDto> CancelAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException($"Order {id} not found");

        if (order.Status == OrderStatus.Paid)
        {
            throw new ConflictException("Paid orders cannot be cancelled through this endpoint.");
        }

        if (order.Tickets.Count > 0)
        {
            await ticketRepository.DeleteRangeAsync(order.Tickets, cancellationToken);
            order.Tickets.Clear();
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await orderRepository.UpdateAsync(order, cancellationToken);
        return mapper.Map<OrderDto>(updated);
    }

    private static string BuildSeatUnavailableMessage(IReadOnlyCollection<Ticket> blockingTickets)
    {
        var reservedSeats = blockingTickets
            .Where(x => x.Status == TicketStatus.Reserved)
            .Select(x => x.Seat)
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.RowNumber)
            .ThenBy(x => x.SeatNumber)
            .ToArray();
        var purchasedSeats = blockingTickets
            .Where(IsPurchasedSeatStatus)
            .Select(x => x.Seat)
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.RowNumber)
            .ThenBy(x => x.SeatNumber)
            .ToArray();

        if (reservedSeats.Length > 0 && purchasedSeats.Length > 0)
        {
            return $"Already purchased: {FormatSeats(purchasedSeats)}. Temporarily reserved: {FormatSeats(reservedSeats)}.";
        }

        if (purchasedSeats.Length > 0)
        {
            return $"These seats have already been purchased: {FormatSeats(purchasedSeats)}.";
        }

        return BuildHeldSeatsMessage(reservedSeats);
    }

    private static bool IsPurchasedSeatStatus(Ticket ticket)
    {
        return ticket.Status is TicketStatus.Active or TicketStatus.Used;
    }

    private static string BuildHeldSeatsMessage(IReadOnlyCollection<Seat> seats)
    {
        if (seats.Count == 0)
        {
            return "One or more seats are currently reserved by another customer.";
        }

        return $"These seats are currently reserved by another customer: {FormatSeats(seats)}. Please try again after 10 minutes.";
    }

    private static string FormatSeats(IEnumerable<Seat> seats)
    {
        return string.Join(
            ", ",
            seats
                .OrderBy(x => x.RowNumber)
                .ThenBy(x => x.SeatNumber)
                .Select(x => $"row {x.RowNumber}, seat {x.SeatNumber}"));
    }
}
