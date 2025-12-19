using AutoMapper;
using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.DTOs.Response;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.MapperProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<CreateUserRequest, User>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.HashPassword, opt => opt.Ignore());

        CreateMap<UpdateUserRequest, User>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.HashPassword, opt => opt.Ignore());

        // ENTITY → DTO
        CreateMap<User, UserDto>();
    }
}