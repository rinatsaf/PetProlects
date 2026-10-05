namespace BasketService.Application;

public sealed record BasketItemDto(
    Guid ProductId,
    string ProductName, 
    decimal UnitPrice, 
    int Quantity,
    string? ImageUrl 
    );