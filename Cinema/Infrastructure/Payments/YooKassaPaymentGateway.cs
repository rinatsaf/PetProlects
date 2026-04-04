using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Services;
using Application.DTOs.Payments;
using Application.Exceptions;
using Domain.Enums;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments;

public sealed class YooKassaPaymentGateway(
    HttpClient httpClient,
    IOptions<YooKassaOptions> options) : IYooKassaPaymentGateway
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly YooKassaOptions _options = options.Value;

    public async Task<YooKassaCreatePaymentResult> CreatePaymentAsync(
        long orderId,
        decimal amount,
        string currency,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        var paymentMethodType = ResolvePaymentMethodType();
        var payload = new
        {
            amount = new
            {
                value = amount.ToString("0.00", CultureInfo.InvariantCulture),
                currency
            },
            payment_method_data = new
            {
                type = paymentMethodType
            },
            confirmation = new
            {
                type = "redirect",
                return_url = returnUrl
            },
            capture = true,
            description = $"Order #{orderId}"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "payments");
        request.Headers.Authorization = CreateAuthorizationHeader();
        request.Headers.Add("Idempotence-Key", Guid.NewGuid().ToString("N"));
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var rawPayload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException($"YooKassa payment creation failed: {rawPayload}");
        }

        using var document = JsonDocument.Parse(rawPayload);
        var root = document.RootElement;

        if (!root.TryGetProperty("id", out var idElement) || string.IsNullOrWhiteSpace(idElement.GetString()))
        {
            throw new ExternalServiceException("YooKassa response does not contain payment id.");
        }

        var status = root.TryGetProperty("status", out var statusElement)
            ? MapStatus(statusElement.GetString())
            : PaymentStatus.Pending;

        string? confirmationUrl = null;
        if (root.TryGetProperty("confirmation", out var confirmationElement) &&
            confirmationElement.TryGetProperty("confirmation_url", out var confirmationUrlElement))
        {
            confirmationUrl = confirmationUrlElement.GetString();
        }

        return new YooKassaCreatePaymentResult
        {
            ExternalPaymentId = idElement.GetString()!,
            Status = status,
            PaymentMethod = paymentMethodType,
            ConfirmationUrl = confirmationUrl,
            RawPayload = rawPayload
        };
    }

    private AuthenticationHeaderValue CreateAuthorizationHeader()
    {
        var raw = $"{_options.ShopId}:{_options.SecretKey}";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return new AuthenticationHeaderValue("Basic", base64);
    }

    private static PaymentStatus MapStatus(string? status)
    {
        return status?.Trim().ToLowerInvariant() switch
        {
            "succeeded" => PaymentStatus.Succeeded,
            "canceled" => PaymentStatus.Cancelled,
            "pending" => PaymentStatus.Pending,
            "waiting_for_capture" => PaymentStatus.Pending,
            _ => PaymentStatus.Failed
        };
    }

    private string ResolvePaymentMethodType()
    {
        var configuredType = _options.PaymentMethodType?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(configuredType) ? "bank_card" : configuredType;
    }
}
