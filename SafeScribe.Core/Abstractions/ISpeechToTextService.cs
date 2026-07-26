using System.Threading.Tasks;

namespace SafeScribe.Core.Abstractions
{
    public interface ISpeechToTextService
    {
        Task<string> TranscribeAudioFileAsync(string audioFilePath);
        Task<string> TranscribeSegmentAsync(float[] audioSamples, int sampleRate);
        Task InitializeModelAsync(string modelPath);
    }
}
