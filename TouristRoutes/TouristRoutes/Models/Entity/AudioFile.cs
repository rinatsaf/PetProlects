namespace TouristRoutes.Models.Entity;

public class AudioFile
{
    public string FilePath { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "audio/mp3";
    public int DurationSec { get; set; }
    public string FileName => Path.GetFileName(FilePath);
}