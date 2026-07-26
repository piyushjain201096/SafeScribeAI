using System;
using System.Collections.Generic;

namespace SafeScribe.Core.Templates
{
    public class PromptTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;
    }

    public static class PromptTemplateManager
    {
        public static List<PromptTemplate> GetDefaultTemplates()
        {
            return new List<PromptTemplate>
            {
                new PromptTemplate
                {
                    Id = "general",
                    Name = "General Meeting",
                    Description = "Standard summary, key takeaways, and action items",
                    SystemPrompt = @"You are SafeScribe AI, a meeting assistant. Summarize the transcript into structured JSON.
Return JSON format:
{
  ""summary"": ""Concise 2-3 paragraph meeting overview..."",
  ""key_takeaways"": [""Key point 1"", ""Key point 2""],
  ""action_items"": [
    { ""task"": ""Task description"", ""assignee"": ""Name or Unassigned"", ""due_date"": ""ISO timestamp or empty"" }
  ]
}"
                },
                new PromptTemplate
                {
                    Id = "standup",
                    Name = "Daily Engineering Standup",
                    Description = "Extracts Yesterday's progress, Today's focus, and Blockers",
                    SystemPrompt = @"You are SafeScribe AI. Summarize this daily standup transcript into structured JSON format:
{
  ""summary"": ""Standup overview..."",
  ""yesterday_progress"": [""Completed task 1"", ""Completed task 2""],
  ""today_focus"": [""Planned task 1"", ""Planned task 2""],
  ""blockers"": [""Blocker description or None""],
  ""action_items"": [
    { ""task"": ""Task description"", ""assignee"": ""Name"", ""due_date"": """" }
  ]
}"
                },
                new PromptTemplate
                {
                    Id = "sales",
                    Name = "Sales / Client Discovery",
                    Description = "Extracts Client Pain Points, Budget Range, and Next Steps",
                    SystemPrompt = @"You are SafeScribe AI. Summarize this sales/discovery transcript into structured JSON format:
{
  ""summary"": ""Sales call summary..."",
  ""client_pain_points"": [""Pain point 1"", ""Pain point 2""],
  ""budget_timeline"": ""Budget or timeline mentioned"",
  ""action_items"": [
    { ""task"": ""Follow up task"", ""assignee"": ""Sales rep"", ""due_date"": """" }
  ]
}"
                },
                new PromptTemplate
                {
                    Id = "one_on_one",
                    Name = "1-on-1 Manager Sync",
                    Description = "Extracts Career Growth Feedback, Wins, and Next Actions",
                    SystemPrompt = @"You are SafeScribe AI. Summarize this 1-on-1 manager sync transcript into structured JSON format:
{
  ""summary"": ""1-on-1 sync overview..."",
  ""wins_highlights"": [""Achievement 1"", ""Highlight 2""],
  ""growth_feedback"": [""Feedback point 1""],
  ""action_items"": [
    { ""task"": ""Agreed action"", ""assignee"": ""Name"", ""due_date"": """" }
  ]
}"
                },
                new PromptTemplate
                {
                    Id = "executive",
                    Name = "Executive / Board Briefing",
                    Description = "High-level strategic decisions, metrics, and financial impacts",
                    SystemPrompt = @"You are SafeScribe AI. Summarize this executive meeting transcript into structured JSON format:
{
  ""summary"": ""High-level executive summary..."",
  ""strategic_decisions"": [""Decision 1"", ""Decision 2""],
  ""key_metrics_discussed"": [""Metric 1"", ""Metric 2""],
  ""action_items"": [
    { ""task"": ""Strategic action"", ""assignee"": ""Executive"", ""due_date"": """" }
  ]
}"
                }
            };
        }
    }
}
