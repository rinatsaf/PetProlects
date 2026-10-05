using Application.Abstractions.Services;
using Application.DTOs.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController(ISessionService sessionService) : ControllerBase
{
    /// <summary>
    /// Returns upcoming sessions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of sessions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> GetUpcoming(CancellationToken cancellationToken)
    {
        var sessions = await sessionService.GetUpcomingAsync(cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// Returns session by id.
    /// </summary>
    /// <param name="id">Session id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Session details.</returns>
    [HttpGet("{id:long}", Name = "GetSessionById")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var session = await sessionService.GetByIdAsync(id, cancellationToken);
        return Ok(session);
    }

    /// <summary>
    /// Get upcoming Sessions by movie id.
    /// </summary>
    /// <param name="movieId">Id movie</param>
    /// <param name="cancellationToken">Cancelletion Token</param>
    /// <returns>Sessions if exist for movie</returns>
    [HttpGet("movie/{id:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> GetByMovieId(long movieId,
        CancellationToken cancellationToken)
    {
        var sessions = await sessionService.GetUpcomingByMovieAsync(movieId, cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<IReadOnlyList<SessionScheduleDto>>> GetByDate([FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        var sessions = await sessionService.GetScheduleAsync(date, cancellationToken);
        
        return Ok(sessions);
    }
    
    /// <summary>
    /// Get upcoming Sessions by hall id.
    /// </summary>
    /// <param name="hallId">Id hall</param>
    /// <param name="cancellationToken">Cancelletion Token</param>
    /// <returns>Sessions if exist for hall</returns>
    [HttpGet("hall/{id:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> GetByHallId([FromQuery] long hallId,
        CancellationToken cancellationToken)
    {
        var sessions = await sessionService.GetUpcomingByHallAsync(hallId, cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("{id:long}/seat-map")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SeatMapDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SeatMapDto>> GetSeatMap(long id, CancellationToken cancellationToken)
    {
        var seatMap = await sessionService.GetSeatMapAsync(id, cancellationToken);
        return Ok(seatMap);
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="request">Session payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created session.</returns>
    [HttpPost]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionDto>> Create([FromBody] CreateSessionRequest request, CancellationToken cancellationToken)
    {
        var created = await sessionService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetSessionById", new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing session.
    /// </summary>
    /// <param name="id">Session id.</param>
    /// <param name="request">Updated session payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated session.</returns>
    [HttpPut("{id:long}")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionDto>> Update(long id, [FromBody] UpdateSessionRequest request, CancellationToken cancellationToken)
    {
        var updated = await sessionService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a session.
    /// </summary>
    /// <param name="id">Session id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted session.</returns>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionDto>> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await sessionService.DeleteAsync(id, cancellationToken);
        return Ok(deleted);
    }
}
