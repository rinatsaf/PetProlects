using EventBus;

namespace BasketService.Application.Commands.CheckoutBasket;

public sealed record BasketCheckedOutIntegrationEvent(
    Guid BasketId,
    Guid UserId,
    List<BasketItemDto> Items,
    decimal Total) : IntegrationEvent;
