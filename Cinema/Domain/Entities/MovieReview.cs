using Domain.Common;

namespace Domain.Entities;

public class MovieReview : BaseEntity
{
    public long UserId { get; set; }
    public long MovieId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; } 
    
    public User? User { get; set; }
    public Movie? Movie { get; set; }
}