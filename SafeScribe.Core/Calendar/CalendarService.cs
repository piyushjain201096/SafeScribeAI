using System;
using System.IO;
using System.Text;
using System.Web;

namespace SafeScribe.Core.Calendar
{
    public class ActionItem
    {
        public string Task { get; set; } = string.Empty;
        public string Assignee { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
    }

    public static class CalendarService
    {
        public static string GenerateGoogleCalendarUrl(string title, string description, DateTime startTime, DateTime endTime)
        {
            string startIso = startTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");
            string endIso = endTime.ToUniversalTime().ToString("yyyyMMddTHHmmssZ");

            string url = $"https://calendar.google.com/calendar/render?action=TEMPLATE" +
                        $"&text={Uri.EscapeDataString(title)}" +
                        $"&details={Uri.EscapeDataString(description)}" +
                        $"&dates={startIso}/{endIso}";

            return url;
        }

        public static string GenerateOutlookWebUrl(string title, string description, DateTime startTime, DateTime endTime)
        {
            string startIso = startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            string endIso = endTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            string url = $"https://outlook.live.com/calendar/0/deeplink/compose?" +
                        $"subject={Uri.EscapeDataString(title)}" +
                        $"&body={Uri.EscapeDataString(description)}" +
                        $"&startdt={startIso}" +
                        $"&enddt={endIso}";

            return url;
        }

        public static string GenerateIcsFileContent(string title, string description, DateTime startTime, DateTime endTime)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//SafeScribe AI Meeting Assistant//EN");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{Guid.NewGuid()}@safescribe.ai");
            sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTSTART:{startTime.ToUniversalTime():yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTEND:{endTime.ToUniversalTime():yyyyMMddTHHmmssZ}");
            sb.AppendLine($"SUMMARY:{EscapeIcsField(title)}");
            sb.AppendLine($"DESCRIPTION:{EscapeIcsField(description)}");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        public static void SaveIcsFile(string filePath, string title, string description, DateTime startTime, DateTime endTime)
        {
            string content = GenerateIcsFileContent(title, description, startTime, endTime);
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        private static string EscapeIcsField(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            return field.Replace("\\", "\\\\")
                        .Replace(";", "\\;")
                        .Replace(",", "\\,")
                        .Replace("\n", "\\n")
                        .Replace("\r", "");
        }
    }
}
