using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SeatRepository(CinemaDbContext context) : ISeatRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<IReadOnlyList<Seat>> GetByHallAsync(long hallId, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .AsNoTracking()
            .Where(s => s.HallId == hallId)
            .OrderBy(s => s.RowNumber)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Seat>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        var seatIds = ids.Distinct().ToArray();
        return await _context.Seats
            .AsNoTracking()
            .Where(s => seatIds.Contains(s.Id))
            .OrderBy(s => s.RowNumber)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Seat?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Seats.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByPositionAsync(long hallId, int rowNumber, int seatNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Seats.AnyAsync(s =>
            s.HallId == hallId &&
            s.RowNumber == rowNumber &&
            s.SeatNumber == seatNumber, cancellationToken);
    }

    public async Task<Seat> AddAsync(Seat seat, CancellationToken cancellationToken = default)
    {
        _context.Seats.Add(seat);
        await _context.SaveChangesAsync(cancellationToken);
        return seat;
    }

    public async Task AddRangeAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default)
    {
        await _context.Seats.AddRangeAsync(seats, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Seat> UpdateAsync(Seat seat, CancellationToken cancellationToken = default)
    {
        _context.Seats.Update(seat);
        await _context.SaveChangesAsync(cancellationToken);
        return seat;
    }

    public async Task DeleteRangeAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default)
    {
        _context.Seats.RemoveRange(seats);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


