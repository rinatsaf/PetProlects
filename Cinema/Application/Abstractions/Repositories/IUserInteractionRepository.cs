using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

public interface IUserInteractionRepository
{
    Task AddAsync(UserInteraction interaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserInteraction>> GetByUserAsync(long userId, InteractionType? type = null, CancellationToken cancellationToken = default);
}
