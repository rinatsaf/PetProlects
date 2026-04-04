using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrderRepository(CinemaDbContext context) : IOrderRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Tickets)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetExpiredPendingAsync(DateTimeOffset now, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Tickets)
            .Where(o =>
                (o.Status == Domain.Enums.OrderStatus.Pending || o.Status == Domain.Enums.OrderStatus.AwaitingPayment) &&
                o.ExpiresAt != null &&
                o.ExpiresAt < now)
            .OrderBy(o => o.ExpiresAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Order> AddWithTicketsAsync(Order order, IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Tickets.AddRangeAsync(tickets, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return order;
    }

    public async Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }
}
