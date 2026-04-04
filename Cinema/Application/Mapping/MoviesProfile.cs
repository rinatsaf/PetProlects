using Application.DTOs.Movies;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class MoviesProfile : Profile
{
    public MoviesProfile()
    {
        CreateMap<Movie, MovieDto>();

        CreateMap<CreateMovieRequest, Movie>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));

        CreateMap<UpdateMovieRequest, Movie>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));
    }
}
