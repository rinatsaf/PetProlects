using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ProductService.Domain.Products;
using ProductService.Domain.Shared;

namespace ProductService.Infrastructure.Mappings;

public static class ProductMongoMapping
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
            return;
        _configured = true;

        BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

        BsonClassMap.RegisterClassMap<Money>(cm =>
        {
            cm.MapMember(m => m.Amount).SetElementName("Amount");
            cm.MapMember(m => m.Currency).SetElementName("Currency");
            cm.MapCreator(m => Money.Create(m.Amount, m.Currency));
        });
        
        BsonClassMap.RegisterClassMap<ProductImage>(cm =>
        {
            cm.MapMember(m => m.Url).SetElementName("url");
            cm.MapMember(m => m.AltText).SetElementName("altText");
            cm.MapMember(m => m.IsPrimary).SetElementName("isPrimary");
            cm.MapCreator(m => ProductImage.Create(m.Url, m.AltText, m.IsPrimary));
        });

        BsonClassMap.RegisterClassMap<Product>(cm =>
        {
            cm.AutoMap();                          
            cm.SetIgnoreExtraElements(true);
            cm.MapMember(p => p.Name).SetElementName("name");
            cm.MapMember(p => p.Description).SetElementName("description");
            cm.MapMember(p => p.Price).SetElementName("price");
            cm.MapMember(p => p.CategoryId).SetElementName("categoryId");
            cm.MapMember(p => p.StockQuantity).SetElementName("stockQuantity");
            cm.MapMember(p => p.Status).SetElementName("status");
            cm.MapMember(p => p.Images).SetElementName("images");
            cm.MapMember(p => p.CreatedAt).SetElementName("createdAt");
            cm.MapMember(p => p.UpdatedAt).SetElementName("updatedAt");
            cm.UnmapMember(p => p.DomainEvents);
        });
    }
}