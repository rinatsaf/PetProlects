using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public sealed class Movie : BaseEntity
{
    public required string Title { get; set; }
    [MaxLength(500)]
    public required string Description { get; set; }
    public int DurationMinutes { get; set; }
    public required string AgeRating { get; set; }
    public required string Country { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public string? PosterUrl { get; set; }
    public decimal PopularityScore { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<UserInteraction> Interactions { get; set; } = new List<UserInteraction>();
    public ICollection<MovieReview> Reviews { get; set; } = new List<MovieReview>();
}
