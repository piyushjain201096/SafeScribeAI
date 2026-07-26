using System.Threading.Tasks;

namespace SafeScribe.Core.Abstractions
{
    public interface ILlmService
    {
        Task<string> GenerateMeetingSummaryAsync(string modelName, string transcript, string promptTemplate = "");
        Task<bool> IsEngineAvailableAsync();
    }
}
