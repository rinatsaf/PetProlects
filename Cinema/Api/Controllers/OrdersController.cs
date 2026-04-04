using Application.Abstractions.Services;
using Application.DTOs.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<OrderDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return Ok(order);
    }

    [HttpGet("user/{userId:long}")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetByUser(long userId, CancellationToken cancellationToken)
    {
        var orders = await orderService.GetByUserAsync(userId, cancellationToken);
        return Ok(orders);
    }

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var created = await orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:long}/pay")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<OrderDto>> MarkPaid(long id, CancellationToken cancellationToken)
    {
        var updated = await orderService.MarkPaidAsync(id, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:long}/cancel")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<OrderDto>> Cancel(long id, CancellationToken cancellationToken)
    {
        var updated = await orderService.CancelAsync(id, cancellationToken);
        return Ok(updated);
    }
}
