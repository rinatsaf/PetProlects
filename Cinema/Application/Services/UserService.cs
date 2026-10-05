using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.DTOs.Sessions;
using Application.DTOs.Users;
using Application.Exceptions;
using AutoMapper;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IUserActiveCacheService userActiveCacheService,
    IMapper mapper) : IUserService
{
    public Task<SessionDto> GetRecommendedSessionsAsync(long id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<UserDto> GetProfileAsync(CancellationToken ct)
    {
        var current = currentUserService.GetCurrentUser();

        var user = await userRepository.GetByIdAsync(current.UserId, ct)
            ?? throw new NotFoundException("User not found");
        
        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateRoleAsync(long id, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await userRepository.ExistsByIdAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException($"User {id} not found");
        }

        var updated = await userRepository.UpdateRoleAsync(id, request.Role, cancellationToken);
        if (!updated)
        {
            throw new ConflictException($"Role for user {id} was not updated");
        }

        var user = await userRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException($"User {id} not found");

        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateStatusAsync(long id, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await userRepository.ExistsByIdAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException($"User {id} not found");
        }

        var updated = await userRepository.UpdateStatusAsync(id, request.IsActive, cancellationToken);
        if (!updated)
        {
            throw new ConflictException($"Status for user {id} was not updated");
        }

        await userActiveCacheService.InvalidateAsync(id, cancellationToken);

        var user = await userRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException($"User {id} not found");

        return mapper.Map<UserDto>(user);
    }
}
