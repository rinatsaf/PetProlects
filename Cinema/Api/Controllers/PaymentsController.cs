using Application.Abstractions.Services;
using Application.DTOs.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    /// <summary>
    /// Returns payments accessible to current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of payments.</returns>
    [HttpGet]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> GetAccessible(CancellationToken cancellationToken)
    {
        var payments = await paymentService.GetAccessibleAsync(cancellationToken);
        return Ok(payments);
    }

    /// <summary>
    /// Returns payment by id if it is accessible to current user.
    /// </summary>
    /// <param name="id">Payment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Payment details.</returns>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetByIdAsync(id, cancellationToken);
        return Ok(payment);
    }

    /// <summary>
    /// Returns payment by external provider id.
    /// </summary>
    /// <param name="externalId">External payment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Payment details.</returns>
    [HttpGet("external/{externalId}")]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetByExternalId(string externalId, CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetByExternalIdAsync(externalId, cancellationToken);
        return Ok(payment);
    }

    /// <summary>
    /// Creates a payment for an order.
    /// </summary>
    /// <param name="request">Payment payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created payment.</returns>
    [HttpPost]
    [Authorize(Policy = "Customer")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var created = await paymentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Handles YooKassa webhook callback.
    /// </summary>
    /// <param name="request">Webhook payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated payment state.</returns>
    [HttpPost("yookassa/webhook")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> HandleYooKassaWebhook(
        [FromBody] YooKassaWebhookRequest request,
        CancellationToken cancellationToken)
    {
        Log.Information("YooKassa webhook received: Event={Event}, PaymentId={PaymentId}",
            request.Event, request.Payment?.Id);

        var rawPayload = JsonSerializer.Serialize(request);
        var updated = await paymentService.HandleYooKassaWebhookAsync(request, rawPayload, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Updates payment status manually by staff.
    /// </summary>
    /// <param name="id">Payment id.</param>
    /// <param name="request">Target status payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated payment.</returns>
    [HttpPost("{id:long}/status")]
    [Authorize(Policy = "Staff")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> UpdateStatus(long id, [FromBody] PaymentStatusUpdateRequest request, CancellationToken cancellationToken)
    {
        var updated = await paymentService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(updated);
    }
}
