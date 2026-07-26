using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SafeScribe.Core.PlatformDetector
{
    public class MeetingDetectionEventArgs : EventArgs
    {
        public string PlatformName { get; set; } = string.Empty;
        public string MeetingTitle { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    }

    public class ProcessMonitor
    {
        public event EventHandler<MeetingDetectionEventArgs>? MeetingDetected;

        private CancellationTokenSource? _cts;
        private bool _isMeetingActive = false;
        private string _lastMeetingKey = string.Empty;

        public void StartMonitoring(int intervalSeconds = 3)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
                while (!token.IsCancellationRequested && await timer.WaitForNextTickAsync(token))
                {
                    CheckForActiveMeetings();
                }
            }, token);
        }

        public void StopMonitoring()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void CheckForActiveMeetings()
        {
            try
            {
                var processes = Process.GetProcesses();

                foreach (var proc in processes)
                {
                    string procName = proc.ProcessName.ToLowerInvariant();
                    string windowTitle = string.Empty;

                    try
                    {
                        windowTitle = proc.MainWindowTitle;
                    }
                    catch
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(windowTitle)) continue;

                    if (procName.Contains("teams") || procName.Contains("ms-teams"))
                    {
                        if (windowTitle.Contains("Teams", StringComparison.OrdinalIgnoreCase) ||
                            windowTitle.Contains("Meeting", StringComparison.OrdinalIgnoreCase) ||
                            windowTitle.Contains("Call", StringComparison.OrdinalIgnoreCase))
                        {
                            TriggerMeetingEvent("Microsoft Teams", windowTitle);
                            return;
                        }
                    }

                    if (procName.Contains("slack"))
                    {
                        if (windowTitle.Contains("Huddle", StringComparison.OrdinalIgnoreCase) ||
                            windowTitle.Contains("Slack Call", StringComparison.OrdinalIgnoreCase))
                        {
                            TriggerMeetingEvent("Slack", windowTitle);
                            return;
                        }
                    }

                    if (procName.Contains("chrome") || procName.Contains("msedge") || procName.Contains("firefox") || procName.Contains("brave"))
                    {
                        if (windowTitle.Contains("- Google Meet", StringComparison.OrdinalIgnoreCase) ||
                            windowTitle.Contains("meet.google.com", StringComparison.OrdinalIgnoreCase))
                        {
                            TriggerMeetingEvent("Google Meet", windowTitle);
                            return;
                        }
                    }
                }

                if (_isMeetingActive)
                {
                    _isMeetingActive = false;
                    _lastMeetingKey = string.Empty;
                }
            }
            catch
            {
            }
        }

        private void TriggerMeetingEvent(string platform, string title)
        {
            string meetingKey = $"{platform}:{title}";

            if (!_isMeetingActive || _lastMeetingKey != meetingKey)
            {
                _isMeetingActive = true;
                _lastMeetingKey = meetingKey;

                MeetingDetected?.Invoke(this, new MeetingDetectionEventArgs
                {
                    PlatformName = platform,
                    MeetingTitle = title
                });
            }
        }
    }
}
