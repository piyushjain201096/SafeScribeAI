using System.Collections.Generic;
using System.Threading.Tasks;
using SafeScribe.Core.Data;

namespace SafeScribe.Core.Abstractions
{
    public interface IStorageService
    {
        Task SaveMeetingAsync(MeetingRecord record);
        Task<List<MeetingRecord>> GetAllMeetingsAsync();
        Task DeleteMeetingAsync(string id);
    }
}
