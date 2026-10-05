using Application.Abstractions.Services;
using Application.DTOs.MovieReview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/movies/{movieId:long}/reviews")]
public class MovieReviewsController(IMovieReviewService reviewService) : ControllerBase
{
    /// <summary>
    /// Returns all reviews for a movie.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MovieReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MovieReviewDto>>> GetAll(long movieId, CancellationToken cancellationToken)
    {
        var reviews = await reviewService.GetAllReviewsByMovie(movieId, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Creates a review for a movie.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(MovieReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MovieReviewDto>> Create(long movieId, [FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await reviewService.CreateReview(movieId, request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { movieId }, review);
    }

    /// <summary>
    /// Updates a review. Only the author can update.
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(MovieReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieReviewDto>> Update(long movieId, long id, [FromBody] UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await reviewService.UpdateReview(id, request, cancellationToken);
        return Ok(review);
    }

    /// <summary>
    /// Deletes a review. Only the author can delete.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long movieId, long id, CancellationToken cancellationToken)
    {
        await reviewService.DeleteReview(id, cancellationToken);
        return NoContent();
    }
}
