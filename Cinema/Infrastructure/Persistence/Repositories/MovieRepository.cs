using Application.Abstractions.Repositories;
using Application.DTOs.Movies;
using Application.Exceptions;
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
            .Include(x => x.MovieGenres)
            .ThenInclude(x => x.Genre)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Movie>> SearchAsync(
        MovieSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Movies
            .AsNoTracking()
            .Include(x => x.MovieGenres)
            .ThenInclude(x => x.Genre)
            .AsQueryable();
        query = ApplySearchFilters(query, request);

        return await query
            .OrderByDescending(x => x.PopularityScore)
            .ThenByDescending(x => x.ReleaseDate)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .Include(x => x.MovieGenres)
            .ThenInclude(x => x.Genre)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Movie> AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        var mov = await _context.Movies.FirstOrDefaultAsync(x => x.Title == movie.Title, cancellationToken);
        if (mov != null)
            throw new ConflictException("Movie already exists");

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Movies
                   .AsNoTracking()
                   .Include(x => x.MovieGenres)
                   .ThenInclude(x => x.Genre)
                   .FirstAsync(x => x.Id == movie.Id, cancellationToken);
    }

    public async Task<Movie> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Movies
                   .AsNoTracking()
                   .Include(x => x.MovieGenres)
                   .ThenInclude(x => x.Genre)
                   .FirstAsync(x => x.Id == movie.Id, cancellationToken);
    }

    public async Task<Movie> DeleteAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);

        return movie;
    }

    public async Task<List<Movie>> GetRecommendedAsync(
        Dictionary<string, decimal> userWeights,
        List<long> excludeIds,
        int count,
        CancellationToken ct = default)
    {
        var featureKeys = userWeights.Keys.ToList();
        
        var candidates = await _context.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Where(m => m.IsActive && !excludeIds.Contains(m.Id))
            .Where(m => m.MovieGenres.Any(mg => featureKeys.Contains(mg.Genre.Name)))
            .OrderByDescending(m => m.PopularityScore)
            .Take(count + 5) 
            .ToListAsync(ct);
        
        return candidates
            .Select(m => new
            {
                Movie = m,
                Score = m.MovieGenres
                    .Where(mg => userWeights.ContainsKey(mg.Genre.Name))
                    .Sum(mg => userWeights[mg.Genre.Name])
            })
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Movie.PopularityScore)
            .Take(count)
            .Select(x => x.Movie)
            .ToList();
    }

    public async Task<List<Movie>> GetPopularAsync(List<long> excludeIds, int count, CancellationToken ct)
    {
        return await _context.Movies
            .AsNoTracking()
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Where(m => m.IsActive && !excludeIds.Contains(m.Id))
            .OrderByDescending(m => m.PopularityScore)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetGenreNamesByMovieIdAsync(long movieId, CancellationToken ct = default)
    {
        return await _context.MovieGenres
            .Where(mg => mg.MovieId == movieId)
            .Select(mg => mg.Genre.Name)
            .ToListAsync(ct);
    }

    private static IQueryable<Movie> ApplySearchFilters(IQueryable<Movie> query, MovieSearchRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            var title = request.Title.Trim();
            query = query.Where(x => x.Title.Contains(title));
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            var country = request.Country.Trim();
            query = query.Where(x => x.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(request.AgeRating))
        {
            var ageRating = request.AgeRating.Trim();
            query = query.Where(x => x.AgeRating == ageRating);
        }

        if (!string.IsNullOrWhiteSpace(request.Genre))
        {
            var genre = request.Genre.Trim();
            query = query.Where(x => x.MovieGenres.Any(mg => mg.Genre.Name == genre));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.ReleaseDateFrom.HasValue)
        {
            query = query.Where(x => x.ReleaseDate >= request.ReleaseDateFrom.Value);
        }

        if (request.ReleaseDateTo.HasValue)
        {
            query = query.Where(x => x.ReleaseDate <= request.ReleaseDateTo.Value);
        }

        if (request.MinDurationMinutes.HasValue)
        {
            query = query.Where(x => x.DurationMinutes >= request.MinDurationMinutes.Value);
        }

        if (request.MaxDurationMinutes.HasValue)
        {
            query = query.Where(x => x.DurationMinutes <= request.MaxDurationMinutes.Value);
        }

        if (request.MinPopularityScore.HasValue)
        {
            query = query.Where(x => x.PopularityScore >= request.MinPopularityScore.Value);
        }

        if (request.MaxPopularityScore.HasValue)
        {
            query = query.Where(x => x.PopularityScore <= request.MaxPopularityScore.Value);
        }

        return query;
    }
}


