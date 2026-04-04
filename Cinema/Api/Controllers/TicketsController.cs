using Application.Abstractions.Services;
using Application.DTOs.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<TicketDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetByIdAsync(id, cancellationToken);
        return Ok(ticket);
    }

    [HttpGet("order/{orderId:long}")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> GetByOrder(long orderId, CancellationToken cancellationToken)
    {
        var tickets = await ticketService.GetByOrderAsync(orderId, cancellationToken);
        return Ok(tickets);
    }
}
