using System;
using System.IO;
using System.Text;

namespace SafeScribe.Core.Exporters
{
    public static class ExportService
    {
        public static string ExportToObsidianMarkdown(string title, string platform, DateTime date, string summaryText, string transcriptText, string[] tags)
        {
            var sb = new StringBuilder();
            sb.AppendLine("---");
            sb.AppendLine($"title: \"{title}\"");
            sb.AppendLine($"date: {date:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"platform: \"{platform}\"");
            sb.AppendLine($"tags: [{string.Join(", ", tags)}]");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine($"# 🎙️ {title}");
            sb.AppendLine();
            sb.AppendLine("## 📋 AI Summary");
            sb.AppendLine(summaryText);
            sb.AppendLine();
            sb.AppendLine("## 💬 Full Transcript");
            sb.AppendLine(transcriptText);

            return sb.ToString();
        }

        public static string ExportToSlackTeamsFormat(string title, string summaryText)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"*:🎙️ Meeting Summary: {title}*");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine(summaryText.Replace("### ", "*").Replace("## ", "*"));
            sb.AppendLine();
            sb.AppendLine("_Generated locally by SafeScribe AI_");

            return sb.ToString();
        }

        public static string ExportToNotionMarkdown(string title, string summaryText)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {title}");
            sb.AppendLine("> **SafeScribe AI Meeting Summary**");
            sb.AppendLine();
            sb.AppendLine(summaryText);

            return sb.ToString();
        }

        public static void SaveMarkdownFile(string destinationPath, string content)
        {
            string? dir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(destinationPath, content, Encoding.UTF8);
        }
    }
}
