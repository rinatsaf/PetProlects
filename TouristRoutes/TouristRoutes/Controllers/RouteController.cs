using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;

namespace TouristRoutes.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RouteController(IRouteService routeService, IUserService userService) : ControllerBase
{
    private readonly IRouteService _routeService = routeService;
    private readonly IUserService _userService = userService;

    [HttpGet]
    public async Task<ActionResult<List<RouteResponse>>> GetRoutes()
    {
        if (IsAdmin())
            return Ok(await _routeService.GetAllRoutesAsync());

        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Forbid();

        return Ok(await _routeService.GetAllRoutesByUserAsync(currentUserId.Value));
    }

    [HttpGet("userId/{userId:int}")]
    public async Task<ActionResult<List<RouteResponse>>> GetRoutesByUserId(int userId)
    {
        if (!IsAdmin() && GetCurrentUserId() != userId)
            return Forbid();

        return Ok(await _routeService.GetAllRoutesByUserAsync(userId));
    }
    
    [HttpGet("email/{email}")]
    public async Task<ActionResult<List<RouteResponse>>> GetByUserEmail(string email)
    {
        if (!IsAdmin() && !string.Equals(GetCurrentUserEmail(), email, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var routes = await _routeService.GetAllRoutesByUserEmailAsync(email);
        return Ok(routes);
    }
    
    [HttpPost("user/{userId:int}")]
    public async Task<ActionResult<RouteResponse>> CreateRoute(int userId,[FromBody] RouteBuildRequest request)
    {
        if (!IsAdmin() && GetCurrentUserId() != userId)
            return Forbid();

        var route = await _routeService.CreateRouteAsync(request, userId);
        return Ok(route);
    }

    [HttpPost("preview")]
    public async Task<ActionResult<RouteResponse>> PreviewRoute([FromBody] RouteBuildRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Forbid();

        var route = await _routeService.PreviewRouteAsync(request, currentUserId.Value);
        return Ok(route);
    }
    
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RouteResponse>> UpdateRoute(int id,[FromBody] UpdateRouteRequest request)
    {
        if (!IsAdmin() && !await UserOwnsRouteAsync(id))
            return Forbid();

        var updated = await _routeService.UpdateRouteAsync(id, request);
        return Ok(updated);
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        if (!IsAdmin() && !await UserOwnsRouteAsync(id))
            return Forbid();

        await _routeService.DeleteRouteByIdAsync(id);
        return NoContent();
    }

    private bool IsAdmin() => User.IsInRole("Admin");

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }

    private string? GetCurrentUserEmail() => User.FindFirstValue(ClaimTypes.Email);

    private async Task<bool> UserOwnsRouteAsync(int routeId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return false;

        var routes = await _routeService.GetAllRoutesByUserAsync(currentUserId.Value);
        return routes.Any(r => r.Id == routeId);
    }
}
