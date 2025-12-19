using AutoMapper;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.MapperProfiles;

public class PoiProfile : Profile
{
    public PoiProfile()
    {
        // CREATE DTO → ENTITY
        CreateMap<CreatePoiRequest, Poi>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())              // Id генерируется БД
            .ForMember(dest => dest.AudioFilePath, opt => opt.Ignore());  // добавляется сервисом после генерации аудио

        // UPDATE DTO → ENTITY
        CreateMap<UpdatePoiRequest, Poi>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())              // ID не меняем
            .ForMember(dest => dest.AudioFilePath, opt => opt.Ignore())   // не перетираем существующий путь
            .ForAllMembers(opts =>
                opts.Condition((_, _, srcValue) => srcValue != null));    // не затираем null значениями

        // ENTITY → DTO
        CreateMap<Poi, PoiDto>();
    }
}
