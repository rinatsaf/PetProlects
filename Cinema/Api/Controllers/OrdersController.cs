using Application.Abstractions.Services;
using Application.DTOs.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>
    /// Returns orders accessible to current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of orders.</returns>
    [HttpGet]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAccessible(CancellationToken cancellationToken)
    {
        var orders = await orderService.GetAccessibleAsync(cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Returns order by id if it is accessible to current user.
    /// </summary>
    /// <param name="id">Order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Order details.</returns>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return Ok(order);
    }

    /// <summary>
    /// Returns orders for target user id within current access scope.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of user orders.</returns>
    [HttpGet("user/{userId:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetByUser(long userId, CancellationToken cancellationToken)
    {
        var orders = await orderService.GetByUserAsync(userId, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="request">Order payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created order.</returns>
    [HttpPost]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var created = await orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Marks order as paid.
    /// </summary>
    /// <param name="id">Order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated order.</returns>
    [HttpPost("{id:long}/pay")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> MarkPaid(long id, CancellationToken cancellationToken)
    {
        var updated = await orderService.MarkPaidAsync(id, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:long}/refund")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> RefundOrder(long id, CancellationToken cancellationToken)
    {
        var order =  await orderService.RefundOrderAsync(id, cancellationToken);
        return Ok(order);
    }
    /// <summary>
    /// Cancels order if cancellation is allowed.
    /// </summary>
    /// <param name="id">Order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated order.</returns>
    [HttpPost("{id:long}/cancel")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Cancel(long id, CancellationToken cancellationToken)
    {
        var updated = await orderService.CancelAsync(id, cancellationToken);
        return Ok(updated);
    }
}
