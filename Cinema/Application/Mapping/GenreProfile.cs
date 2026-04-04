using Application.DTOs.Genres;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class GenreProfile : Profile
{
    public GenreProfile()
    {
        CreateMap<Genre, GenreDto>();
        CreateMap<CreateGenreRequest, Genre>();
        CreateMap<UpdateGenreRequest, Genre>();
    }
}
