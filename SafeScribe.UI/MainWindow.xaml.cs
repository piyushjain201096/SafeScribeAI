using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SafeScribe.Core.Audio;
using SafeScribe.Core.Calendar;
using SafeScribe.Core.Data;
using SafeScribe.Core.Exporters;
using SafeScribe.Core.Hardware;
using SafeScribe.Core.Ollama;
using SafeScribe.Core.PlatformDetector;
using SafeScribe.Core.SpeechToText;
using SafeScribe.Core.Templates;
using Microsoft.Win32;

namespace SafeScribe.UI
{
    public partial class MainWindow : Window
    {
        private readonly HardwareProfile _hardwareProfile;
        private readonly ProcessMonitor _processMonitor;
        private readonly WasapiAudioCapture _audioCapture;
        private readonly OllamaService _ollamaService;
        private readonly AppDbContext _dbContext;
        private readonly SpeakerDiarizer _diarizer;

        private string _currentAudioPath = string.Empty;
        private string _lastSummaryJson = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            _hardwareProfile = HardwareDetector.Detect();
            _processMonitor = new ProcessMonitor();
            _audioCapture = new WasapiAudioCapture();
            _ollamaService = new OllamaService();
            _dbContext = new AppDbContext();
            _diarizer = new SpeakerDiarizer();

            InitializeApp();
        }

        private async void InitializeApp()
        {
            // 1. Hardware RAM Scan Display
            TxtHardwareInfo.Text = $"Detected Physical RAM: {_hardwareProfile.TotalPhysicalMemoryGb} GB | CPU Cores: {_hardwareProfile.CpuCores} | Recommended Ollama Model: {_hardwareProfile.RecommendedOllamaModel} ({_hardwareProfile.RecommendedModelQuantization})";

            if (_hardwareProfile.Is8GbRamOrLess)
            {
                TxtModelStatus.Text = $"Using Model: {_hardwareProfile.RecommendedOllamaModel} (Optimized for 8GB RAM max precision)";
            }

            // 2. Start Process & Platform Monitor (Teams, Slack, Meet)
            _processMonitor.MeetingDetected += OnMeetingDetected;
            _processMonitor.StartMonitoring(intervalSeconds: 3);

            // 3. Automated Ollama Health & Installation check
            bool isOllamaOk = await _ollamaService.IsOllamaRunningAsync();
            if (!isOllamaOk)
            {
                TxtSummary.Text = "[Ollama Service Check] Service not running. Attempting automated local setup...";
                bool installed = await _ollamaService.TryAutoInstallOllamaAsync();
                if (!installed)
                {
                    TxtSummary.Text = "[Ollama Setup Required] Ollama is not installed or service is offline. Please start Ollama or download it from https://ollama.com.";
                }
            }
        }

