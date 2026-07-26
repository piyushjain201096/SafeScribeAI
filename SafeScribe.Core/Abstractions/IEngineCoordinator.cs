using System;
using System.Threading.Tasks;

namespace SafeScribe.Core.Abstractions
{
    public class MeetingProcessDetectedEventArgs : EventArgs
    {
        public string PlatformName { get; set; } = string.Empty;
        public string MeetingTitle { get; set; } = string.Empty;
    }

    public interface IEngineCoordinator
    {
        IAudioCaptureService Audio { get; }
        ISpeechToTextService Speech { get; }
        ILlmService Llm { get; }
        IStorageService Storage { get; }

        event EventHandler<MeetingProcessDetectedEventArgs>? MeetingProcessDetected;

        void StartMeetingRecording(string meetingTitle);
        Task<string> StopMeetingRecordingAndSummarizeAsync(string templateId = "general");
        Task<string> ImportAudioAndSummarizeAsync(string filePath, string templateId = "general");
    }
}
