using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GenreRepository(CinemaDbContext context) : IGenreRepository
{
    private readonly CinemaDbContext _context = context;

    public async Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Genres
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Genre?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Genres.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Genres.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, long excludeId, CancellationToken cancellationToken = default)
    {
        return await _context.Genres.AnyAsync(x => x.Name == name && x.Id != excludeId, cancellationToken);
    }

    public async Task<Genre> AddAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        await _context.Genres.AddAsync(genre, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return genre;
    }

    public async Task<Genre> UpdateAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        _context.Genres.Update(genre);
        await _context.SaveChangesAsync(cancellationToken);
        return genre;
    }

    public async Task<Genre> DeleteAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync(cancellationToken);
        return genre;
    }
}