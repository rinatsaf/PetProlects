using System.Net;

namespace Infrastructure.Payments;

public sealed class YooKassaOptions
{
    public const string SectionName = "YooKassa";

    public string ShopId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PaymentMethodType { get; set; } = "bank_card";

    public List<string> AllowedWebhookIps { get; set; } =
    [
        "185.71.76.0/27",
        "185.71.77.0/27",
        "77.75.153.0/25",
        "77.75.154.128/25",
        "77.75.156.11/32",
        "77.75.156.35/32",
        "2a02:5180::/32"
    ];

    public List<IPNetwork> AllowedWebhookNetworks => AllowedWebhookIps
        .Select(ip => ip.Contains('/') ? ip : $"{ip}/32")
        .Select(IPNetwork.Parse)
        .ToList();
}
