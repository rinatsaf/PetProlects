using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;

namespace TouristRoutes.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PoiController(IPoiService poiService) : ControllerBase
{
    private readonly IPoiService _poiService = poiService;

    [HttpGet("city/{cityId:int}")]
    public async Task<ActionResult<List<PoiDto>>> GetPoisByCity(int cityId)
    {
        var pois = await _poiService.GetPoisByCityIdAsync(cityId);
        return Ok(pois);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PoiDto>> GetPoiById(int id)
    {
        var poi = await _poiService.GetPoiByIdAsync(id);
        return Ok(poi);
    }

    [HttpGet("name/{name}")]
    public async Task<ActionResult<PoiDto>> GetPoisByName(string name)
    {
        var poi = await _poiService.GetPoiByNameAsync(name);
        return Ok(poi);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<PoiDto>> CreatePoi([FromBody] CreatePoiRequest poiDto)
    {
        var poi = await _poiService.CreatePoiAsync(poiDto);
        return CreatedAtAction(nameof(GetPoiById), new { id = poi.Id }, poi);

    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}")]
    public async Task<ActionResult<PoiDto>> UpdatePoi(int id, [FromBody] UpdatePoiRequest poiDto)
    {
        var updated = await _poiService.UpdatePoiAsync(poiDto, id);
        return Ok(updated);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePoiById(int id)
    {
        await _poiService.DeleteByIdAsync(id);
        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("name/{name}")]
    public async Task<ActionResult> DeletePoiByName(string name)
    {
        await _poiService.DeleteByNameAsync(name);
        return NoContent();
    }
}

