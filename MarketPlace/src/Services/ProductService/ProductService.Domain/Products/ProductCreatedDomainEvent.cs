using ProductService.Domain.Shared;
using Shared.Abstractions;

namespace ProductService.Domain.Products;


public sealed record ProductCreatedDomainEvent(
    Guid ProductId,
    string ProductName,
    Money Price) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}