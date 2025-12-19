using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Interfaces.Services;

public interface IPoiService
{
    Task<List<PoiDto>> GetPoisByCityIdAsync(int cityId);
    Task<PoiDto> GetPoiByIdAsync(int id);
    Task<PoiDto> GetPoiByNameAsync(string name);
    
    Task<PoiDto> CreatePoiAsync(CreatePoiRequest poi);
    Task<PoiDto> UpdatePoiAsync(UpdatePoiRequest poi, int id);
    
    Task DeleteByIdAsync(int id);
    Task DeleteByNameAsync(string name);
}