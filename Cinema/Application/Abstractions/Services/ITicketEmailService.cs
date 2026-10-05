using Domain.Entities;

namespace Application.Abstractions.Services;

public interface ITicketEmailService
{
    Task SendTicketsAsync(Order order, CancellationToken cancellationToken = default);
}
