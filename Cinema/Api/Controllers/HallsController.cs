using Application.Abstractions.Services;
using Application.DTOs.Halls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/halls")]
public class HallsController(IHallService hallService) : ControllerBase
{
    /// <summary>
    /// Returns all halls.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of halls.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HallDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HallDto>>> GetAll(CancellationToken cancellationToken)
    {
        var halls = await hallService.GetAllAsync(cancellationToken);
        return Ok(halls);
    }

    /// <summary>
    /// Returns hall by id.
    /// </summary>
    /// <param name="id">Hall id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Hall details.</returns>
    [HttpGet("{id:long}", Name = "GetHallById")]
    [ProducesResponseType(typeof(HallDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HallDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var hall = await hallService.GetByIdAsync(id, cancellationToken);
        return Ok(hall);
    }

    /// <summary>
    /// Creates a new hall.
    /// </summary>
    /// <param name="request">Hall payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created hall.</returns>
    [HttpPost]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(HallDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HallDto>> Create([FromBody] CreateHallRequest request, CancellationToken cancellationToken)
    {
        var created = await hallService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetHallById", new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing hall.
    /// </summary>
    /// <param name="id">Hall id.</param>
    /// <param name="request">Updated hall payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated hall.</returns>
    [HttpPut("{id:long}")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(HallDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HallDto>> Update(long id, [FromBody] UpdateHallRequest request, CancellationToken cancellationToken)
    {
        var updated = await hallService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a hall.
    /// </summary>
    /// <param name="id">Hall id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted hall.</returns>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(HallDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HallDto>> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await hallService.DeleteAsync(id, cancellationToken);
        return Ok(deleted);
    }
}
