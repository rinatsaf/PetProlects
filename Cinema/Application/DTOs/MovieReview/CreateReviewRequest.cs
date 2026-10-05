namespace Application.DTOs.MovieReview;

public sealed class CreateReviewRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}