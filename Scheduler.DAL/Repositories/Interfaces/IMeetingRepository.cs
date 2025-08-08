using Scheduler.DAL.Entities;

namespace Scheduler.DAL.Repositories.Interfaces;

public interface IMeetingRepository : IRepository<Meeting>
{
    Task<List<Meeting>> GetAllAsync();
    Task<Meeting?> GetByIdAsync(int id);
    Task<List<Meeting>> GetByUserIdAsync(int userId);
    Task<List<Meeting>> GetMeetingsForParticipantsInRangeAsync(IEnumerable<int> participantIds, DateTime fromInclusive, DateTime toExclusive);
    Task<Meeting> AddWithParticipantsAsync(Meeting meeting, IEnumerable<int> participantIds);
}
