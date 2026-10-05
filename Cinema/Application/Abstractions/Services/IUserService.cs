using Application.DTOs.Sessions;
using Application.DTOs.Users;

namespace Application.Abstractions.Services;

public interface IUserService
{
    Task<SessionDto> GetRecommendedSessionsAsync(long id, CancellationToken ct);
    Task<UserDto> GetProfileAsync(CancellationToken ct);
    Task<UserDto> UpdateRoleAsync(long id, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateStatusAsync(long id, UpdateUserStatusRequest request, CancellationToken cancellationToken = default);
}
