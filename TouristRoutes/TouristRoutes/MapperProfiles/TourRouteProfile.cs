using System;
using System.Linq;
using AutoMapper;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.MapperProfiles;

public class TourRouteProfile : Profile
{
    public TourRouteProfile()
    {
        // PLAN RESULT -> ENTITY
        CreateMap<RouteResponse, TourRoute>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.UserId, opt => opt.Ignore())
            .ForMember(x => x.Pois, opt => opt.Ignore());

        // UPDATE
        CreateMap<UpdateRouteRequest, TourRoute>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.UserId, opt => opt.Ignore());

        // ENTITY → DTO
        CreateMap<TourRoute, RouteResponse>()
            .ForMember(
                dest => dest.Points,
                opt => opt.MapFrom(src =>
                    src.Pois.Select(p => new RoutePoint
                    {
                        Poi = ShallowPoi(p),
                        ArrivalTime = TimeSpan.Zero,
                        DepartureTime = TimeSpan.Zero,
                        TravelTimeToNextMin = 0,
                        DistanceToNextKm = 0
                    }).ToList()
                ));
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
}
