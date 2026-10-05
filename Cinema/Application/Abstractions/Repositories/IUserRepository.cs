using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> UpdateRoleAsync(long id, UserRole role, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(long id, bool isActive, CancellationToken cancellationToken = default);

}
