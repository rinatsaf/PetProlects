namespace Application.Abstractions.Security;

public interface ILoginRateLimiter
{
    Task EnsureNotLimitedAsync(string email, string ip, CancellationToken cancellationToken = default);
    Task RegisterFailureAsync(string email, string ip, CancellationToken cancellationToken = default);
    Task ResetAsync(string email, string ip, CancellationToken cancellationToken = default);
}
