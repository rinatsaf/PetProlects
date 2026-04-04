using System.Text.Json.Serialization;

namespace Application.DTOs.Payments;

public sealed class YooKassaWebhookPayment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("paid")]
    public bool Paid { get; set; }

    [JsonPropertyName("confirmation")]
    public YooKassaWebhookConfirmation? Confirmation { get; set; }
}