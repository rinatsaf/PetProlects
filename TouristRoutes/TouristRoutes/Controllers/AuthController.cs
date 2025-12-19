using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;

namespace TouristRoutes.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserService userService, ILoginRateLimiter rateLimiter) : ControllerBase
{
    private readonly IUserService _userService = userService; 
    private readonly ILoginRateLimiter _rateLimiter = rateLimiter;

    [HttpGet("test-email")]
    public async Task<IActionResult> Test([FromServices] IEmailService email)
    {
        await email.Send2FaCodeAsync("test@example.com", "123456");
        return Ok("Sent!");
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
    {
        var createRequest = new CreateUserRequest
        {
            Email = request.Email,
            Password = request.Password,
            UserName = request.Username,
        };

        var created = await _userService.CreateUserAsync(createRequest);

        return Ok(created);
    }


    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest request)
    {
        var clientKey = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (!await _rateLimiter.IsAllowedAsync(clientKey))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests,
                "Too many login attempts. Try later.");
        }
        try
        {
            var domainUser = await _userService.GetDomainUserByEmailAsync(request.Email);

            if (!_userService.VerifyPassword(domainUser, request.Password))
            {
                await _rateLimiter.RegisterFailureAsync(clientKey);
                return Unauthorized("Invalid password");
            }

            await _rateLimiter.ResetAsync(clientKey);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, domainUser.Role.ToString()),
                new Claim(ClaimTypes.Email, domainUser.Email),
                new Claim(ClaimTypes.NameIdentifier, domainUser.Id.ToString()),
                new Claim(ClaimTypes.Name, domainUser.Username)
            };
            
            var identity = new ClaimsIdentity(claims, "Cookie");
            var principal = new ClaimsPrincipal(identity);
            
            await HttpContext.SignInAsync("Cookies", principal);

            return Ok(new { message = "Logged in" });
        }
        catch (KeyNotFoundException)
        {
            await _rateLimiter.RegisterFailureAsync(clientKey);
            return Unauthorized("User not found");
        }
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return Ok(new { message = "Logged out" });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            username = User.FindFirstValue(ClaimTypes.Name)
        });
    }
}
