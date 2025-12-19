using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;

namespace TouristRoutes.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CityController(ICityService cityService) : ControllerBase
{
    private readonly ICityService _cityService = cityService;

    [HttpGet]
    public async Task<ActionResult<List<CityDto>>> GetAll()
    {
        var cities = await _cityService.GetAllAsync();
        return Ok(cities);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CityDto>> GetById(int id)
    {
        var city = await _cityService.GetByIdAsync(id);
        return Ok(city);
    }

    [HttpGet("name/{name}")]
    public async Task<ActionResult<CityDto>> GetByName(string name)
    {
        var city = await _cityService.GetByNameAsync(name);
        return Ok(city);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<CityDto>> CreateAsync([FromBody] CreateCityRequest dto)
    {
        var city = await _cityService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = city.Id }, city);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CityDto>> UpdateAsync(int id,[FromBody] UpdateCityRequest dto)
    {
        var city = await _cityService.UpdateAsync(id, dto);
        return Ok(city);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteById(int id)
    {
        await _cityService.DeleteByIdAsync(id);
        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("name/{name}")]
    public async Task<ActionResult> DeleteByName(string name)
    {
        await _cityService.DeleteByNameAsync(name);
        return NoContent();
    }
}