using Application.DTOs.Tickets;

namespace Application.Abstractions.Services;

public interface ITicketService
{
    Task<TicketDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetByOrderAsync(long orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetAvailableBySessionAsync(long sessionId, CancellationToken cancellationToken = default);
    Task<TicketDto> MarkUsedAsync(long id, CancellationToken cancellationToken = default);
    Task<TicketDto> MarkUsedByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> IssueAsync(IssueTicketsRequest request, CancellationToken cancellationToken = default);
}
