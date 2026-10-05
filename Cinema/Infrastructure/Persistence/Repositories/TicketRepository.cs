using Application;
using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TicketRepository(CinemaDbContext context) : ITicketRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<Ticket?> GetByIdAsync(long id, CurrentUserInfo info, CancellationToken cancellationToken = default)
    {
        return await BuildAccessibleTicketsQuery(info, asNoTracking: false)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Ticket?> GetByCodeAsync(string code, CurrentUserInfo info, CancellationToken cancellationToken = default)
    {
        return await BuildAccessibleTicketsQuery(info, asNoTracking: false)
            .FirstOrDefaultAsync(t => t.TicketCode == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetByOrderAsync(long orderId, CurrentUserInfo info, CancellationToken cancellationToken = default)
    {
        return await BuildAccessibleTicketsQuery(info, asNoTracking: true)
            .Where(t => t.OrderId == orderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetAvailableBySessionAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t =>
                t.SessionId == sessionId &&
                t.OrderId == null &&
                t.Status == TicketStatus.Available)
            .OrderBy(t => t.SeatId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetAvailableBySessionAndSeatsAsync(long sessionId, IEnumerable<long> seatIds, CancellationToken cancellationToken = default)
    {
        var seatIdList = seatIds.Distinct().ToArray();
        return await _context.Tickets
            .Where(t =>
                t.SessionId == sessionId &&
                seatIdList.Contains(t.SeatId) &&
                t.OrderId == null &&
                t.Status == TicketStatus.Available)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetBlockingTicketsAsync(long sessionId, IEnumerable<long> seatIds, CancellationToken cancellationToken = default)
    {
        var seatIdList = seatIds.Distinct().ToArray();
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Seat)
            .Where(
            t => t.SessionId == sessionId &&
                 seatIdList.Contains(t.SeatId) &&
                 t.Status != TicketStatus.Available &&
                 t.Status != TicketStatus.Cancelled &&
                 t.Status != TicketStatus.Refunded)
            .OrderBy(t => t.Seat.RowNumber)
            .ThenBy(t => t.Seat.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        await _context.Tickets.AddRangeAsync(tickets, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        _context.Tickets.UpdateRange(tickets);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Ticket> UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    private IQueryable<Ticket> BuildAccessibleTicketsQuery(CurrentUserInfo info, bool asNoTracking)
    {
        if (!info.IsAuthenticated)
        {
            throw new UnauthorizedAccessException();
        }

        IQueryable<Ticket> query = _context.Tickets
            .Include(t => t.Session)
            .Include(t => t.Seat)
            .Include(t => t.Order);

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
            return query.Where(t =>
                t.Order != null &&
                (t.Order.UserId == info.UserId || t.Session.CreatedByUserId == info.UserId));
        }

        return query.Where(t => t.Order != null && t.Order.UserId == info.UserId);
    }
}
