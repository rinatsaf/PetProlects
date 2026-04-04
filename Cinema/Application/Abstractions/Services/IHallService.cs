using Application.DTOs.Halls;

namespace Application.Abstractions.Services;

public interface IHallService
{
    Task<IReadOnlyList<HallDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<HallDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<HallDto> CreateAsync(CreateHallRequest request, CancellationToken cancellationToken = default);
    Task<HallDto> UpdateAsync(long id, UpdateHallRequest request, CancellationToken cancellationToken = default);
    Task<HallDto> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
