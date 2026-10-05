using System.Windows.Input;
using MediatR;

namespace BasketService.Application.Commands.AddItemToBasket;

public sealed record AddItemToBasketCommand(
    Guid UserId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string? ImageUrl,
    int Quantity = 1) : IRequest;