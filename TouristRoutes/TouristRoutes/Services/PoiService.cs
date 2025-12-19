using AutoMapper;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;
using TouristRoutes.Repositories;

namespace TouristRoutes.Services;

public class PoiService(IMapper mapper, IPoiRepository poiRepository, IAudioStorageService audioStorageService, ICityRepository cityRepository, ICacheService cacheService) : IPoiService
{
    private readonly IMapper _mapper = mapper;
    private readonly IPoiRepository  _poiRepository = poiRepository;
    private readonly IAudioStorageService _audioStorageService = audioStorageService;
    private readonly ICityRepository _cityRepository = cityRepository;
    private readonly ICacheService _cacheService = cacheService;
    private const string Key = "pois:city:";
    public async Task<List<PoiDto>> GetPoisByCityIdAsync(int cityId)
    {
        var cacheKey = $"{Key}{cityId}";
        
        var cached = await _cacheService.GetAsync<List<PoiDto>>(cacheKey);
        if (cached != null) return cached;
        
        var pois = await _poiRepository.GetPoisByCityId(cityId); 
        var dtos = _mapper.Map<List<PoiDto>>(pois);
        
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromHours(12));
        return dtos;
    }

    public async Task<PoiDto> GetPoiByIdAsync(int id)
    {
        var poi = await _poiRepository.GetPoiByIdAsync(id);
        return _mapper.Map<PoiDto>(poi);
    }

    public async Task<PoiDto> GetPoiByNameAsync(string name)
    {
        var poi = await _poiRepository.GetPoiByNameAsync(name);
        return _mapper.Map<PoiDto>(poi);
    }

    public async Task<PoiDto> CreatePoiAsync(CreatePoiRequest poi)
    {
        var cityId = (await _cityRepository.GetCityByNameAsync(poi.CityName)).Id;
        var poiEntity = _mapper.Map<Poi>(poi);
        if (poi.Description != null)
        {
            var filePath = await _audioStorageService.UploadAudioAsync(poi.Description);
            poiEntity.AudioFilePath = filePath;
        }
        poiEntity.CityId = cityId;
        var created = await _poiRepository.CreatePoiAsync(poiEntity);
        
        var cacheKey = $"{Key}{created.CityId}";
        await _cacheService.RemoveAsync(cacheKey);
        
        return _mapper.Map<PoiDto>(created);
    }

    public async Task<PoiDto> UpdatePoiAsync(UpdatePoiRequest poi, int id)
    {
        var existingPoi = await _poiRepository.GetPoiByIdAsync(id);
        if  (existingPoi == null)
            throw new KeyNotFoundException("Poi not found");

        var oldDescription = existingPoi.Description;
        var oldCityId = existingPoi.CityId;

        _mapper.Map(poi, existingPoi);

        var descriptionChanged = existingPoi.Description != null &&
                                 existingPoi.Description != oldDescription;

        if (descriptionChanged)
        {
            var newDescription = existingPoi.Description!;
            var currentAudioKey = existingPoi.AudioFilePath;

            existingPoi.AudioFilePath = string.IsNullOrWhiteSpace(currentAudioKey)
                ? await _audioStorageService.UploadAudioAsync(newDescription)
                : await _audioStorageService.UpdateAudioAsync(newDescription, currentAudioKey);
        }

        await _poiRepository.UpdatePoiAsync(existingPoi);
        
        var cacheKey = $"{Key}{existingPoi.CityId}";
        await _cacheService.RemoveAsync(cacheKey);

        if (existingPoi.CityId != oldCityId)
        {
            await _cacheService.RemoveAsync($"{Key}{oldCityId}");
        }
        
        return _mapper.Map<PoiDto>(existingPoi);
    }

    public async Task DeleteByIdAsync(int id)
    {
        var poi = await _poiRepository.GetPoiByIdAsync(id);
        if (poi == null)
            throw new KeyNotFoundException("Poi not found");

        if (!String.IsNullOrWhiteSpace(poi.AudioFilePath))
        {
            await _audioStorageService.DeleteAudioAsync(poi.AudioFilePath);
        }

        await _poiRepository.DeletePoiByIdAsync(id);
        
        var cacheKey = $"{Key}{poi.CityId}";
        await _cacheService.RemoveAsync(cacheKey);
    }

    public async Task DeleteByNameAsync(string name)
    {
        var poi = await _poiRepository.GetPoiByNameAsync(name);
        if (poi == null)
            throw new KeyNotFoundException("Poi not found");

        if (!String.IsNullOrWhiteSpace(poi.AudioFilePath))
        {
            await _audioStorageService.DeleteAudioAsync(poi.AudioFilePath);
        }

        await _poiRepository.DeletePoiByNameAsync(name);
        
        var cacheKey = $"{Key}{poi.CityId}";
        await _cacheService.RemoveAsync(cacheKey);
    }
}
