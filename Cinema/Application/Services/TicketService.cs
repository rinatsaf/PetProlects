using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Tickets;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class TicketService(ITicketRepository ticketRepository, IMapper mapper) : ITicketService
{
    private readonly ITicketRepository _ticketRepository = ticketRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<TicketDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id, cancellationToken)
                     ?? throw new NotFoundException($"Ticket {id} not found");
        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<IReadOnlyList<TicketDto>> GetByOrderAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetByOrderAsync(orderId, cancellationToken);
        return _mapper.Map<IReadOnlyList<TicketDto>>(tickets);
    }

    public async Task<IReadOnlyList<TicketDto>> IssueAsync(IssueTicketsRequest request, CancellationToken cancellationToken = default)
    {
        var tickets = request.Seats.Select(seat => new Ticket
        {
            OrderId = request.OrderId,
            SessionId = request.SessionId,
            SeatId = seat.SeatId,
            Price = request.Price,
            TicketCode = seat.TicketCode,
            QrCodeUrl = seat.QrCodeUrl,
            Status = TicketStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        }).ToList();

        await _ticketRepository.AddRangeAsync(tickets, cancellationToken);

        return _mapper.Map<IReadOnlyList<TicketDto>>(tickets);
    }
}
