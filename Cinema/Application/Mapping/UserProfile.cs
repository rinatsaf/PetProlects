using Application.DTOs.Users;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
    }
}
