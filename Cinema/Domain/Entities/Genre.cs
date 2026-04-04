using Domain.Common;

namespace Domain.Entities;

public sealed class Genre : BaseEntity
{
    public string Name { get; set; } = null!;

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}