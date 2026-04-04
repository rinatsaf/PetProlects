using Application.DTOs.Tickets;

namespace Application.Abstractions.Services;

public interface ITicketService
{
    Task<TicketDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetByOrderAsync(long orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> IssueAsync(IssueTicketsRequest request, CancellationToken cancellationToken = default);
}
