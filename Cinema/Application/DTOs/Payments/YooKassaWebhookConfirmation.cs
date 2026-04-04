using System.Text.Json.Serialization;

namespace Application.DTOs.Payments;

public sealed class YooKassaWebhookConfirmation
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("confirmation_url")]
    public string? ConfirmationUrl { get; set; }
}