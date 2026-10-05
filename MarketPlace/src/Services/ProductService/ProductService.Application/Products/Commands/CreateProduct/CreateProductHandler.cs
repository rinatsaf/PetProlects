using EventBus;
using MediatR;
using ProductService.Application.Common.Abstractions;
using ProductService.Domain.Products;
using ProductService.Domain.Shared;

namespace ProductService.Application.Products.Commands.CreateProduct;

public sealed class CreateProductHandler(IProductRepository productRepository, IMessageBus messageBus)
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMessageBus _messageBus = messageBus;

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var price = Money.Create(request.PriceAmount, request.PriceCurrency);

        var product = Product.Create(
            request.Name,
            request.Description,
            price,
            request.CategoryId,
            request.StockQuantity);
        
        await _productRepository.AddAsync(product, cancellationToken);

        foreach (var domainEvent in product.DomainEvents)
        {
            if (domainEvent is ProductCreatedDomainEvent created)
            {
                var integrationEvent = new ProductCreatedIntegrationEvent(
                    created.ProductId, created.ProductName, created.Price);
                
                await _messageBus.PublishAsync(integrationEvent, cancellationToken);
            }
        }
        product.ClearDomainEvents();
        
        return product.Id;
    }
}