using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TicketRepository(CinemaDbContext context) : ITicketRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<Ticket?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Include(t => t.Session)
            .Include(t => t.Seat)
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetByOrderAsync(long orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.OrderId == orderId)
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

    public async Task<Ticket> UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    public async Task DeleteRangeAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        _context.Tickets.RemoveRange(tickets);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
