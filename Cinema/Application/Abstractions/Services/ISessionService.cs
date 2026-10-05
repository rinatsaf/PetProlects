using Application.DTOs.Sessions;

namespace Application.Abstractions.Services;

public interface ISessionService
{
    Task<IReadOnlyList<SessionDto>> GetUpcomingAsync(CancellationToken cancellationToken = default);
    Task<SessionDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionScheduleDto>> GetScheduleAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<SeatMapDto> GetSeatMapAsync(long id, CancellationToken cancellationToken = default);
    Task<SessionDto> CreateAsync(CreateSessionRequest request, CancellationToken cancellationToken = default);
    Task<SessionDto> UpdateAsync(long id, UpdateSessionRequest request, CancellationToken cancellationToken = default);
    Task<SessionDto> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionDto>> GetUpcomingByMovieAsync(long movieId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SessionDto>> GetUpcomingByHallAsync(long movieId, CancellationToken cancellationToken);
}
