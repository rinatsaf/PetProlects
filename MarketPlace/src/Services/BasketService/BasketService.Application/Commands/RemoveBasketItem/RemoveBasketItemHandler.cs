using BasketService.Application.Common.Abstractions;
using MediatR;

namespace BasketService.Application.Commands.RemoveBasketItem;

public sealed class RemoveBasketItemHandler(IBasketRepository basketRepository)
    : IRequestHandler<RemoveBasketItemCommand>
{
    private readonly IBasketRepository _basketRepository = basketRepository;

    public async Task Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetByUserIdAsync(request.UserId, cancellationToken)
                     ?? throw new InvalidOperationException("Basket not found");

        basket.RemoveItem(request.ProductId);
        await _basketRepository.UpsertAsync(basket, cancellationToken);
    }
}