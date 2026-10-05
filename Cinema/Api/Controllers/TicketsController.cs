using Application.Abstractions.Services;
using Application.DTOs.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(ITicketService ticketService) : ControllerBase
{
    /// <summary>
    /// Returns ticket by id if it is accessible to current user.
    /// </summary>
    /// <param name="id">Ticket id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ticket details.</returns>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetByIdAsync(id, cancellationToken);
        return Ok(ticket);
    }

    /// <summary>
    /// Returns tickets for target order id.
    /// </summary>
    /// <param name="orderId">Order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of tickets.</returns>
    [HttpGet("order/{orderId:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetByOrder(long orderId, CancellationToken cancellationToken)
    {
        var tickets = await ticketService.GetByOrderAsync(orderId, cancellationToken);
        return Ok(tickets);
    }

    /// <summary>
    /// Returns currently available tickets (free seats) for target session id.
    /// </summary>
    /// <param name="sessionId">Session id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of available tickets.</returns>
    [HttpGet("session/{sessionId:long}/available")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(IReadOnlyList<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetAvailableBySession(long sessionId, CancellationToken cancellationToken)
    {
        var tickets = await ticketService.GetAvailableBySessionAsync(sessionId, cancellationToken);
        return Ok(tickets);
    }

    /// <summary>
    /// Marks active ticket as used (check-in at entrance).
    /// </summary>
    /// <param name="id">Ticket id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated ticket.</returns>
    [HttpPost("{id:long}/check-in")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDto>> CheckInById(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.MarkUsedAsync(id, cancellationToken);
        return Ok(ticket);
    }

    /// <summary>
    /// Marks active ticket as used by ticket code.
    /// </summary>
    /// <param name="request">Ticket code payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated ticket.</returns>
    [HttpPost("check-in/by-code")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDto>> CheckInByCode([FromBody] CheckInByCodeRequest request, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.MarkUsedByCodeAsync(request.Code, cancellationToken);
        return Ok(ticket);
    }
}
