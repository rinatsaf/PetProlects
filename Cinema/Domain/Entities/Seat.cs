using Domain.Common;

namespace Domain.Entities;

public sealed class Seat : BaseEntity
{
    public long HallId { get; set; }
    public required Hall Hall { get; set; } 

    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public required string SeatType { get; set; }
    public decimal BasePrice { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}