using ProductService.Domain.Products;

namespace ProductService.Application.Common.Abstractions;

public sealed record ProductReadModel(
    Guid Id,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    string MainImageUrl,
    ProductStatus Status);