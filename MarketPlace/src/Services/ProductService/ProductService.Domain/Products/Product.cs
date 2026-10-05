using ProductService.Domain.Shared;
using Shared.Abstractions;

namespace ProductService.Domain.Products;

public class Product : Entity<Guid>, IAggregateRoot
{
    private readonly List<ProductImage> _images = [];
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public Guid CategoryId { get; private set; }
    public int StockQuantity { get; private set; }
    public ProductStatus Status { get; private set; }
    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }
    
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    private Product() : base(Guid.NewGuid()) {} // MongoDb

    private Product(
        Guid id,
        string name,
        string description,
        Money price,
        Guid categoryId,
        int stockQuantity
    ) : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        StockQuantity = stockQuantity;
        Status = ProductStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Product Create(
        string name,
        string description,
        Money price,
        Guid categoryId,
        int stockQuantity = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        var product = new Product(
            Guid.NewGuid(),
            name,
            description,
            price,
            categoryId,
            stockQuantity);

        product.AddDomainEvent(new ProductCreatedDomainEvent(
            product.Id, product.Name, product.Price));

        return product;
    }

    public void UpdateDetails(string name, string description, Money price, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
        Description = description;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void AddImage(string url, string altText, bool isPrimary = false)
    {
        if (isPrimary && _images.Any(i => i.IsPrimary))
            throw new InvalidOperationException("Product already has a primary image");

        _images.Add(ProductImage.Create(url, altText, isPrimary));
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveImage(string url)
    {
        var image = _images.FirstOrDefault(i => i.Url == url);
        if (image is not null)
            _images.Remove(image);

        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Publish() => Status = ProductStatus.Active;

    public void Suspend() => Status = ProductStatus.Suspended;

    public void Archive() => Status = ProductStatus.Archived;
    
    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (StockQuantity - quantity < 0)
            throw new InvalidOperationException("Insufficient stock");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}