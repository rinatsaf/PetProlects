using System.Text.Json.Serialization;

namespace Application.DTOs.Payments;

public sealed class YooKassaWebhookRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public YooKassaWebhookPayment? Payment { get; set; }
}