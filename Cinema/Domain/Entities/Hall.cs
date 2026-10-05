using Domain.Common;

namespace Domain.Entities;

public sealed class Hall : BaseEntity
{
    public long CreatedByUserId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int RowsCount { get; set; }
    public int SeatsPerRow { get; set; }
    public string Type { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
