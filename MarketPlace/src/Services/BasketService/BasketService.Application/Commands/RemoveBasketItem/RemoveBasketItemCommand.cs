using MediatR;

namespace BasketService.Application.Commands.RemoveBasketItem;

public sealed record RemoveBasketItemCommand(
    Guid UserId,
    Guid ProductId) : IRequest;