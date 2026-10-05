namespace BasketService.Application;

public sealed record BasketDto(
    Guid Id,
    Guid UserId,
    List<BasketItemDto> Items,
    decimal Total,
    DateTime LastUpdatedAt);