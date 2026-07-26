using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SafeScribe.Core.Ollama
{
    public class ModelProgressEventArgs : EventArgs
    {
        public string Status { get; set; } = string.Empty;
        public long CompletedBytes { get; set; }
        public long TotalBytes { get; set; }
        public double Percentage => TotalBytes > 0 ? (double)CompletedBytes / TotalBytes * 100.0 : 0.0;
    }

    public class OllamaService
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        private readonly string _baseUrl;

        public event EventHandler<ModelProgressEventArgs>? ModelPullProgress;

        public OllamaService(string baseUrl = "http://localhost:11434")
        {
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<bool> IsOllamaRunningAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/version");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryAutoInstallOllamaAsync()
        {
            if (await IsOllamaRunningAsync()) return true;

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "install Ollama.Ollama --accept-source-agreements --accept-package-agreements",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(psi);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                    }

                    var startPsi = new ProcessStartInfo
                    {
                        FileName = "ollama",
                        Arguments = "serve",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(startPsi);

                    for (int i = 0; i < 10; i++)
                    {
                        await Task.Delay(1000);
                        if (await IsOllamaRunningAsync()) return true;
                    }
                }
            }
            catch
            {
            }

            return await IsOllamaRunningAsync();
        }

        public async Task PullModelAsync(string modelName, CancellationToken cancellationToken = default)
        {
            var payload = new { name = modelName, stream = true };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/pull")
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null && !cancellationToken.IsCancellationRequested)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    string status = root.TryGetProperty("status", out var statusElem) ? statusElem.GetString() ?? "" : "";
                    long completed = root.TryGetProperty("completed", out var compElem) ? compElem.GetInt64() : 0;
                    long total = root.TryGetProperty("total", out var totElem) ? totElem.GetInt64() : 0;

                    ModelPullProgress?.Invoke(this, new ModelProgressEventArgs
                    {
                        Status = status,
                        CompletedBytes = completed,
                        TotalBytes = total
                    });
                }
                catch
                {
                }
            }
        }

        public async Task<string> GenerateMeetingSummaryAsync(string modelName, string transcript, string promptTemplate = "")
        {
            if (string.IsNullOrWhiteSpace(promptTemplate))
            {
                promptTemplate = @"You are SafeScribe AI, an expert AI meeting assistant. Summarize the following meeting transcript into structured JSON.
Return JSON with this exact format:
{
  ""summary"": ""Concise 2-3 paragraph summary of main discussion points..."",
  ""key_takeaways"": [""Takeaway 1"", ""Takeaway 2""],
  ""action_items"": [
    {
      ""task"": ""Task description"",
      ""assignee"": ""Assignee name or Unassigned"",
      ""due_date"": ""ISO timestamp or empty string""
    }
  ]
}";
            }

            var messages = new[]
            {
                new { role = "system", content = promptTemplate },
                new { role = "user", content = $"Meeting Transcript:\n{transcript}" }
            };

            var payload = new
            {
                model = modelName,
                messages = messages,
                stream = false,
                format = "json"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var text))
            {
                return text.GetString() ?? "";
            }

            return responseJson;
        }
    }
}
