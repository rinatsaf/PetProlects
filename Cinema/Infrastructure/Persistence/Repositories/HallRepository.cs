using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class HallRepository(CinemaDbContext context) : IHallRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<IReadOnlyList<Hall>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .AsNoTracking()
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Hall?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .Include(h => h.Seats)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Halls.AnyAsync(h => h.Name == name, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, long excludeId, CancellationToken cancellationToken = default)
    {
        return await _context.Halls.AnyAsync(h => h.Name == name && h.Id != excludeId, cancellationToken);
    }

    public async Task<Hall> AddAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        await _context.Halls.AddAsync(hall, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return hall;
    }

    public async Task<Hall> UpdateAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        _context.Halls.Update(hall);
        await _context.SaveChangesAsync(cancellationToken);
        return hall;
    }

    public async Task<Hall> DeleteAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        _context.Halls.Remove(hall);
        await _context.SaveChangesAsync(cancellationToken);
        return hall;
    }
}
