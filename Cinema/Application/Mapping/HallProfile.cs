using Application.DTOs.Halls;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class HallProfile : Profile
{
    public HallProfile()
    {
        CreateMap<Hall, HallDto>();
        CreateMap<CreateHallRequest, Hall>();
        CreateMap<UpdateHallRequest, Hall>();
    }
}
