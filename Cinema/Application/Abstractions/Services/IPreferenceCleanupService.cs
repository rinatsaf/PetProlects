namespace Application.Abstractions.Services;

public interface IPreferenceCleanupService
{
    Task<int> ApplyWeeklyDecayAsync(CancellationToken cancellationToken = default);
}
