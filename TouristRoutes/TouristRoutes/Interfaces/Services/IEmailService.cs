namespace TouristRoutes.Interfaces.Services;

public interface IEmailService
{
    Task Send2FaCodeAsync(string email, string code);
}