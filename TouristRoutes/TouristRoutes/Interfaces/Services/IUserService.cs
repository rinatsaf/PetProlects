using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Interfaces.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto> GetUserByIdAsync(int id);
    Task<UserDto> GetUserByEmailAsync(string email);
    Task<UserDto> GetUserByUsernameAsync(string username);

    Task<UserDto> CreateUserAsync(CreateUserRequest request);

    Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request);

    Task DeleteUserByIdAsync(int id);
    Task DeleteUserByEmailAsync(string email);
    bool VerifyPassword(User user, string password);
    Task<User> GetDomainUserByEmailAsync(string requestEmail);
}