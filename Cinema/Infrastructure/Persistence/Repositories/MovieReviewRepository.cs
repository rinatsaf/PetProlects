using Application.Abstractions.Repositories;
using Application.Exceptions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class MovieReviewRepository(CinemaDbContext context) : IMovieReviewRepository
{
    public async Task<IReadOnlyList<MovieReview>> GetReviewsByMovieAsync(long movieId, CancellationToken ct = default)
    {
        var reviews = await context.MovieReviews
            .Where(mr => mr.MovieId == movieId)
            .AsNoTracking()
            .ToListAsync(ct);
        
        return  reviews;
    }

    public async Task<MovieReview?> GetByIdAsync(long reviewId, CancellationToken ct = default)
    {
        var review = await context.MovieReviews
            .FirstOrDefaultAsync(mr =>  mr.Id == reviewId, ct);
        
        return review;
    }

    public async Task<MovieReview> UpdateReviewAsync(MovieReview review, CancellationToken ct = default)
    {
        context.MovieReviews.Update(review);
        await context.SaveChangesAsync(ct);
        
        return review;
    }

    public async Task<MovieReview> CreateReviewAsync(MovieReview review, CancellationToken ct = default)
    {
        context.MovieReviews.Add(review);
        await context.SaveChangesAsync(ct);
        return review;
    }

    public async Task DeleteReviewAsync(long id, CancellationToken ct = default)
    {
        var review = await context.MovieReviews.FirstOrDefaultAsync(mr  => mr.Id == id, ct)
            ?? throw new NotFoundException($"MovieReview with id {id} not found");
        
        context.MovieReviews.Remove(review);
        await context.SaveChangesAsync(ct);
    }
}