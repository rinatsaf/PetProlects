using System.Globalization;
using System.Net;
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
        request.Headers.Add("Idempotence-Key", $"order_id={orderId}");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request, cancellationToken);
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

    public async Task<PaymentStatus> GetPaymentStatusAsync(
        string externalPaymentId,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"payments/{externalPaymentId}");
        request.Headers.Authorization = CreateAuthorizationHeader();

        var response = await httpClient.SendAsync(request, cancellationToken);
        var rawPayload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException(
                $"YooKassa payment status check failed for {externalPaymentId}: {rawPayload}");
        }

        using var document = JsonDocument.Parse(rawPayload);
        var root = document.RootElement;

        var status = root.TryGetProperty("status", out var statusElement)
            ? MapStatus(statusElement.GetString())
            : PaymentStatus.Pending;

        return status;
    }

    public async Task<YooKassaRefundResult> CreateRefundAsync(
        string externalPaymentId, 
        decimal amount, 
        string currency, 
        string idempotencyKey,
        CancellationToken ct)
    {
        // 1. Формируем тело запроса согласно API ЮKassa
        var payload = new
        {
            payment_id = externalPaymentId,
            amount = new
            {
                value = amount.ToString("0.00", CultureInfo.InvariantCulture),
                currency
            }
        };

        // 2. Создаем HTTP-запрос
        var request = new HttpRequestMessage(HttpMethod.Post, "refunds");
        request.Headers.Authorization = CreateAuthorizationHeader();
        request.Headers.Add("Idempotence-Key", idempotencyKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        // 3. Отправляем запрос и читаем ответ
        var response = await httpClient.SendAsync(request, ct);
        var rawPayload = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException($"YooKassa refund creation failed: {rawPayload}");
        }

        // 4. Парсим JSON-ответ
        using var document = JsonDocument.Parse(rawPayload);
        var root = document.RootElement;

        if (!root.TryGetProperty("id", out var idElement) || string.IsNullOrWhiteSpace(idElement.GetString()))
        {
            throw new ExternalServiceException("YooKassa response does not contain refund id.");
        }

        // 5. Маппим результат в ваш класс YooKassaRefundResult
        return new YooKassaRefundResult
        {
            ExternalId = idElement.GetString()!,
            Status = root.TryGetProperty("status", out var statusElement) ? statusElement.GetString()! : "pending",
            PaymentId = root.TryGetProperty("payment_id", out var paymentIdElement) ? paymentIdElement.GetString()! : externalPaymentId
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
