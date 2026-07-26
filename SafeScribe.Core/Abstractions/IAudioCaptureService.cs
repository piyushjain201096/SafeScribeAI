using System;

namespace SafeScribe.Core.Abstractions
{
    public class AudioLevelEventArgs : EventArgs
    {
        public float MicLevel { get; set; }
        public float SystemLevel { get; set; }
        public bool IsSpeechDetected { get; set; }
    }

    public interface IAudioCaptureService
    {
        bool IsRecording { get; }
        void StartCapture(string outputFilePath);
        void StopCapture();
        event EventHandler<AudioLevelEventArgs>? AudioLevelUpdated;
    }
}
