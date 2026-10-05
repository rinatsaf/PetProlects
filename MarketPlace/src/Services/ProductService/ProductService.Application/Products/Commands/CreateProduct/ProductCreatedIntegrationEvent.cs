using EventBus;
using ProductService.Domain.Shared;

namespace ProductService.Application.Products.Commands.CreateProduct;

public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string ProductName,
    Money Price) : IntegrationEvent;