using Application.DTOs.MovieReview;

namespace Application.Abstractions.Services;

public interface IMovieReviewService
{
    Task<MovieReviewDto> CreateReview(long movieId, CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MovieReviewDto>> GetAllReviewsByMovie(long movieId, CancellationToken cancellationToken = default);
    Task<MovieReviewDto> UpdateReview(long reviewId, UpdateReviewRequest request, CancellationToken cancellationToken = default);
    Task DeleteReview(long reviewId, CancellationToken cancellationToken = default);
}