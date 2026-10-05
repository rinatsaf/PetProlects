using Application;
using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrderRepository(CinemaDbContext context) : IOrderRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<Order?> GetByIdAsync(long id, CurrentUserInfo info, CancellationToken cancellationToken = default)
    {
        var query = BuildAccessibleOrdersQuery(info, asNoTracking: false)
            .Include(o => o.User)
            .Include(o => o.Tickets)
            .Include(o => o.Payments);

        return await query
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAccessibleAsync(CurrentUserInfo info, long? userId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = BuildAccessibleOrdersQuery(info, asNoTracking: true);
        query = query
            .Include(o => o.Tickets)
            .Include(o => o.Payments);

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        return await query
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
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Order> AddWithTicketsAsync(Order order, IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var ticket in tickets)
        {
            ticket.OrderId = order.Id;
        }

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

    private IQueryable<Order> BuildAccessibleOrdersQuery(CurrentUserInfo info, bool asNoTracking)
    {
        if (!info.IsAuthenticated)
        {
            throw new UnauthorizedAccessException();
        }

        IQueryable<Order> query = _context.Orders;
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (info.IsAdmin)
        {
            return query;
        }

        if (info.IsCashier)
        {
            return query.Where(x =>
                x.UserId == info.UserId ||
                x.Tickets.Any(t => t.Session.CreatedByUserId == info.UserId));
        }

        return query.Where(x => x.UserId == info.UserId);
    }
}


