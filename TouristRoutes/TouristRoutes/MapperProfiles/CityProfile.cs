using AutoMapper;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.MapperProfiles;

public class CityProfile : Profile
{
    public CityProfile()
    {
        CreateMap<City, CityDto>();
        CreateMap<CreateCityRequest, City>();
        CreateMap<UpdateCityRequest, City>()
            .ForAllMembers(opts =>
                opts.Condition((src, dest, srcValue) 
                    => srcValue != null));
    }
}