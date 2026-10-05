using Application.Abstractions.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SessionRepository(CinemaDbContext context) : ISessionRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<IReadOnlyList<Session>> GetAllUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .AsNoTracking()
            .Where(s => s.StartTime >= now && s.Status != SessionStatus.Canceled)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Session?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Session>> GetUpcomingByHallAsync(long hallId, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .AsNoTracking()
            .Where(s => s.HallId == hallId)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Session>> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var sessions = await context.Sessions
            .AsNoTracking()
            .Where(s => s.StartTime.Year == date.Year 
                        && s.StartTime.Month == date.Month 
                        && s.StartTime.Day == date.Day)
            .Where(s => s.Status == SessionStatus.Active
                        || s.Status == SessionStatus.Planned)
            .Include(s => s.Tickets)
                .ThenInclude(t => t.Seat)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(cancellationToken);
        
        return sessions;
    }

    public async Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<Session> AddWithTicketsAsync(Session session, IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var ticket in tickets)
        {
            ticket.TicketCode = BuildTicketCode(session.HallId, session.Id, ticket.SeatId);
        }

        _context.Tickets.AddRange(tickets);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return session;
    }

    private static string BuildTicketCode(long hallId, long sessionId, long seatId)
    {
        return $"H{hallId}-S{sessionId}-SEAT{seatId}".Trim();
    }

    public async Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<Session> DeleteAsync(Session session, CancellationToken cancellationToken = default)
    {
        if (session.Tickets.Count > 0)
        {
            _context.Tickets.RemoveRange(session.Tickets);
        }

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<bool> HasOverlapAsync(long hallId, DateTimeOffset startTime, DateTimeOffset endTime, long? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions.AnyAsync(s =>
                s.HallId == hallId &&
                (excludeId == null || s.Id != excludeId) &&
                s.StartTime < endTime &&
                s.EndTime > startTime,
            cancellationToken);
    }

    public async Task<int> ActivatePlannedAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Where(s => s.Status == SessionStatus.Planned && s.StartTime <= now && s.EndTime > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, SessionStatus.Active)
                .SetProperty(x => x.UpdatedAt, now), cancellationToken);
    }

    public async Task<int> FinishActiveAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Where(s => (s.Status == SessionStatus.Active || s.Status == SessionStatus.Planned) && s.EndTime <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, SessionStatus.Finished)
                .SetProperty(x => x.UpdatedAt, now), cancellationToken);
    }
}


