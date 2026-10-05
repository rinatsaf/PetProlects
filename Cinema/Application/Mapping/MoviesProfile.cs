using Application.DTOs.MovieReview;
using Application.DTOs.Movies;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class MoviesProfile : Profile
{
    public MoviesProfile()
    {
        CreateMap<Movie, MovieDto>()
            .ForMember(dest => dest.GenreIds, opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.GenreId)))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.MovieGenres.Select(mg => mg.Genre)))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Reviews.Any() ? (double?)src.Reviews.Average(r => r.Rating) : null))
            .ForMember(dest => dest.ReviewsCount, opt => opt.MapFrom(src => src.Reviews.Count));

        CreateMap<CreateMovieRequest, Movie>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));

        CreateMap<UpdateMovieRequest, Movie>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));

        CreateMap<MovieReview, MovieReviewDto>();
    }
}
