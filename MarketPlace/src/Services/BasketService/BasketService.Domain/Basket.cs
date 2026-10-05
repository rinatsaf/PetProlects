using BasketService.Domain.Events;
using Shared.Abstractions;

namespace BasketService.Domain;

public sealed class Basket : Entity<Guid>, IAggregateRoot
{
    private readonly List<BasketItem> _items = [];
    private readonly IList<IDomainEvent> _domainEvents = [];
    
    public Guid UserId { get; private set;}
    public BasketStatus Status { get; private set;}
    public IReadOnlyList<BasketItem> Items => _items.AsReadOnly();
    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public DateTime LastUpdatedAt { get; private set; }
    
    private Basket() : base(Guid.Empty) { }

    private Basket(Guid id, Guid userId) : base(id)
    {
        UserId = userId;
        Status = BasketStatus.Active;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public static Basket Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("user id cannot be empty", nameof(userId));
        
        return new Basket(Guid.NewGuid(), userId);
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity = 1)
    {
        if (Status != BasketStatus.Active)
            throw new InvalidOperationException("Cannot add another item");
        
        if (_items.Count >= 50)
            throw new InvalidOperationException("Cannot add more than 50 items");
        
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + quantity;
            if (newQuantity > 99)
                throw new InvalidOperationException("Cannot add more than 99 items");
            
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            if (quantity > 99)
                throw new InvalidOperationException("Cannot add more than 99 items");
            _items.Add(BasketItem.Create(productId, productName, unitPrice, imageUrl, quantity));
        }
        
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        if (Status != BasketStatus.Active)
            throw new InvalidOperationException("Cannot update another item");
        
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException("Cannot update another item");
        
        item.UpdateQuantity(quantity);
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != BasketStatus.Active)
            throw new InvalidOperationException("Cannot remove another item");
        
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
                   ?? throw new InvalidOperationException("Cannot remove another item");
        
        _items.Remove(item);
        LastUpdatedAt = DateTime.UtcNow;
    }
    public void Checkout()
    {
        if (Status != BasketStatus.Active)
            throw new InvalidOperationException("Basket is already checked out");

        if (!_items.Any())
            throw new InvalidOperationException("Cannot checkout empty basket");

        Status = BasketStatus.CheckedOut;
        LastUpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BasketCheckedOutDomainEvent(Id, UserId, _items));
    }
    
    public decimal GetTotal() => _items.Sum(i => i.UnitPrice * i.Quantity);

    private void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
