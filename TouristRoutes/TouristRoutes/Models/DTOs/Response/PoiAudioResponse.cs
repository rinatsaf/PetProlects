using TouristRoutes.Models.Entity;

namespace TouristRoutes.Models.DTOs.Response;

public class PoiAudioResponse
{
    public int PoiId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AudioFile? Audio { get; set; }
    public bool HasAudio => Audio != null;
}