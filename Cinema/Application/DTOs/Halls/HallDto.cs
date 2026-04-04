namespace Application.DTOs.Halls;

public sealed class HallDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public int RowsCount { get; set; }
    public int SeatsPerRow { get; set; }
    public string Type { get; set; } = null!;
    public bool IsActive { get; set; }
}
