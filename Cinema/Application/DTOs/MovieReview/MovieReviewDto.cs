namespace Application.DTOs.MovieReview;

public sealed class MovieReviewDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = null!;
    public long MovieId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}