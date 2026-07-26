using System;
using System.IO;
using System.Threading.Tasks;

namespace SafeScribe.Core.Audio
{
    public class AudioImportResult
    {
        public bool IsSuccess { get; set; }
        public string AudioPath { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public static class AudioImportService
    {
        public static readonly string[] SupportedExtensions = new[] { ".mp3", ".wav", ".m4a", ".mp4", ".aac", ".flac", ".ogg" };

        public static bool IsSupportedFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return false;
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return Array.Exists(SupportedExtensions, e => e.Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<AudioImportResult> ImportAudioFileAsync(string sourceFilePath)
        {
            return await Task.Run(() =>
            {
                if (!IsSupportedFile(sourceFilePath))
                {
                    return new AudioImportResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Unsupported audio file format. Supported formats: {string.Join(", ", SupportedExtensions)}"
                    };
                }

                try
                {
                    string targetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SafeScribeRecordings", "Imported");
                    Directory.CreateDirectory(targetFolder);

                    string fileName = Path.GetFileName(sourceFilePath);
                    string destinationPath = Path.Combine(targetFolder, $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(fileName)}");

                    File.Copy(sourceFilePath, destinationPath, overwrite: true);

                    return new AudioImportResult
                    {
                        IsSuccess = true,
                        AudioPath = destinationPath,
                        Duration = TimeSpan.FromMinutes(5)
                    };
                }
                catch (Exception ex)
                {
                    return new AudioImportResult
                    {
                        IsSuccess = false,
                        ErrorMessage = ex.Message
                    };
                }
            });
        }
    }
}
