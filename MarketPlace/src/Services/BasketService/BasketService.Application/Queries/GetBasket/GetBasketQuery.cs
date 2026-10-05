using MediatR;

namespace BasketService.Application.Queries.GetBasket;

public sealed record GetBasketQuery(Guid UserId) : IRequest<BasketDto?>;