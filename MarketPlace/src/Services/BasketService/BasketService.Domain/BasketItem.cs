using Shared.Abstractions;

namespace BasketService.Domain;

public sealed class BasketItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = String.Empty;
    public decimal UnitPrice { get; private set; }
    public string? ImageUrl { get; private set; } = String.Empty;
    public int Quantity { get; private set; }
    public DateTime AddedAt { get; private set; }

    private BasketItem() : base(Guid.Empty)
    {
    }

    private BasketItem(Guid id, Guid productId, string productName, decimal unitPrice, string? imageUrl,
        int quantity) : base(id)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        ImageUrl = imageUrl;
        Quantity = quantity;
        AddedAt = DateTime.UtcNow;
    }

    public static BasketItem Create(Guid productId, string productName, decimal unitPrice, string? imageUrl,
        int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new BasketItem(Guid.NewGuid(), productId, productName, unitPrice, imageUrl, quantity);
    }
    
    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
    }

    public void IncreaseQuantity(int by)
    {
        if (by <= 0)
            throw new ArgumentException("Must increase by positive amount", nameof(by));

        Quantity += by;
    }
}