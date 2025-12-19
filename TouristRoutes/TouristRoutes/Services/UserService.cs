using AutoMapper;
using Microsoft.AspNetCore.Identity;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Services;

public class UserService(IMapper mapper, IUserRepository userRepository, IPasswordHasher<User> passwordHasher) : IUserService
{
    private readonly IMapper _mapper = mapper;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users =  await _userRepository.GetAllUsersAsync();
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var user = _mapper.Map<User>(request);
        user.HashPassword = _passwordHasher.HashPassword(user, request.Password);
        user.CreatedAt = DateTime.Now;
        var created = await _userRepository.CreateUserAsync(user);
        
        return _mapper.Map<UserDto>(created);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(id);
        if (existingUser == null)
            throw new KeyNotFoundException("User not found");

        _mapper.Map(request, existingUser);

        if (!String.IsNullOrWhiteSpace(request.Password))
        {
            existingUser.HashPassword = _passwordHasher.HashPassword(existingUser, request.Password);
        }
        var updated = await _userRepository.UpdateUserAsync(existingUser);
        return _mapper.Map<UserDto>(updated);
    }

    public async Task DeleteUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        
        if (user == null)
            throw new KeyNotFoundException("User not found");
        
        await _userRepository.DeleteUserAsync(id);
    }

    public async Task DeleteUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        
        if (user == null)
            throw new KeyNotFoundException("User not found");
        
        await _userRepository.DeleteUserAsync(user.Id);
    }
    public bool VerifyPassword(User user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, password);
        return result == PasswordVerificationResult.Success;
    }

    public async Task<User> GetDomainUserByEmailAsync(string requestEmail)
    {
        return await _userRepository.GetUserByEmailAsync(requestEmail);
    }
}
