using BasketService.Application.Common.Abstractions;
using MediatR;

namespace BasketService.Application.Queries.GetBasket;

public sealed class GetBasketHandler(IBasketRepository repository) : IRequestHandler<GetBasketQuery, BasketDto?>
{
    public async Task<BasketDto?> Handle(GetBasketQuery request, CancellationToken ct)
    {
        var basket = await repository.GetByUserIdAsync(request.UserId, ct);
        if (basket is null) return null;

        return new BasketDto(
            basket.Id,
            basket.UserId,
            basket.Items.Select(i => new BasketItemDto(
                i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.ImageUrl)).ToList(),
            basket.GetTotal(),
            basket.LastUpdatedAt);
    }
}