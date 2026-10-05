using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Sessions;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Serilog;

namespace Application.Services;

public class SessionService(
    ISessionRepository sessionRepository,
    IMovieRepository movieRepository,
    IHallRepository hallRepository,
    IRecommendationService recommendationService,
    IMapper mapper,
    ICurrentUserService currentUserService,
    ISeatHoldService seatHoldService) : ISessionService
{
    private static readonly ILogger BusinessLogger = Log.ForContext("BusinessLog", true);
    private readonly ISessionRepository _sessionRepository = sessionRepository;
    private readonly IMovieRepository _movieRepository = movieRepository;
    private readonly IHallRepository _hallRepository = hallRepository;
    private readonly IRecommendationService _recommendationService = recommendationService;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ISeatHoldService _seatHoldService = seatHoldService;

    public async Task<IReadOnlyList<SessionDto>> GetUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetAllUpcomingAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<SessionDto>>(sessions);
    }

    public async Task<SessionDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Session with id {id} not found");
        return _mapper.Map<SessionDto>(session);
    }

    public async Task<IReadOnlyList<SessionScheduleDto>> GetScheduleAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository
            .GetByDateAsync(date, cancellationToken);
        
        return _mapper.Map<IReadOnlyList<SessionScheduleDto>>(sessions);
    }

    public async Task<SessionDto> CreateAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureStaffUser();
        var hall = await EnsureMovieAndHallExist(request.MovieId, request.HallId, cancellationToken);

        if (await _sessionRepository.HasOverlapAsync(request.HallId, request.StartTime, request.EndTime, null, cancellationToken))
        {
            throw new ConflictException("Session overlaps with existing session in this hall");
        }

        var session = _mapper.Map<Session>(request);
        session.CreatedByUserId = currentUser.UserId;

        var tickets = BuildSessionTickets(session, hall);
        var created = await _sessionRepository.AddWithTicketsAsync(session, tickets, cancellationToken);
        BusinessLogger.Information(
            "Session created: SessionId={SessionId}, HallId={HallId}, MovieId={MovieId}, TicketsCount={TicketsCount}, CreatedBy={CreatedBy}",
            created.Id,
            created.HallId,
            created.MovieId,
            tickets.Count,
            created.CreatedByUserId);
        return _mapper.Map<SessionDto>(created);
    }

    public async Task<SessionDto> UpdateAsync(long id, UpdateSessionRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureStaffUser();
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Session with id {id} not found");
        EnsureCanManageResource(currentUser, session.CreatedByUserId, $"Session {id}");

        if (session.HallId != request.HallId)
        {
            throw new ConflictException("Changing hall for an existing session is not supported because tickets are generated automatically.");
        }

        await EnsureMovieAndHallExist(request.MovieId, request.HallId, cancellationToken);

        if (await _sessionRepository.HasOverlapAsync(request.HallId, request.StartTime, request.EndTime, id, cancellationToken))
        {
            throw new ConflictException("Session overlaps with existing session in this hall");
        }

        _mapper.Map(request, session);
        session.UpdatedAt = DateTimeOffset.UtcNow;
        foreach (var ticket in session.Tickets.Where(x => x.Status == Domain.Enums.TicketStatus.Available))
        {
            ticket.Price = request.BasePrice;
            ticket.UpdatedAt = session.UpdatedAt;
        }

        var updated = await _sessionRepository.UpdateAsync(session, cancellationToken);
        BusinessLogger.Information("Session updated: SessionId={SessionId}, UpdatedBy={UpdatedBy}", updated.Id, currentUser.UserId);
        return _mapper.Map<SessionDto>(updated);
    }

    public async Task<SessionDto> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureStaffUser();
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Session with id {id} not found");
        EnsureCanManageResource(currentUser, session.CreatedByUserId, $"Session {id}");

        if (session.Tickets.Any(x => x.Status != Domain.Enums.TicketStatus.Available))
        {
            throw new ConflictException("Session cannot be deleted because it already has reserved or sold tickets.");
        }

        var deleted = await _sessionRepository.DeleteAsync(session, cancellationToken);
        BusinessLogger.Information("Session deleted: SessionId={SessionId}, DeletedBy={DeletedBy}", deleted.Id, currentUser.UserId);
        return _mapper.Map<SessionDto>(deleted);
    }

    public async Task<IReadOnlyList<SessionDto>> GetUpcomingByMovieAsync(long movieId, CancellationToken cancellationToken)
    {
        var sessions = await GetUpcomingAsync(cancellationToken);

        if (sessions.Any(x => x.MovieId == movieId))
        {
            var userId = _currentUserService
                .GetCurrentUser()
                .UserId;
            
            await _recommendationService.TrackInteractionAsync(userId, movieId, InteractionType.View, cancellationToken);
        }
        
        return sessions
            .Where(x => x.MovieId == movieId)
            .ToList();
    }

    public async Task<IReadOnlyList<SessionDto>> GetUpcomingByHallAsync(long hallId,
        CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetUpcomingByHallAsync(hallId, cancellationToken);
        return _mapper.Map<IReadOnlyList<SessionDto>>(sessions);
    }
    

    private async Task<Hall> EnsureMovieAndHallExist(long movieId, long hallId, CancellationToken cancellationToken)
    {
        if (!await _movieRepository.ExistsByIdAsync(movieId, cancellationToken))
        {
            throw new NotFoundException($"Movie {movieId} not found");
        }

        var hall = await _hallRepository.GetByIdAsync(hallId, cancellationToken);
        if (hall is null)
        {
            throw new NotFoundException($"Hall {hallId} not found");
        }

        return hall;
    }

    private CurrentUserInfo EnsureStaffUser()
    {
        var currentUser = _currentUserService.GetCurrentUser();
        if (!currentUser.IsAuthenticated || !currentUser.IsStaff)
        {
            throw new ForbiddenException("Only staff can manage sessions.");
        }

        return currentUser;
    }

    private static void EnsureCanManageResource(CurrentUserInfo currentUser, long createdByUserId, string resourceName)
    {
        if (currentUser.IsAdmin)
        {
            return;
        }

        if (createdByUserId != currentUser.UserId)
        {
            throw new ForbiddenException($"{resourceName} can only be changed by the staff member who created it.");
        }
    }

    private static List<Ticket> BuildSessionTickets(Session session, Hall hall)
    {
        var now = DateTimeOffset.UtcNow;
        return hall.Seats
            .OrderBy(x => x.RowNumber)
            .ThenBy(x => x.SeatNumber)
            .Select(seat => new Ticket
            {
                Session = session,
                SeatId = seat.Id,
                Price = session.BasePrice,
                TicketCode = "PENDING",
                Status = TicketStatus.Available,
                CreatedAt = now,
                UpdatedAt = now
            })
            .ToList();
    }

    public async Task<SeatMapDto> GetSeatMapAsync(long id, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException($"Session with id {id} not found");

        var hall = await _hallRepository.GetByIdAsync(session.HallId, cancellationToken) 
                    ?? throw new NotFoundException($"Hall with id {session.HallId} not found");

        var tickets = session.Tickets;

        var seats = hall.Seats
            .OrderBy(x => x.RowNumber)
            .ThenBy(x => x.SeatNumber)
            .Select(seat => new SeatMapSeatDto
            {
                Id = seat.Id,
                RowNumber = seat.RowNumber,
                SeatNumber = seat.SeatNumber,
                SeatType = seat.SeatType,
                BasePrice = seat.BasePrice,
                Status = GetSeatStatus(seat.Id, tickets)
            })
            .ToList();

        var seatMap = new SeatMapDto
        {
            HallName = hall.Name,
            HallAddress = hall.Address,
            RowsCount = hall.RowsCount,
            SeatsPerRow = hall.SeatsPerRow,
            Type = hall.Type,
            Seats = seats
        };

        await CheckReservedSeats(seats, session.Id, cancellationToken);

        return seatMap;
    }

    private static string GetSeatStatus(long seatId, IEnumerable<Ticket> tickets)
    {
        var ticket = tickets.FirstOrDefault(x => x.SeatId == seatId);
        if (ticket == null)
            throw new NotFoundException($"Ticket for seat {seatId} not found");

        return ticket.Status switch
        {
            TicketStatus.Active => "sold",
            TicketStatus.Available => "available",
            TicketStatus.Reserved => "reserved",
            TicketStatus.Used => "sold",
            TicketStatus.Cancelled => "cancelled",
            _ => throw new NotSupportedException($"Ticket status {ticket.Status} is not supported")
        };
    }

    private async Task CheckReservedSeats(IEnumerable<SeatMapSeatDto> seats, long sessionId, CancellationToken cancellationToken)
    {
        var seatMapSeatDtos = seats.ToArray();
        var reservedSeats = await _seatHoldService.GetHeldSeatIdsAsync(sessionId, seatMapSeatDtos.Select(x => x.Id), cancellationToken);
        
        var seatsId = reservedSeats.ToHashSet();

        foreach (var seat in seatMapSeatDtos){
            if (seatsId.Contains(seat.Id))
                seat.Status = "reserved";
        }
    }
}
