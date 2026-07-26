using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace SafeScribe.Core.Data
{
    public class MeetingRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string AudioPath { get; set; } = string.Empty;
        public string Transcript { get; set; } = string.Empty;
        public string SummaryJson { get; set; } = string.Empty;
    }

    public class AppDbContext
    {
        private readonly string _dbPath;

        public AppDbContext(string? customPath = null)
        {
            if (string.IsNullOrEmpty(customPath))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string scribeFolder = Path.Combine(appData, "SafeScribeAI");
                Directory.CreateDirectory(scribeFolder);
                _dbPath = Path.Combine(scribeFolder, "safescribe.db");
            }
            else
            {
                _dbPath = customPath;
            }

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conn = GetConnection();
            conn.Open();

            string sql = @"
            CREATE TABLE IF NOT EXISTS Meetings (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Platform TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                AudioPath TEXT,
                Transcript TEXT,
                SummaryJson TEXT
            );";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        private SqliteConnection GetConnection() => new SqliteConnection($"Data Source={_dbPath}");

        public async Task SaveMeetingAsync(MeetingRecord record)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string sql = @"
            INSERT OR REPLACE INTO Meetings (Id, Title, Platform, CreatedAt, AudioPath, Transcript, SummaryJson)
            VALUES (@Id, @Title, @Platform, @CreatedAt, @AudioPath, @Transcript, @SummaryJson);";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", record.Id);
            cmd.Parameters.AddWithValue("@Title", record.Title);
            cmd.Parameters.AddWithValue("@Platform", record.Platform);
            cmd.Parameters.AddWithValue("@CreatedAt", record.CreatedAt.ToString("o"));
            cmd.Parameters.AddWithValue("@AudioPath", record.AudioPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Transcript", record.Transcript ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SummaryJson", record.SummaryJson ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<MeetingRecord>> GetAllMeetingsAsync()
        {
            var list = new List<MeetingRecord>();

            using var conn = GetConnection();
            await conn.OpenAsync();

            string sql = "SELECT Id, Title, Platform, CreatedAt, AudioPath, Transcript, SummaryJson FROM Meetings ORDER BY CreatedAt DESC;";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new MeetingRecord
                {
                    Id = reader.GetString(0),
                    Title = reader.GetString(1),
                    Platform = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    AudioPath = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Transcript = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    SummaryJson = reader.IsDBNull(6) ? "" : reader.GetString(6)
                });
            }

            return list;
        }
    }
}
