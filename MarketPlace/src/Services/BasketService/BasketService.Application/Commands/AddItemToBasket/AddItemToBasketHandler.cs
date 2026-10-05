using BasketService.Application.Common.Abstractions;
using BasketService.Domain;
using MediatR;

namespace BasketService.Application.Commands.AddItemToBasket;

public sealed class AddItemToBasketHandler(IBasketRepository basketRepository) : IRequestHandler<AddItemToBasketCommand>
{
    private readonly IBasketRepository _basketRepository = basketRepository;

    public async Task Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (basket is null)
        {
            basket = Basket.Create(request.UserId);
            basket.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.ImageUrl, request.Quantity);
            await _basketRepository.UpsertAsync(basket, cancellationToken);
            return;
        }
        
        basket.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.ImageUrl, request.Quantity);
        await _basketRepository.UpsertAsync(basket, cancellationToken);
    }
}