using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMovieReviewRepository
{
    Task<IReadOnlyList<MovieReview>> GetReviewsByMovieAsync(long movieId, CancellationToken ct = default);
    Task<MovieReview?> GetByIdAsync(long reviewId, CancellationToken ct = default);
    Task<MovieReview> UpdateReviewAsync(MovieReview review, CancellationToken ct = default);
    Task<MovieReview> CreateReviewAsync(MovieReview review, CancellationToken ct = default);
    Task DeleteReviewAsync(long id, CancellationToken ct = default);
}