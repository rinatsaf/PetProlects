using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.MovieReview;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public sealed class MovieReviewService(
    IMovieReviewRepository repository, 
    IMovieRepository movieRepository,
    ICurrentUserService currentUser,
    IMapper mapper) : IMovieReviewService
{
    public async Task<MovieReviewDto> CreateReview(long movieId, CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var user = currentUser.GetCurrentUser();
        
        if (!await movieRepository.ExistsByIdAsync(movieId, cancellationToken))
            throw new NotFoundException($"Movie with id {movieId} not found");

        var movieReview = new MovieReview
        {
            UserId = user.UserId,
            MovieId = movieId,
            Comment = request.Comment,
            Rating = request.Rating
        };
        
        var created = await repository.CreateReviewAsync(movieReview, cancellationToken);
        
        return mapper.Map<MovieReviewDto>(created);
    }

    public async Task<IReadOnlyList<MovieReviewDto>> GetAllReviewsByMovie(long movieId, CancellationToken cancellationToken = default)
    {
        var reviews = await repository.GetReviewsByMovieAsync(movieId, cancellationToken);
        
        return mapper.Map<IReadOnlyList<MovieReviewDto>>(reviews);
    }

    public async Task<MovieReviewDto> UpdateReview(long reviewId, UpdateReviewRequest request, CancellationToken cancellationToken = default)
    {
        var review = await repository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new NotFoundException($"Review with id {reviewId} not found");
        
        var user = currentUser.GetCurrentUser();
        if (user.UserId != review.UserId)
            throw new ForbiddenException("You can only edit your own review.");
        
        review.Comment = request.Comment;
        review.Rating = request.Rating;
        review.UpdatedAt = DateTimeOffset.UtcNow;
        
        var updated = await repository.UpdateReviewAsync(review, cancellationToken);
        
        return mapper.Map<MovieReviewDto>(updated);
    }

    public async Task DeleteReview(long reviewId, CancellationToken cancellationToken = default)
    {
        var review = await repository.GetByIdAsync(reviewId, cancellationToken)
                     ?? throw new NotFoundException($"Review with id {reviewId} not found");
        
        var user = currentUser.GetCurrentUser();
        if (user.UserId != review.UserId)
            throw new ForbiddenException("You can only delete your own review.");

        await repository.DeleteReviewAsync(reviewId, cancellationToken);
    }
}