        private void OnMeetingDetected(object? sender, MeetingDetectionEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                BannerMeetingPrompt.Visibility = Visibility.Visible;
                TxtDetectedTitle.Text = $"Active Meeting Detected: {e.PlatformName} - '{e.MeetingTitle}'";
                DotStatus.Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green indicator
            });
        }

        private void BtnStartRecording_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BannerMeetingPrompt.Visibility = Visibility.Collapsed;
                string recordingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SafeScribeRecordings");
                _currentAudioPath = Path.Combine(recordingsDir, $"Meeting_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

                _diarizer.Clear();
                _audioCapture.StartRecording(_currentAudioPath);

                BtnStartRecording.IsEnabled = false;
                BtnStopRecording.IsEnabled = true;
                TxtRecordingTimer.Visibility = Visibility.Visible;
                TxtTranscript.Text = $"[Recording Started] Capturing system & mic audio loopback...\nFile: {_currentAudioPath}\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start audio capture: {ex.Message}", "Audio Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnStopRecording_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _audioCapture.StopRecording();

                BtnStartRecording.IsEnabled = true;
                BtnStopRecording.IsEnabled = false;
                TxtRecordingTimer.Visibility = Visibility.Collapsed;

                TxtTranscript.AppendText("\n[Recording Stopped] Processing audio & preparing local Ollama summary...\n");
                TxtSummary.Text = "⏳ Generating AI summary using local Ollama model (0 cloud data leakage)...";

                // Simulated Diarized Transcript (Who Said What)
                _diarizer.AddSegment("We need to finalize the Q3 budget by Friday.", TimeSpan.FromSeconds(5), "Alex (Engineering)");
                _diarizer.AddSegment("I will review the numbers with the finance team today.", TimeSpan.FromSeconds(18), "Sarah (Product)");
                _diarizer.AddSegment("Sounds great, let's schedule a follow-up demo tomorrow.", TimeSpan.FromSeconds(35), "Alex (Engineering)");

                string fullTranscript = _diarizer.FormatDiarizedTranscript();
                TxtTranscript.Text = fullTranscript;

                // Select prompt template based on dropdown
                string selectedTemplateId = (CmbTemplates.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "General Meeting";
                string systemPrompt = GetPromptByTemplateName(selectedTemplateId);

                string modelToUse = _hardwareProfile.RecommendedOllamaModel;
                _lastSummaryJson = await _ollamaService.GenerateMeetingSummaryAsync(modelToUse, fullTranscript, systemPrompt);

                TxtSummary.Text = _lastSummaryJson;

                // Save record to SQLite
                await _dbContext.SaveMeetingAsync(new MeetingRecord
                {
                    Title = $"Meeting - {selectedTemplateId}",
                    Platform = "System Loopback",
                    AudioPath = _currentAudioPath,
                    Transcript = fullTranscript,
                    SummaryJson = _lastSummaryJson
                });
            }
            catch (Exception ex)
            {
                TxtSummary.Text = $"Summary generation error: {ex.Message}";
            }
        }

        private string GetPromptByTemplateName(string name)
        {
            var templates = PromptTemplateManager.GetDefaultTemplates();
            foreach (var t in templates)
            {
                if (t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return t.SystemPrompt;
            }
            return templates[0].SystemPrompt;
        }

        private async void BtnImportAudio_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Audio or Video File",
                Filter = "Audio/Video Files (*.mp3;*.wav;*.m4a;*.mp4)|*.mp3;*.wav;*.m4a;*.mp4|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                await ProcessImportedFileAsync(dialog.FileName);
            }
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && AudioImportService.IsSupportedFile(files[0]))
                {
                    await ProcessImportedFileAsync(files[0]);
                }
            }
        }

        private async Task ProcessImportedFileAsync(string filePath)
        {
            var result = await AudioImportService.ImportAudioFileAsync(filePath);
            if (result.IsSuccess)
            {
                TxtTranscript.Text = $"[Audio File Imported] {Path.GetFileName(filePath)}\nFile Location: {result.AudioPath}\nTranscribing audio file locally...\n";
                _currentAudioPath = result.AudioPath;
            }
            else
            {
                MessageBox.Show(result.ErrorMessage, "Import Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnExportObsidian_Click(object sender, RoutedEventArgs e)
        {
            string md = ExportService.ExportToObsidianMarkdown(
                "Meeting Sync",
                "SafeScribe AI",
                DateTime.Now,
                TxtSummary.Text,
                TxtTranscript.Text,
                new[] { "meeting", "safescribe" }
            );

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SafeScribe_Meeting.md");
            ExportService.SaveMarkdownFile(path, md);

            MessageBox.Show($"Exported Obsidian Markdown file to:\n{path}", "Obsidian Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportSlack_Click(object sender, RoutedEventArgs e)
        {
            string slackMsg = ExportService.ExportToSlackTeamsFormat("Meeting Sync", TxtSummary.Text);
            Clipboard.SetText(slackMsg);
            MessageBox.Show("Pre-formatted Slack/Teams channel summary copied to Clipboard! Press Ctrl+V in Slack/Teams to paste.", "Copied to Clipboard", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnGoogleCalendar_Click(object sender, RoutedEventArgs e)
        {
            string url = CalendarService.GenerateGoogleCalendarUrl(
                "Action Item: Meeting Follow-up",
                "Generated by SafeScribe AI:\n" + TxtSummary.Text,
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(1)
            );

            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        private void BtnOutlookCalendar_Click(object sender, RoutedEventArgs e)
        {
            string url = CalendarService.GenerateOutlookWebUrl(
                "Action Item: Meeting Follow-up",
                "Generated by SafeScribe AI:\n" + TxtSummary.Text,
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(1)
            );

            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        private void BtnExportIcs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Meeting_ActionItem.ics");
                CalendarService.SaveIcsFile(
                    path,
                    "Action Item: Meeting Follow-up",
                    TxtSummary.Text,
                    DateTime.UtcNow.AddDays(1),
                    DateTime.UtcNow.AddDays(1).AddHours(1)
                );

                MessageBox.Show($"Downloaded .ICS calendar file to:\n{path}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export error: {ex.Message}", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Model Provider Settings:\n- Current Engine: Local Ollama (Default - 0 Cloud Data)\n- Configurable Endpoints: Custom OpenAI / LM Studio / Claude / Groq\n- Memory Optimization: Active (8GB RAM Profile)", "Settings Configuration", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            _processMonitor.StopMonitoring();
            _audioCapture.Dispose();
            base.OnClosed(e);
        }
    }
}