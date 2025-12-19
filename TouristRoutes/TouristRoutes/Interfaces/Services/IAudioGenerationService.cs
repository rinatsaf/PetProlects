namespace TouristRoutes.Interfaces.Services;

public interface IAudioGenerationService
{
    Task<byte[]> GenerateAudioFromTextAsync(string text);
}
