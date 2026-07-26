using System;
using System.Collections.Generic;
using System.Text;

namespace SafeScribe.Core.SpeechToText
{
    public class SpeakerSegment
    {
        public string SpeakerId { get; set; } = "Speaker 1";
        public string SpeakerName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public TimeSpan Timestamp { get; set; }
    }

    public class SpeakerDiarizer
    {
        private readonly List<SpeakerSegment> _segments = new List<SpeakerSegment>();
        private int _currentSpeakerIndex = 1;

        public void AddSegment(string text, TimeSpan timestamp, string? speakerName = null)
        {
            string speakerId = !string.IsNullOrEmpty(speakerName) ? speakerName : $"Speaker {_currentSpeakerIndex}";

            _segments.Add(new SpeakerSegment
            {
                SpeakerId = speakerId,
                SpeakerName = speakerId,
                Text = text.Trim(),
                Timestamp = timestamp
            });
        }

        public string FormatDiarizedTranscript()
        {
            var sb = new StringBuilder();
            string lastSpeaker = string.Empty;

            foreach (var seg in _segments)
            {
                if (seg.SpeakerId != lastSpeaker)
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.AppendLine($"[{seg.Timestamp:mm\\:ss}] {seg.SpeakerId}:");
                    lastSpeaker = seg.SpeakerId;
                }

                sb.AppendLine($"  {seg.Text}");
            }

            return sb.ToString();
        }

        public void Clear()
        {
            _segments.Clear();
            _currentSpeakerIndex = 1;
        }
    }
}
