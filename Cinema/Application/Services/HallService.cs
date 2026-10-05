using Application;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Halls;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Serilog;

namespace Application.Services;

public class HallService(
    IHallRepository hallRepository,
    IMapper mapper,
    ICurrentUserService currentUserService) : IHallService
{
    private static readonly ILogger BusinessLogger = Log.ForContext("BusinessLog", true);
    private readonly IHallRepository _hallRepository = hallRepository;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<IReadOnlyList<HallDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var halls = await _hallRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<HallDto>>(halls);
    }

    public async Task<HallDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var hall = await _hallRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException($"Hall with id {id} not found");
        return _mapper.Map<HallDto>(hall);
    }

    public async Task<HallDto> CreateAsync(CreateHallRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureStaffUser();

        if (await _hallRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new ConflictException("Hall with the same name already exists");
        }

        var hall = _mapper.Map<Hall>(request);
        hall.CreatedByUserId = currentUser.UserId;
        hall.Seats = BuildSeats(hall);
        var created = await _hallRepository.AddAsync(hall, cancellationToken);
        BusinessLogger.Information(
            "Hall created: HallId={HallId}, Name={Name}, Address={Address}, Rows={Rows}, SeatsPerRow={SeatsPerRow}, CreatedBy={CreatedBy}",
            created.Id,
            created.Name,
            created.Address,
            created.RowsCount,
            created.SeatsPerRow,
            created.CreatedByUserId);
        return _mapper.Map<HallDto>(created);
    }

    public async Task<HallDto> UpdateAsync(long id, UpdateHallRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureStaffUser();
        var hall = await _hallRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException($"Hall with id {id} not found");
        EnsureCanManageResource(currentUser, hall.CreatedByUserId, $"Hall {id}");

        if (hall.RowsCount != request.RowsCount || hall.SeatsPerRow != request.SeatsPerRow)
        {
            throw new ConflictException("Changing hall layout requires seat regeneration and is not supported by this endpoint.");
        }

        if (await _hallRepository.ExistsByNameAsync(request.Name, id, cancellationToken))
        {
            throw new ConflictException("Hall with the same name already exists");
        }

        _mapper.Map(request, hall);
        hall.UpdatedAt = DateTimeOffset.UtcNow;

        var updated = await _hallRepository.UpdateAsync(hall, cancellationToken);
        BusinessLogger.Information("Hall updated: HallId={HallId}, UpdatedBy={UpdatedBy}", updated.Id, currentUser.UserId);
        return _mapper.Map<HallDto>(updated);
    }

    public async Task<HallDto> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var currentUser = EnsureStaffUser();
        var hall = await _hallRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException($"Hall with id {id} not found");
        EnsureCanManageResource(currentUser, hall.CreatedByUserId, $"Hall {id}");

        var deleted = await _hallRepository.DeleteAsync(hall, cancellationToken);
        BusinessLogger.Information("Hall deleted: HallId={HallId}, DeletedBy={DeletedBy}", deleted.Id, currentUser.UserId);
        return _mapper.Map<HallDto>(deleted);
    }

    private CurrentUserInfo EnsureStaffUser()
    {
        var currentUser = _currentUserService.GetCurrentUser();
        if (!currentUser.IsAuthenticated || !currentUser.IsStaff)
        {
            throw new ForbiddenException("Only staff can manage halls.");
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

    private static List<Seat> BuildSeats(Hall hall)
    {
        var now = DateTimeOffset.UtcNow;
        var seats = new List<Seat>(hall.RowsCount * hall.SeatsPerRow);

        for (var row = 1; row <= hall.RowsCount; row++)
        {
            for (var seatNumber = 1; seatNumber <= hall.SeatsPerRow; seatNumber++)
            {
                seats.Add(new Seat
                {
                    Hall = hall,
                    RowNumber = row,
                    SeatNumber = seatNumber,
                    SeatType = "Standard",
                    BasePrice = 0,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
        }

        return seats;
    }
}
