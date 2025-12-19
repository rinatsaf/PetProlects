using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;

namespace TouristRoutes.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpGet("name/{name}")]
    public async Task<ActionResult<UserDto>> GetUserByName(string name)
    {
        var user = await _userService.GetUserByUsernameAsync(name);
        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest user)
    {
        var created = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
    }
 
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateUserAsync(id, request);
        return Ok(user);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUserById(int id)
    {
        await _userService.DeleteUserByIdAsync(id);
        return NoContent();
    }

    
    [HttpDelete("email/{email}")]
    public async Task<IActionResult> DeleteUserByEmail(string email)
    {
        await _userService.DeleteUserByEmailAsync(email);
        return NoContent();
    }
}
