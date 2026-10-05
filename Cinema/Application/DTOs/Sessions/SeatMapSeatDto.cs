namespace Application.DTOs.Sessions;

public sealed class SeatMapSeatDto
{
    public long Id { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public string SeatType { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
}