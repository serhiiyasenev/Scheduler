using Microsoft.EntityFrameworkCore;
using Scheduler.DAL.Entities;
using Scheduler.DAL.Repositories.Interfaces;

namespace Scheduler.DAL.Repositories;

public class MeetingRepository(SchedulerDbContext context) : BaseRepository<Meeting>(context), IMeetingRepository
{
    public async Task<List<Meeting>> GetAllAsync()
        => await Set.AsNoTracking()
               .Include(m => m.MeetingParticipants)
               .ToListAsync();

    public async Task<Meeting?> GetByIdAsync(int id)
        => await Set.AsNoTracking()
               .Include(m => m.MeetingParticipants)
               .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<Meeting>> GetByUserIdAsync(int userId)
        => await Set.AsNoTracking()
               .Include(m => m.MeetingParticipants)
               .Where(m => m.MeetingParticipants.Any(mp => mp.UserId == userId))
               .OrderBy(m => m.StartTime)
               .ToListAsync();

    public async Task<List<Meeting>> GetMeetingsForParticipantsInRangeAsync(IEnumerable<int> participantIds, DateTime fromInclusive, DateTime toExclusive)
    {
        var ids = participantIds.Distinct().ToList();
        return await Set.AsNoTracking()
            .Include(m => m.MeetingParticipants)
            .Where(m => m.EndTime > fromInclusive && m.StartTime < toExclusive)
            .Where(m => m.MeetingParticipants.Any(mp => ids.Contains(mp.UserId)))
            .OrderBy(m => m.StartTime)
            .ToListAsync();
    }

    public async Task<Meeting> AddWithParticipantsAsync(Meeting meeting, IEnumerable<int> participantIds)
    {
        meeting.MeetingParticipants = participantIds
            .Distinct()
            .Select(pid => new MeetingParticipant { UserId = pid, Meeting = meeting })
            .ToList();

        var entry = await Set.AddAsync(meeting);
        return entry.Entity;
    }
}
