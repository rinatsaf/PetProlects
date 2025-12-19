using AutoMapper;
using TouristRoutes.Interfaces.Repositories;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;
using TouristRoutes.Repositories;

namespace TouristRoutes.Services;

public class RouteService(
    IRouteRepository routeRepository, 
    IMapper mapper, IRoutePlanner routePlanner, 
    IUserRepository userRepository, 
    ICacheService cacheService,
    IPoiRepository poiRepository,
    ICityRepository cityRepository) : IRouteService
{
    private readonly IRouteRepository _routeRepository = routeRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IRoutePlanner _routePlanner = routePlanner;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IPoiRepository _poiRepository = poiRepository;
    private readonly ICityRepository _cityRepository = cityRepository;
    private static readonly TimeSpan RouteCacheTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan RouteVersionTtl = TimeSpan.FromDays(30);
    private const string UserVersionPrefix = "routever:user:";
    private const string GlobalVersionKey = "routever:global";
    
    public async Task<List<RouteResponse>> GetAllRoutesAsync()
    {
        var  routes = await _routeRepository.GetAllRoutesAsync();
        return routes.Select(SanitizeRouteFromEntity).ToList();
    }

    public async Task<List<RouteResponse>> GetAllRoutesByUserAsync(int userId)
    {
        var routes = await _routeRepository.GetAllRoutesByUserAsync(userId);
        return routes.Select(SanitizeRouteFromEntity).ToList();
    }

    public async Task<List<RouteResponse>> GetAllRoutesByUserEmailAsync(string email)
    {
        var routes = await _routeRepository.GetAllRoutesByUserAsync(email);
        return routes.Select(SanitizeRouteFromEntity).ToList();
    }

    public async Task<RouteResponse> CreateRouteAsync(RouteBuildRequest request, int userId)
    {
        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("EndTime must be greater than StartTime");

        var key = await BuildRouteCacheKey(request, userId);
        
        var cached = await _cacheService.GetAsync<RouteResponse>(key);
        if (cached != null)
            return cached;
        
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        
        var city = await _cityRepository.GetCityByNameAsync(request.City);
        var pois = await _poiRepository.GetPoisByCityId(city.Id);
        
        var route = _routePlanner.PlanRoute(request, pois);
        if (!string.IsNullOrWhiteSpace(request.Name))
            route.Name = request.Name;
        
        var routeToSave = _mapper.Map<TourRoute>(route);
        routeToSave.UserId = userId;
        routeToSave.Name = string.IsNullOrWhiteSpace(route.Name)
            ? $"Route {city.Name} {DateTime.UtcNow:yyyyMMddHHmmss}"
            : route.Name;
        routeToSave.CreatedAt = DateTime.Now;
        foreach (var poi in route.Points)
            routeToSave.Pois.Add(poi.Poi);
        
        var savedRoute = await _routeRepository.SaveRouteAsync(routeToSave);
        route.Id = savedRoute.Id;
        route.UserId = savedRoute.UserId;
        route.Name = savedRoute.Name;

        var response = SanitizeRoute(route);
        
        await _cacheService.SetAsync(key, response, RouteCacheTtl);
        
        return response;
    }

    public async Task<RouteResponse> PreviewRouteAsync(RouteBuildRequest request, int userId)
    {
        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("EndTime must be greater than StartTime");

        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");
        
        var city = await _cityRepository.GetCityByNameAsync(request.City);
        var pois = await _poiRepository.GetPoisByCityId(city.Id);

        var route = _routePlanner.PlanRoute(request, pois);
        route.UserId = userId;
        if (!string.IsNullOrWhiteSpace(request.Name))
            route.Name = request.Name;

        return SanitizeRoute(route);
    }

    public async Task<RouteResponse> UpdateRouteAsync(int id, UpdateRouteRequest request)
    {
        var existingRoute = await _routeRepository.GetTourRouteByIdAsync(id);
        if (existingRoute == null)
            throw new KeyNotFoundException($"Route with id {id} not found");
        
        _mapper.Map(request, existingRoute);
        
        var updatedRoute = await _routeRepository.UpdateRouteAsync(existingRoute);
        await BumpUserRouteVersion(updatedRoute.UserId);
        return SanitizeRouteFromEntity(updatedRoute);
    }

    public async Task DeleteRouteByIdAsync(int id)
    {
        var route = await _routeRepository.GetTourRouteByIdAsync(id)
                    ?? throw new KeyNotFoundException("Route not found");
        
        await _routeRepository.DeleteRouteByIdAsync(id);
        await BumpUserRouteVersion(route.UserId);
    }
    
    private static RouteResponse SanitizeRoute(RouteResponse route)
    {
        return new RouteResponse
        {
            Id = route.Id,
            Name = route.Name,
            UserId = route.UserId,
            TotalCost = route.TotalCost,
            TotalDuration = route.TotalDuration,
            TotalTravelTime = route.TotalTravelTime,
            TotalDistanceKm = route.TotalDistanceKm,
            RouteSummary = route.RouteSummary,
            Points = route.Points.Select(p => new RoutePoint
            {
                ArrivalTime = p.ArrivalTime,
                DepartureTime = p.DepartureTime,
                DistanceToNextKm = p.DistanceToNextKm,
                TravelTimeToNextMin = p.TravelTimeToNextMin,
                Poi = ShallowPoi(p.Poi)
            }).ToList()
        };
    }

    private static RouteResponse SanitizeRouteFromEntity(TourRoute entity)
    {
        return new RouteResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            UserId = entity.UserId,
            TotalCost = entity.TotalCost,
            TotalDuration = entity.TotalDuration,
            TotalTravelTime = entity.TotalTravelTime,
            TotalDistanceKm = entity.TotalDistanceKm,
            RouteSummary = entity.RouteSummary,
            Points = entity.Pois.Select(p => new RoutePoint
            {
                ArrivalTime = TimeSpan.Zero,
                DepartureTime = TimeSpan.Zero,
                DistanceToNextKm = 0,
                TravelTimeToNextMin = 0,
                Poi = ShallowPoi(p)
            }).ToList()
        };
    }

    private static Poi ShallowPoi(Poi poi) => new Poi
    {
        Id = poi.Id,
        Name = poi.Name,
        Lat = poi.Lat,
        Lon = poi.Lon,
        Category = poi.Category,
        VisitDurationMin = poi.VisitDurationMin,
        EntranceFeeCents = poi.EntranceFeeCents,
        OpeningHoursJson = poi.OpeningHoursJson,
        AudioFilePath = poi.AudioFilePath,
        Rating = poi.Rating,
        Description = poi.Description,
        DistanceKm = poi.DistanceKm,
        CreatedAt = poi.CreatedAt,
        UpdatedAt = poi.UpdatedAt,
        CityId = poi.CityId,
        City = null!,
        Routes = new List<TourRoute>()
    };
    
    private async Task<string> BuildRouteCacheKey(RouteBuildRequest request, int userId)
    {
        var userVersion = await GetRouteVersion(UserVersionPrefix + userId);
        var globalVersion = await GetRouteVersion(GlobalVersionKey);

        var city = request.City.Trim().ToLower();

        var categories = request.PreferredCategories is { Count: > 0 }
            ? string.Join("|", request.PreferredCategories
                .Select(c => c.Trim().ToLower())
                .OrderBy(c => c))
            : "none";

        var transport = string.IsNullOrWhiteSpace(request.TransportType)
            ? "unknown"
            : request.TransportType.Trim().ToLower();
        
        var name = string.IsNullOrWhiteSpace(request.Name)
            ? "none"
            : request.Name.Trim().ToLower();

        return 
            $"route:{userId};" +
            $"ver={globalVersion}:{userVersion};" +
            $"city={city};" +
            $"budget={request.Budget};" +
            $"coords={request.UserLat:F6}_{request.UserLon:F6};" +
            $"time={request.StartTime}-{request.EndTime};" +
            $"transport={transport};" +
            $"name={name};" +
            $"cats={categories}";
    }

    private async Task<int> GetRouteVersion(string key)
    {
        var version = await _cacheService.GetAsync<int?>(key);
        if (version is null)
        {
            await _cacheService.SetAsync(key, 1, RouteVersionTtl);
            return 1;
        }

        return version.Value;
    }

    private async Task BumpUserRouteVersion(int userId)
    {
        var key = UserVersionPrefix + userId;
        var current = await GetRouteVersion(key);
        await _cacheService.SetAsync(key, current + 1, RouteVersionTtl);
    }
}
