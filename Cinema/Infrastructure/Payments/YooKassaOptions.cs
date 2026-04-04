namespace Infrastructure.Payments;

public sealed class YooKassaOptions
{
    public const string SectionName = "YooKassa";

    public string ShopId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PaymentMethodType { get; set; } = "bank_card";
}
