using Application.Abstractions.Services;
using Application.DTOs.Movies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieController(IMovieService movieService) : ControllerBase
{
    private readonly IMovieService _movieService = movieService;

    /// <summary>
    /// Returns all movies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of movies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MovieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MovieDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _movieService.GetAllAsync(cancellationToken);
        return Ok(movies);
    }

    /// <summary>
    /// Searches movies by filter set.
    /// </summary>
    /// <param name="request">Search filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered list of movies.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<MovieDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<MovieDto>>> SearchAsync(
        [FromQuery] MovieSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var movies = await _movieService.SearchAsync(request, cancellationToken);
        return Ok(movies);
    }

    /// <summary>
    /// Returns movie by id and tracks the view for authenticated users.
    /// </summary>
    /// <param name="id">Movie id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Movie details.</returns>
    [HttpGet("{id}", Name = "GetMovieById")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var movie = await _movieService.GetByIdAsync(id, cancellationToken);
        return Ok(movie);
    }

    /// <summary>
    /// Returns personalized movie recommendations for a user.
    /// </summary>
    /// <param name="count">Requested number of recommendations (1..50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recommended movies list.</returns>
    [HttpGet("recommendations")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(IReadOnlyList<MovieDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<MovieDto>>> GetRecommendationsAsync(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var recommended = await _movieService.GetRecommendationsAsync(count, cancellationToken);
        return Ok(recommended);
    }

    /// <summary>
    /// Creates a new movie.
    /// </summary>
    /// <param name="movie">Movie payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created movie.</returns>
    [Authorize(Policy = "Staff")]
    [HttpPost]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MovieDto>> CreateMovieAsync(
        [FromBody] CreateMovieRequest movie,
        CancellationToken cancellationToken = default)
    {
        var created = await _movieService.CreateAsync(movie, cancellationToken);
        return CreatedAtRoute("GetMovieById", new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing movie.
    /// </summary>
    /// <param name="id">Movie id.</param>
    /// <param name="request">Updated movie payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated movie.</returns>
    [HttpPut("{id:long}")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDto>> UpdateMovieAsync(
        long id,
        [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var updatedMovie = await _movieService.UpdateAsync(id, request, cancellationToken);
        return Ok(updatedMovie);
    }

    /// <summary>
    /// Deletes a movie.
    /// </summary>
    /// <param name="id">Movie id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted movie.</returns>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(MovieDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieDto>> DeleteMovieAsync(long id, CancellationToken cancellationToken)
    {
        var deletedMovie = await _movieService.DeleteAsync(id, cancellationToken);
        return Ok(deletedMovie);
    }
}
