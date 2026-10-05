namespace Application.DTOs.Sessions;

public sealed class SeatMapDto
{
    public string HallName { get; set; } = null!;
    public string HallAddress { get; set; } = null!;
    public int RowsCount { get; set; }
    public int SeatsPerRow { get; set; }
    public string Type { get; set; } = null!;

    public IReadOnlyList<SeatMapSeatDto> Seats { get; set; } = new List<SeatMapSeatDto>();
}