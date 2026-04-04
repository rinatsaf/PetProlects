using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MovieRepository(CinemaDbContext context) : IMovieRepository
{
    private readonly CinemaDbContext _context = context;


    public async Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Movie> AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await _context.Movies.AddAsync(movie, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return movie;
    }

    public async Task<Movie> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return movie;
    }

    public async Task<Movie> DeleteAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return movie;
    }
}