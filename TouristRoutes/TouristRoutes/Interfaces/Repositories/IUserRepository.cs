using Microsoft.AspNetCore.Mvc;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Interfaces.Repositories;

public interface IUserRepository
{
    // GET
    Task<List<User>> GetAllUsersAsync();
    Task<User> GetUserByEmailAsync(string email);
    Task<User> GetUserByIdAsync(int id);
    Task<User> GetUserByUsernameAsync(string username);
    
    // POST
    Task<User> CreateUserAsync(User user);
    
    // PUT
    Task<User> UpdateUserAsync(User user);
    
    // DELETE
    Task DeleteUserAsync(int id);
    Task DeleteUserByEmailAsync(string email);
}