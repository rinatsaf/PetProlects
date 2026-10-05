using Application;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Orders;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Serilog;

namespace Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    ISeatRepository seatRepository,
    ITicketRepository ticketRepository,
    ISeatHoldService seatHoldService,
    IPaymentRepository paymentRepository,
    IMapper mapper,
    IRecommendationService recommendationService,
    ICurrentUserService currentUserService,
    IYooKassaPaymentGateway paymentGateway,
    ITransactionManager transactionManager) : IOrderService
{
    private static readonly ILogger BusinessLogger = Log.ForContext("BusinessLog", true);

    public async Task<OrderDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        
        var order = await orderRepository.GetByIdAsync(id, user, cancellationToken)
                    ?? throw new NotFoundException($"Order {id} not found");
        return mapper.Map<OrderDto>(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetAccessibleAsync(CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        var orders = await orderRepository.GetAccessibleAsync(user, null, cancellationToken);
        return mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<IReadOnlyList<OrderDto>> GetByUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        var orders = await orderRepository.GetAccessibleAsync(user, userId, cancellationToken);
        return mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = currentUserService.GetCurrentUser();
        var targetUserId = ResolveTargetUserId(currentUser, request.UserId);

        var user = await userRepository.GetByIdAsync(targetUserId, cancellationToken)
                   ?? throw new NotFoundException($"User {targetUserId} not found");

        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken)
            ?? throw new NotFoundException($"Session {request.SessionId} not found");
        EnsureSessionAvailableForBooking(session);
        
        await recommendationService.TrackInteractionAsync(currentUser.UserId, session.MovieId, InteractionType.Click, cancellationToken);

        var seatIds = request.SeatIds.Distinct().ToArray();
        var blockingTickets = await ticketRepository.GetBlockingTicketsAsync(request.SessionId, seatIds, cancellationToken);
        if (blockingTickets.Count > 0)
        {
            throw new ConflictException(BuildSeatUnavailableMessage(blockingTickets));
        }

        var availableTickets = await ticketRepository.GetAvailableBySessionAndSeatsAsync(request.SessionId, seatIds, cancellationToken);
        if (availableTickets.Count != seatIds.Length)
        {
            throw new ConflictException("One or more selected seats are unavailable for this session.");
        }

        var holdTtl = TimeSpan.FromMinutes(10);
        var holdId = Guid.NewGuid().ToString("N");
        var holdOk = await seatHoldService.TryHoldAsync(request.SessionId, seatIds, holdId, holdTtl, cancellationToken);
        if (!holdOk)
        {
            var heldSeatIds = await seatHoldService.GetHeldSeatIdsAsync(request.SessionId, seatIds, cancellationToken);
            var heldSeats = await seatRepository.GetByIdsAsync(heldSeatIds, cancellationToken);
            throw new ConflictException(BuildHeldSeatsMessage(heldSeats));
        }

        var order = mapper.Map<Order>(request);
        order.UserId = user.Id;
        order.Status = OrderStatus.Pending;
        order.ExpiresAt = DateTimeOffset.UtcNow.Add(holdTtl);
        order.CreatedAt = DateTimeOffset.UtcNow;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        
        var now = DateTimeOffset.UtcNow;
        var tickets = availableTickets
            .OrderBy(t => t.SeatId)
            .ToList();

        foreach (var ticket in tickets)
        {
            var seat = await seatRepository.GetByIdAsync(ticket.SeatId, CancellationToken.None)
                ?? throw new NotFoundException($"Seat {ticket.SeatId} not found");

            var increaseToPrice = CalculateSeatPrice(seat, session.Hall);

            ticket.Order = order;
            ticket.Price = session.BasePrice + increaseToPrice;
            ticket.Status = TicketStatus.Reserved;
            ticket.UpdatedAt = now;
        }
        
        order.TotalAmount = tickets.Sum(t => t.Price);
        if (order.TotalAmount <= 0)
        {
            throw new ConflictException("Order total amount must be greater than zero.");
        }

        try
        {
            var created = await orderRepository.AddWithTicketsAsync(order, tickets, cancellationToken);
            
            await recommendationService.TrackInteractionAsync(currentUser.UserId, session.MovieId, InteractionType.BuyTicket, cancellationToken);
            BusinessLogger.Information(
                "Order created: OrderId={OrderId}, UserId={UserId}, SessionId={SessionId}, Seats={SeatsCount}, TotalAmount={TotalAmount}",
                created.Id,
                created.UserId,
                request.SessionId,
                seatIds.Length,
                created.TotalAmount);
    
            return mapper.Map<OrderDto>(created);
        }
        catch
        {
            await seatHoldService.ReleaseAsync(request.SessionId, seatIds, holdId, cancellationToken);
            throw;
        }
    }

    private decimal CalculateSeatPrice(Seat seat, Hall hall)
    {
        var price = seat.SeatType switch
        {
            "Standard" => 0m,
            "Vip" => 200m,
            _ => 0m
        };
        
        if (seat.RowNumber == hall.RowsCount)
        {
            price -= 50m;
        }
            
        return price;
    }

    public async Task<OrderDto> MarkPaidAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        
        var order = await orderRepository.GetByIdAsync(id, user, cancellationToken)
                    ?? throw new NotFoundException($"Order {id} not found");

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTimeOffset.UtcNow;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await orderRepository.UpdateAsync(order, cancellationToken);
        BusinessLogger.Information("Order marked paid manually: OrderId={OrderId}, ActorUserId={ActorUserId}", order.Id, user.UserId);
        return mapper.Map<OrderDto>(updated);
    }

    public async Task<OrderDto> CancelAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = currentUserService.GetCurrentUser();
        
        var order = await orderRepository.GetByIdAsync(id, user, cancellationToken)
                    ?? throw new NotFoundException($"Order {id} not found");

        if (order.Status == OrderStatus.Paid)
        {
            throw new ConflictException("Paid orders cannot be cancelled through this endpoint.");
        }

        if (order.Tickets.Count > 0)
        {
            ReleaseTickets(order.Tickets, DateTimeOffset.UtcNow);
            await ticketRepository.UpdateRangeAsync(order.Tickets, cancellationToken);
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await orderRepository.UpdateAsync(order, cancellationToken);
        BusinessLogger.Information("Order cancelled: OrderId={OrderId}, ActorUserId={ActorUserId}", order.Id, user.UserId);
        return mapper.Map<OrderDto>(updated);
    }

    public async Task<OrderDto> RefundOrderAsync(long id, CancellationToken cancellationToken)
    {
        var user =  currentUserService.GetCurrentUser();
        var order = await orderRepository.GetByIdAsync(id, user, cancellationToken)
            ?? throw new NotFoundException($"Order {id} not found");
        
        if (order.Status != OrderStatus.Paid)
            throw new ConflictException($"Order {id} not paid");
        
        var firstTicket = order.Tickets.FirstOrDefault()
                          ?? throw new ConflictException($"Order {id} has no tickets");
        var sessionId = firstTicket.SessionId;

        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ??  throw new NotFoundException($"Session {sessionId} not found");
        
        if (session.StartTime <= DateTimeOffset.UtcNow.AddHours(2))
            throw new ConflictException($"Session {sessionId} started less than in 2 hours");
        
        var payment = order.Payments.FirstOrDefault(x => x.Status == PaymentStatus.Succeeded)
                      ?? throw new ConflictException("Cannot refund order");
        
        try
        {
            var youKassaResponse = await paymentGateway.CreateRefundAsync(
                payment.ExternalPaymentId,
                order.TotalAmount,
                payment.Currency,
                $"refund_order_id={order.Id}",
                cancellationToken
            );
        }
        catch (Exception e)
        {
            throw new ExternalServiceException("Unable to create refund");
        }

        await transactionManager.ExecuteAsync(async ct =>
        {
            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTimeOffset.UtcNow;
            await paymentRepository.UpdateAsync(payment, ct);

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            await orderRepository.UpdateAsync(order, ct);

            foreach (var ticket in order.Tickets.Where(t => t.Status == TicketStatus.Active))
            {
                ticket.Status = TicketStatus.Refunded;
                ticket.UpdatedAt = DateTimeOffset.UtcNow;
            }
            await ticketRepository.UpdateRangeAsync(order.Tickets, ct);
        }, cancellationToken);

        BusinessLogger.Information("Order refunded: OrderId={OrderId}", order.Id);
        
        return mapper.Map<OrderDto>(order);
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

    private static void EnsureSessionAvailableForBooking(Session session)
    {
        if (session.Status is SessionStatus.Canceled or SessionStatus.Finished)
        {
            throw new ConflictException("Tickets cannot be purchased for a finished or canceled session.");
        }

        if (session.EndTime <= DateTimeOffset.UtcNow)
        {
            throw new ConflictException("Tickets cannot be purchased after the session has ended.");
        }
    }

    private static long ResolveTargetUserId(CurrentUserInfo currentUser, long? requestedUserId)
    {
        if (currentUser.IsAdmin || currentUser.IsCashier)
        {
            return requestedUserId ?? currentUser.UserId;
        }

        if (requestedUserId.HasValue && currentUser.UserId != requestedUserId.Value)
        {
            throw new ForbiddenException("You cannot create an order for another user.");
        }

        return currentUser.UserId;
    }
}
