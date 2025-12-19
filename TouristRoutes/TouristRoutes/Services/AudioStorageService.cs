using Amazon.S3;
using Amazon.S3.Model;
using TouristRoutes.Interfaces.Services;
using TouristRoutes.Models.Entity;

public class AudioStorageService : IAudioStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly IAudioGenerationService _audioGenerationService;
    
    private readonly ICacheService _cache;
    private const string AudioKeyPrefix = "audio:";

    public AudioStorageService(IConfiguration configuration, IAudioGenerationService audioGenerationService, ICacheService cache)
    {
        _audioGenerationService = audioGenerationService;
        _cache = cache;
        var s3Section = configuration.GetSection("S3");

        _bucketName = s3Section["BucketName"]
            ?? throw new InvalidOperationException("S3:BucketName is missing");

        var accessKey = s3Section["AccessKey"];
        var secretKey = s3Section["SecretKey"];
        var serviceUrl = s3Section["ServiceURL"];

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(serviceUrl))
            throw new InvalidOperationException("S3 credentials or ServiceURL are missing");

        _s3 = new AmazonS3Client(
            accessKey,
            secretKey,
            new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = bool.Parse(s3Section["ForcePathStyle"] ?? "true")
            });
    }

    public async Task<string> UploadAudioAsync(string description)
    {
        byte[] audioBytes = await _audioGenerationService.GenerateAudioFromTextAsync(description);
        
        var fileName = $"poi-{Guid.NewGuid()}.mp3";
        var key = $"audio/{fileName}";
        
        using var stream = new MemoryStream(audioBytes);
        
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = "audio/mpeg"
        };
        
        await _s3.PutObjectAsync(request);
        
        await _cache.SetAsync($"{AudioKeyPrefix}{key}", audioBytes,  TimeSpan.FromDays(7));
        
        return key;
    }


    public async Task<AudioFile?> DownloadAudioAsync(string key)
    {
        try
        {
            var cachedKey = $"{AudioKeyPrefix}{key}";
            var cachedBytes = await _cache.GetAsync<byte[]>(cachedKey);
            if (cachedBytes != null)
            {
                return new AudioFile
                {
                    FilePath = key,
                    Content = cachedBytes,
                    ContentType = "audio/mp3"
                };
            }
            
            var response = await _s3.GetObjectAsync(_bucketName, key);

            await using var responseStream = response.ResponseStream;
            using var ms = new MemoryStream();
            await responseStream.CopyToAsync(ms);

            return new AudioFile
            {
                FilePath = key,
                Content = ms.ToArray(),
                ContentType = response.Headers.ContentType ?? "audio/mpeg"
            };
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<AudioFile>> DownloadManyAudioAsync(IEnumerable<string> keys)
    {
        var result = new List<AudioFile>();

        foreach (var key in keys.Distinct())
        {
            var audio = await DownloadAudioAsync(key);
            if (audio != null)
                result.Add(audio);
        }

        return result;
    }

    public async Task DeleteAudioAsync(string key)
    {
        await _cache.RemoveAsync($"{AudioKeyPrefix}{key}");
        await _s3.DeleteObjectAsync(_bucketName, key);
    }

    public async Task<string> UpdateAudioAsync(string description, string key)
    {
        await _cache.RemoveAsync($"{AudioKeyPrefix}{key}");
        await _s3.DeleteObjectAsync(_bucketName, key);
        var newKey = await UploadAudioAsync(description);
        return newKey;
    }
}
