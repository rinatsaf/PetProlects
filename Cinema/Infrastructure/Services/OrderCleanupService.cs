using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Domain.Enums;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class OrderCleanupService(
    IOrderRepository orderRepository,
    ITicketRepository ticketRepository,
    IOptionsSnapshot<OrderCleanupOptions> options) : IOrderCleanupService
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly ITicketRepository _ticketRepository = ticketRepository;
    private readonly int _batchSize = options.Value.BatchSize;

    public async Task<int> CancelExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = await _orderRepository.GetExpiredPendingAsync(now, _batchSize, cancellationToken);
        foreach (var order in expired)
        {
            if (order.Tickets.Count > 0)
            {
                ReleaseTickets(order.Tickets, now);
                await _ticketRepository.UpdateRangeAsync(order.Tickets, cancellationToken);
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = now;
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        return expired.Count;
    }

    private static void ReleaseTickets(IEnumerable<Domain.Entities.Ticket> tickets, DateTimeOffset now)
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
