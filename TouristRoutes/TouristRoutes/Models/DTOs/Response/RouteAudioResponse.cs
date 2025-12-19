namespace TouristRoutes.Models.DTOs.Response;

public class RouteAudioResponse
{
    public List<PoiAudioResponse> PoisWithAudio { get; set; } = new();
    public int TotalAudioFiles => PoisWithAudio.Count(p => p.HasAudio);
    public int TotalDurationSec => PoisWithAudio.Where(p => p.HasAudio).Sum(p => p.Audio!.DurationSec);
}