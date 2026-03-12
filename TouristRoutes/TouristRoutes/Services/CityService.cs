using AutoMapper;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Services;

public class CityService(IMapper mapper, ICityRepository cityRepository) : ICityService
{
    private readonly ICityRepository _cityRepository = cityRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<List<CityDto>> GetAllAsync()
    {
        var cities = await _cityRepository.GetAllCitiesAsync();
        return _mapper.Map<List<CityDto>>(cities);
    }

    public async Task<CityDto> GetByIdAsync(int id)
    {
        var city = await _cityRepository.GetCityByIdAsync(id);
        return _mapper.Map<CityDto>(city);
    }

    public async Task<CityDto> GetByNameAsync(string name)
    {
        var city = await _cityRepository.GetCityByNameAsync(name);
        return _mapper.Map<CityDto>(city);
    }

    public async Task<CityDto> CreateAsync(CreateCityRequest dto)
    {
        var entity = _mapper.Map<City>(dto);
        entity.CreatedAt = DateTime.Now;
        var created = await _cityRepository.CreateCityAsync(entity);
        return _mapper.Map<CityDto>(created);
    }

    public async Task<CityDto> UpdateAsync(int id, UpdateCityRequest dto)
    {
        try
        {
            var city = await _cityRepository.GetCityByIdAsync(id);
            _mapper.Map(dto, city);
            var updeted =  await _cityRepository.UpdateCityAsync(city);
            return _mapper.Map<CityDto>(updeted);
        }
        catch (KeyNotFoundException ex)
        {
            throw new KeyNotFoundException(ex.Message);
        }
    }

    public async Task DeleteByIdAsync(int id)
    {
        await _cityRepository.DeleteCityByIdAsync(id);
    }

    public async Task DeleteByNameAsync(string name)
    {
        await _cityRepository.DeleteCityByNameAsync(name);
    }
}