namespace Application.DTOs.Tickets;

public sealed class CheckInByCodeRequest
{
    public string Code { get; set; } = string.Empty;
}
