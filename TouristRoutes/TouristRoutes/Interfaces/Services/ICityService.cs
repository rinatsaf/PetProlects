using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;

namespace TouristRoutes.Interfaces.Services;

public interface ICityService
{
    Task<List<CityDto>> GetAllAsync();
    Task<CityDto> GetByIdAsync(int id);
    Task<CityDto> GetByNameAsync(string name);

    Task<CityDto> CreateAsync(CreateCityRequest dto);
    Task<CityDto> UpdateAsync(int id, UpdateCityRequest dto);

    Task DeleteByIdAsync(int id);
    Task DeleteByNameAsync(string name);
}