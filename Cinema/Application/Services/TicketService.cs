using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Tickets;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Serilog;

namespace Application.Services;

public class TicketService(
    ITicketRepository ticketRepository,
    IMapper mapper,
    ICurrentUserService currentUserService) : ITicketService
{
    private static readonly ILogger BusinessLogger = Log.ForContext("BusinessLog", true);
    private readonly ITicketRepository _ticketRepository = ticketRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TicketDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = _currentUserService.GetCurrentUser();
        var ticket = await _ticketRepository.GetByIdAsync(id, user, cancellationToken)
                     ?? throw new NotFoundException($"Ticket {id} not found");
        return _mapper.Map<TicketDto>(ticket);
    }

    public async Task<IReadOnlyList<TicketDto>> GetByOrderAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var user = _currentUserService.GetCurrentUser();
        var tickets = await _ticketRepository.GetByOrderAsync(orderId, user, cancellationToken);
        return _mapper.Map<IReadOnlyList<TicketDto>>(tickets);
    }

    public async Task<IReadOnlyList<TicketDto>> GetAvailableBySessionAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetAvailableBySessionAsync(sessionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<TicketDto>>(tickets);
    }

    public async Task<TicketDto> MarkUsedAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = _currentUserService.GetCurrentUser();
        EnsureStaffUser(user);

        var ticket = await _ticketRepository.GetByIdAsync(id, user, cancellationToken)
                     ?? throw new NotFoundException($"Ticket {id} not found");

        EnsureTicketCanBeUsed(ticket);

        ticket.Status = TicketStatus.Used;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        BusinessLogger.Information(
            "Ticket checked-in by id: TicketId={TicketId}, SessionId={SessionId}, StaffUserId={StaffUserId}",
            updated.Id,
            updated.SessionId,
            user.UserId);
        return _mapper.Map<TicketDto>(updated);
    }

    public async Task<TicketDto> MarkUsedByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var user = _currentUserService.GetCurrentUser();
        EnsureStaffUser(user);

        var normalizedCode = code.Trim();
        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            throw new ValidationException("Ticket code is required.");
        }

        var ticket = await _ticketRepository.GetByCodeAsync(normalizedCode, user, cancellationToken)
                     ?? throw new NotFoundException($"Ticket with code '{normalizedCode}' not found");

        EnsureTicketCanBeUsed(ticket);

        ticket.Status = TicketStatus.Used;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        BusinessLogger.Information(
            "Ticket checked-in by code: TicketId={TicketId}, SessionId={SessionId}, StaffUserId={StaffUserId}",
            updated.Id,
            updated.SessionId,
            user.UserId);
        return _mapper.Map<TicketDto>(updated);
    }

    public async Task<IReadOnlyList<TicketDto>> IssueAsync(IssueTicketsRequest request, CancellationToken cancellationToken = default)
    {
        var seatIds = request.Seats.Select(x => x.SeatId).ToArray();
        var tickets = (await _ticketRepository.GetAvailableBySessionAndSeatsAsync(request.SessionId, seatIds, cancellationToken))
            .ToDictionary(x => x.SeatId);

        foreach (var seat in request.Seats)
        {
            if (!tickets.TryGetValue(seat.SeatId, out var ticket))
            {
                throw new NotFoundException($"Ticket for session {request.SessionId} and seat {seat.SeatId} not found");
            }

            ticket.OrderId = request.OrderId;
            ticket.Price = request.Price;
            ticket.TicketCode = seat.TicketCode;
            ticket.QrCodeUrl = seat.QrCodeUrl;
            ticket.Status = TicketStatus.Active;
            ticket.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _ticketRepository.UpdateRangeAsync(tickets.Values, cancellationToken);

        return _mapper.Map<IReadOnlyList<TicketDto>>(tickets.Values.ToList());
    }

    private static void EnsureStaffUser(CurrentUserInfo user)
    {
        if (!user.IsAuthenticated || !user.IsStaff)
        {
            throw new ForbiddenException("Only staff can check in tickets.");
        }
    }

    private static void EnsureTicketCanBeUsed(Ticket ticket)
    {
        if (ticket.Status == TicketStatus.Used)
        {
            throw new ConflictException("Ticket has already been used.");
        }

        if (ticket.Status != TicketStatus.Active)
        {
            throw new ConflictException("Only active tickets can be checked in.");
        }
    }
}
