using BasketService.Application.Common.Abstractions;
using BasketService.Domain.Events;
using EventBus;
using MediatR;

namespace BasketService.Application.Commands.CheckoutBasket;

public sealed class CheckoutBasketHandler(IBasketRepository basketRepository, IMessageBus messageBus)
    : IRequestHandler<CheckoutBasketCommand>
{
    public async Task Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetByUserIdAsync(request.UserId, cancellationToken)
                     ?? throw new InvalidOperationException("Basket not found");
                     
        basket.Checkout();

        foreach (var domainEvent in basket.DomainEvents)
        {
            if (domainEvent is BasketCheckedOutDomainEvent evt)
            {
                var integrationEvent = new BasketCheckedOutIntegrationEvent(
                    evt.BasketId,
                    evt.UserId,
                    evt.Items.Select(i => new BasketItemDto(
                        i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.ImageUrl)).ToList(),
                    basket.GetTotal());
                
                await messageBus.PublishAsync(integrationEvent, cancellationToken);
            }
        }
        
        basket.ClearDomainEvents();
        await basketRepository.DeleteAsync(request.UserId, cancellationToken);
    }
}