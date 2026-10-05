using BasketService.Application.Common.Abstractions;
using MediatR;

namespace BasketService.Application.Commands.UpdateBasketItem;

public sealed class UpdateBasketItemHandler(IBasketRepository basketRepository)
    : IRequestHandler<UpdateBasketItemCommand>
{
    private readonly IBasketRepository _basketRepository = basketRepository;

    public async Task Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Basket not found");
        
        basket.UpdateItemQuantity(request.ProductId, request.Quantity);
        await _basketRepository.UpsertAsync(basket, cancellationToken);
    }
}