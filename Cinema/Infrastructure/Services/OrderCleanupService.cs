using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Domain.Enums;

namespace Infrastructure.Services;

public sealed class OrderCleanupService(IOrderRepository orderRepository, ITicketRepository ticketRepository) : IOrderCleanupService
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly ITicketRepository _ticketRepository = ticketRepository;

    public async Task<int> CancelExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expired = await _orderRepository.GetExpiredPendingAsync(now, 100, cancellationToken);
        foreach (var order in expired)
        {
            if (order.Tickets.Count > 0)
            {
                await _ticketRepository.DeleteRangeAsync(order.Tickets, cancellationToken);
                order.Tickets.Clear();
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = now;
            await _orderRepository.UpdateAsync(order, cancellationToken);
        }

        return expired.Count;
    }
}
