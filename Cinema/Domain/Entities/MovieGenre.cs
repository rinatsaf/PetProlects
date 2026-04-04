using Domain.Common;

namespace Domain.Entities;

public sealed class MovieGenre : BaseEntity
{
    public long MovieId { get; set; }
    public required Movie Movie { get; set; }

    public long GenreId { get; set; }
    public required Genre Genre { get; set; }
}