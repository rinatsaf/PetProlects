using Application.Abstractions.Services;
using Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Returns profile for current authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User profile.</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetProfileAsync(CancellationToken ct)
    {
        var profile = await userService.GetProfileAsync(ct);
        return Ok(profile);
    }
}
