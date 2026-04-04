namespace Application.DTOs.Movies;

public class UpdateMovieRequest
{
    public string Title { get; set; } 
    public string Description { get; set; }
    public int DurationMinutes { get; set; }
    public string AgeRating { get; set; }
    public string Country { get; set; } 
    public required DateOnly ReleaseDate { get; set; }
    public string? PosterUrl { get; set; }
    public decimal PopularityScore { get; set; }
    public bool IsActive { get; set; }
}