using TouristRoutes.Models.DTOs.Request;
using TouristRoutes.Models.Entity;

namespace TouristRoutes.Interfaces.Services;

public interface IAudioStorageService
{
    Task<string> UploadAudioAsync(string description);
    Task<AudioFile?> DownloadAudioAsync(string key);              // key = то, что сохранили
    Task<IReadOnlyList<AudioFile>> DownloadManyAudioAsync(IEnumerable<string> keys);
    Task DeleteAudioAsync(string key);
    Task<string> UpdateAudioAsync(string description, string key);
}
