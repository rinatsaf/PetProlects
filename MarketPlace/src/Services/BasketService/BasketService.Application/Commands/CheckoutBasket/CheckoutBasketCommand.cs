using MediatR;

namespace BasketService.Application.Commands.CheckoutBasket;

public sealed record CheckoutBasketCommand(Guid UserId) : IRequest;