using Application.Abstractions.Services;
using Application.DTOs.Genres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/genres")]
public class GenresController(IGenreService genreService) : ControllerBase
{
    /// <summary>
    /// Returns all genres.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of genres.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GenreDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GenreDto>>> GetAll(CancellationToken cancellationToken)
    {
        var genres = await genreService.GetAllAsync(cancellationToken);
        return Ok(genres);
    }

    /// <summary>
    /// Returns genre by id.
    /// </summary>
    /// <param name="id">Genre id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Genre details.</returns>
    [HttpGet("{id:long}", Name = "GetGenreById")]
    [ProducesResponseType(typeof(GenreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenreDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var genre = await genreService.GetByIdAsync(id, cancellationToken);
        return Ok(genre);
    }

    /// <summary>
    /// Creates a new genre.
    /// </summary>
    /// <param name="request">Genre payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created genre.</returns>
    [HttpPost]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(GenreDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GenreDto>> Create([FromBody] CreateGenreRequest request, CancellationToken cancellationToken)
    {
        var created = await genreService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetGenreById", new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing genre.
    /// </summary>
    /// <param name="id">Genre id.</param>
    /// <param name="request">Updated genre payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated genre.</returns>
    [HttpPut("{id:long}")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(GenreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GenreDto>> Update(long id, [FromBody] UpdateGenreRequest request, CancellationToken cancellationToken)
    {
        var updated = await genreService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a genre.
    /// </summary>
    /// <param name="id">Genre id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted genre.</returns>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(GenreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenreDto>> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await genreService.DeleteAsync(id, cancellationToken);
        return Ok(deleted);
    }
}
