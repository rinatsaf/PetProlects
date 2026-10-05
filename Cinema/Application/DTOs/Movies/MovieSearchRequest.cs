namespace Application.DTOs.Movies;

public sealed class MovieSearchRequest
{
    /// <summary>
    /// Title contains filter.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Exact country filter.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Exact age rating filter.
    /// </summary>
    public string? AgeRating { get; set; }

    /// <summary>
    /// Exact genre name filter.
    /// </summary>
    public string? Genre { get; set; }

    /// <summary>
    /// Movie active status filter.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Release date lower bound (inclusive).
    /// </summary>
    public DateOnly? ReleaseDateFrom { get; set; }

    /// <summary>
    /// Release date upper bound (inclusive).
    /// </summary>
    public DateOnly? ReleaseDateTo { get; set; }

    /// <summary>
    /// Minimum movie duration in minutes.
    /// </summary>
    public int? MinDurationMinutes { get; set; }

    /// <summary>
    /// Maximum movie duration in minutes.
    /// </summary>
    public int? MaxDurationMinutes { get; set; }

    /// <summary>
    /// Minimum popularity score.
    /// </summary>
    public decimal? MinPopularityScore { get; set; }

    /// <summary>
    /// Maximum popularity score.
    /// </summary>
    public decimal? MaxPopularityScore { get; set; }

    /// <summary>
    /// Number of returned items.
    /// </summary>
    public int Limit { get; set; } = 20;
}
