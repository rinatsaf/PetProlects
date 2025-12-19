namespace TouristRoutes.Interfaces.Services;

public interface ILoginRateLimiter
{
    Task<bool> IsAllowedAsync(string key);  
    Task RegisterFailureAsync(string key);
    Task ResetAsync(string key);   
}