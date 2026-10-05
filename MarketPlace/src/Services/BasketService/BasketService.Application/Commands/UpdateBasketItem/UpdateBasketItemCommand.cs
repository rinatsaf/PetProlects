using MediatR;

namespace BasketService.Application.Commands.UpdateBasketItem;

public sealed record UpdateBasketItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity ) : IRequest;
