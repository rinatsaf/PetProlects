using Application.DTOs.Orders;

namespace Application.Abstractions.Services;

public interface IOrderService
{
    Task<OrderDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetAccessibleAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto> MarkPaidAsync(long id, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelAsync(long id, CancellationToken cancellationToken = default);
    Task<OrderDto> RefundOrderAsync(long id, CancellationToken cancellationToken);
}
