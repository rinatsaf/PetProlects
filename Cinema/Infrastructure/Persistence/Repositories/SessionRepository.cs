using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SessionRepository(CinemaDbContext context) : ISessionRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<IReadOnlyList<Session>> GetAllUpcomingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .AsNoTracking()
            .Where(s => s.StartTime >= DateTimeOffset.Now)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Session?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        await _context.Sessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<Session> DeleteAsync(Session session, CancellationToken cancellationToken = default)
    {
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
}
