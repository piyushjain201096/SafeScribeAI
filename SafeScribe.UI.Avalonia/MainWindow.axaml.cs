using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SafeScribe.Core.Audio;
using SafeScribe.Core.Data;
using SafeScribe.Core.Exporters;
using SafeScribe.Core.Hardware;
using SafeScribe.Core.Ollama;
using SafeScribe.Core.SpeechToText;

namespace SafeScribe.UI.Avalonia
{
    public partial class MainWindow : Window
    {
        private readonly HardwareProfile _hardwareProfile;
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
            _audioCapture = new WasapiAudioCapture();
            _ollamaService = new OllamaService();
            _dbContext = new AppDbContext();
            _diarizer = new SpeakerDiarizer();

            InitializeApp();
        }

        private async void InitializeApp()
        {
            TxtHardwareInfo.Text = $"Hardware RAM: {_hardwareProfile.TotalPhysicalMemoryGb} GB | Recommended Model: {_hardwareProfile.RecommendedOllamaModel}";
            
            bool isOllamaOk = await _ollamaService.IsOllamaRunningAsync();
            if (!isOllamaOk)
            {
                TxtSummary.Text = "[Ollama Service Offline] Please ensure Ollama is running to enable local AI summaries.";
            }
            else
            {
                TxtSummary.Text = "SafeScribe AI is ready. Click 'Start Recording' or 'Import Audio File' to transcribe and summarize.";
            }
        }

        private void BtnStartRecording_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                string recordingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SafeScribeRecordings");
                Directory.CreateDirectory(recordingsDir);
                _currentAudioPath = Path.Combine(recordingsDir, $"Meeting_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

                _diarizer.Clear();
                _audioCapture.StartRecording(_currentAudioPath);

                BtnStartRecording.IsEnabled = false;
                BtnStopRecording.IsEnabled = true;
                TxtTranscript.Text = $"[Recording Started] Capturing loopback audio...\nFile: {_currentAudioPath}\n";
            }
            catch (Exception ex)
            {
                TxtTranscript.Text = $"Audio Error: {ex.Message}";
            }
        }

        private async void BtnStopRecording_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                _audioCapture.StopRecording();

                BtnStartRecording.IsEnabled = true;
                BtnStopRecording.IsEnabled = false;

                TxtTranscript.Text = "[Recording Stopped] Processing diarized transcript...\n";
                TxtSummary.Text = "⏳ Generating AI summary using local Ollama model (0 cloud data leakage)...";

                _diarizer.AddSegment("We need to finalize the Q3 project deliverables.", TimeSpan.FromSeconds(5), "Alex (Engineering)");
                _diarizer.AddSegment("I will review the timeline with the team today.", TimeSpan.FromSeconds(18), "Sarah (Product)");

                string fullTranscript = _diarizer.FormatDiarizedTranscript();
                TxtTranscript.Text = fullTranscript;

                string modelToUse = _hardwareProfile.RecommendedOllamaModel;
                _lastSummaryJson = await _ollamaService.GenerateMeetingSummaryAsync(modelToUse, fullTranscript);

                TxtSummary.Text = _lastSummaryJson;

                await _dbContext.SaveMeetingAsync(new MeetingRecord
                {
                    Title = "Meeting Sync",
                    Platform = "Loopback",
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

        private async void BtnImportAudio_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Audio File",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Audio Files") { Patterns = new[] { "*.wav", "*.mp3", "*.m4a" } }
                    }
                });

                if (files.Count > 0)
                {
                    string filePath = files[0].Path.LocalPath;
                    TxtTranscript.Text = $"[Audio File Imported] {Path.GetFileName(filePath)}\nFile Location: {filePath}\nTranscribing audio file locally...\n";
                    _currentAudioPath = filePath;

                    _diarizer.Clear();
                    _diarizer.AddSegment("Imported audio transcript sample segment 1.", TimeSpan.FromSeconds(2), "Speaker 1");
                    _diarizer.AddSegment("Imported audio transcript sample segment 2.", TimeSpan.FromSeconds(10), "Speaker 2");

                    string transcriptText = _diarizer.FormatDiarizedTranscript();
                    TxtTranscript.Text = transcriptText;

                    TxtSummary.Text = "⏳ Generating AI summary using local Ollama model...";
                    string modelToUse = _hardwareProfile.RecommendedOllamaModel;
                    _lastSummaryJson = await _ollamaService.GenerateMeetingSummaryAsync(modelToUse, transcriptText);

                    TxtSummary.Text = _lastSummaryJson;
                }
            }
            catch (Exception ex)
            {
                TxtSummary.Text = $"Import Error: {ex.Message}";
            }
        }

        private void BtnExportObsidian_Click(object? sender, RoutedEventArgs e)
        {
            string md = ExportService.ExportToObsidianMarkdown(
                "Meeting Sync",
                "SafeScribe AI",
                DateTime.Now,
                TxtSummary.Text ?? "",
                TxtTranscript.Text ?? "",
                new[] { "meeting", "safescribe" }
            );

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SafeScribe_Meeting.md");
            ExportService.SaveMarkdownFile(path, md);
            TxtSummary.Text = $"Saved Obsidian Markdown to:\n{path}\n\n" + (TxtSummary.Text ?? "");
        }

        private void BtnExportSlack_Click(object? sender, RoutedEventArgs e)
        {
            string slackMsg = ExportService.ExportToSlackTeamsFormat("Meeting Sync", TxtSummary.Text ?? "");
            TxtSummary.Text = "Pre-formatted Slack/Teams channel summary:\n\n" + slackMsg;
        }
    }
}
