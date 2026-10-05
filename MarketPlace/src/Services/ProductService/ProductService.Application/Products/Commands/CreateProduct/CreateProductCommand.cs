using MediatR;

namespace ProductService.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency,
    Guid CategoryId,
    int StockQuantity) : IRequest<Guid>;