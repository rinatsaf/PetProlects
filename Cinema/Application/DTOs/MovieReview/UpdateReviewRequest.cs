namespace Application.DTOs.MovieReview;

public sealed class UpdateReviewRequest
{
    public int Rating { get; set; }   
    public string? Comment { get; set; }
}