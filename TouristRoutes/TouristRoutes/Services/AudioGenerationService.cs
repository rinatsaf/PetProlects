using Microsoft.CognitiveServices.Speech;
using TouristRoutes.Interfaces.Services;

namespace TouristRoutes.Services;

public class AudioGenerationService : IAudioGenerationService
{
    private readonly string _key;
    private readonly string _region;

    public AudioGenerationService(IConfiguration config)
    {
        _key = config["AzureSpeech:Key"] ??  throw new ArgumentNullException(nameof(config));
        _region = config["AzureSpeech:Region"] ??  throw new ArgumentNullException(nameof(config));
    }

    public async Task<byte[]> GenerateAudioFromTextAsync(string text)
    {
        var speechConfig = SpeechConfig.FromSubscription(_key, _region);
        
        speechConfig.SpeechSynthesisVoiceName = "ru-RU-DariyaNeural";
        speechConfig.SetSpeechSynthesisOutputFormat(
            SpeechSynthesisOutputFormat.Audio16Khz128KBitRateMonoMp3
        );

        using var synthesizer = new SpeechSynthesizer(speechConfig);

        var result = await synthesizer.SpeakTextAsync(text);

        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
            throw new Exception($"Speech synthesis failed: {result.Reason}");
        
        return result.AudioData;
    }
}