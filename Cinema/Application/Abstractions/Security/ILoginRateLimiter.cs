namespace Application.Abstractions.Security;

public interface ILoginRateLimiter
{
    Task EnsureNotLimitedAsync(string email, CancellationToken cancellationToken = default);
    Task RegisterFailureAsync(string email, CancellationToken cancellationToken = default);
    Task ResetAsync(string email, CancellationToken cancellationToken = default);
}
