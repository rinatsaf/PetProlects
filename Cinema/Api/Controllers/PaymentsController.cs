using Application.Abstractions.Services;
using Application.DTOs.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<PaymentDto>> GetById(long id, CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetByIdAsync(id, cancellationToken);
        return Ok(payment);
    }

    [HttpGet("external/{externalId}")]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<PaymentDto>> GetByExternalId(string externalId, CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetByExternalIdAsync(externalId, cancellationToken);
        return Ok(payment);
    }

    [HttpPost]
    [Authorize(Policy = "Customer")]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var created = await paymentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("yookassa/webhook")]
    [AllowAnonymous]
    public async Task<ActionResult<PaymentDto>> HandleYooKassaWebhook(
        [FromBody] YooKassaWebhookRequest request,
        CancellationToken cancellationToken)
    {
        var rawPayload = JsonSerializer.Serialize(request);
        var updated = await paymentService.HandleYooKassaWebhookAsync(request, rawPayload, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:long}/status")]
    [AllowAnonymous] 
    public async Task<ActionResult<PaymentDto>> UpdateStatus(long id, [FromBody] PaymentStatusUpdateRequest request, CancellationToken cancellationToken)
    {
        var updated = await paymentService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(updated);
    }
}
