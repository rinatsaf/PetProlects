using Shared.Abstractions;

namespace BasketService.Domain.Events;

public sealed record BasketCheckedOutDomainEvent(
    Guid BasketId,
    Guid UserId,
    IReadOnlyList<BasketItem> Items) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}