using Microsoft.EntityFrameworkCore;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL;
using Scheduler.DAL.Entities;

namespace Scheduler.BLL.Services;

public class MeetingService(SchedulerDbContext context) : IMeetingService
{
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await context.Users.AsNoTracking().AsQueryable().ToListAsync();
    }

    public async Task<User?> CreateUserAsync(string name)
    {
        var normalizedName = name.Trim().ToLower();
        var exists = await context.Users.AnyAsync(u => u.Name == normalizedName);
        if (exists)
            return null;

        var user = new User { Name = name };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<DateTime?> FindEarliestMeetingSlotAsync(ScheduleRequestDto request)
    {
        var meetings = await context.Meetings
            .Where(m => m.MeetingParticipants.Any(mp => request.ParticipantIds.Contains(mp.UserId)))
            .OrderBy(m => m.StartTime)
            .ToListAsync();

        var currentSlot = request.EarliestStart.ToUniversalTime();

        foreach (var meeting in meetings)
        {
            if (currentSlot.AddMinutes(request.DurationMinutes) <= meeting.StartTime)
                return currentSlot;

            currentSlot = meeting.EndTime > currentSlot ? meeting.EndTime : currentSlot;
        }

        return currentSlot.AddMinutes(request.DurationMinutes) <= request.LatestEnd
            ? currentSlot
            : null;
    }

    public async Task<ScheduleResponseDto?> CreateMeetingAsync(ScheduleRequestDto dto)
    {
        var earliestSlot = await FindEarliestMeetingSlotAsync(dto);

        if (earliestSlot is null)
            return null;

        var meeting = new Meeting
        {
            StartTime = earliestSlot.Value,
            EndTime = earliestSlot.Value.AddMinutes(dto.DurationMinutes),
            MeetingParticipants = dto.ParticipantIds
                .Select(id => new MeetingParticipant { UserId = id })
                .ToList()
        };

        context.Meetings.Add(meeting);
        await context.SaveChangesAsync();

        return new ScheduleResponseDto
        {
            MeetingId = meeting.Id,
            Link = BuildMeetingLink(meeting.Id),
            Start = meeting.StartTime,
            End = meeting.EndTime,
            DurationMinutes = dto.DurationMinutes,
            ParticipantIds = dto.ParticipantIds
        };
    }

    public async Task<List<ScheduleResponseDto>> GetAllMeetingsAsync()
    {
        var meetings = await context.Meetings
            .Include(m => m.MeetingParticipants)
            .ToListAsync();

        return meetings.Select(m => new ScheduleResponseDto
        {
            MeetingId = m.Id,
            Link = BuildMeetingLink(m.Id),
            Start = m.StartTime,
            End = m.EndTime,
            DurationMinutes = (int)(m.EndTime - m.StartTime).TotalMinutes,
            ParticipantIds = m.MeetingParticipants.Select(mp => mp.UserId).ToList()
        }).ToList();
    }

    public async Task<ScheduleResponseDto?> GetMeetingByIdAsync(int id)
    {
        var meeting = await context.Meetings
            .Include(m => m.MeetingParticipants)
            .FirstOrDefaultAsync(m => m.Id == id);

        return meeting == null ? null : new ScheduleResponseDto
        {
            MeetingId = meeting.Id,
            Link = BuildMeetingLink(meeting.Id),
            Start = meeting.StartTime,
            End = meeting.EndTime,
            DurationMinutes = (int)(meeting.EndTime - meeting.StartTime).TotalMinutes,
            ParticipantIds = meeting.MeetingParticipants.Select(mp => mp.UserId).ToList()
        };
    }

    public async Task<List<ScheduleResponseDto>> GetMeetingsByUserIdAsync(int userId)
    {
        var meetings = await context.Meetings
            .Include(m => m.MeetingParticipants)
            .Where(m => m.MeetingParticipants.Any(mp => mp.UserId == userId))
            .ToListAsync();

        return meetings.Select(m => new ScheduleResponseDto
        {
            MeetingId = m.Id,
            Link = BuildMeetingLink(m.Id),
            Start = m.StartTime,
            End = m.EndTime,
            DurationMinutes = (int)(m.EndTime - m.StartTime).TotalMinutes,
            ParticipantIds = m.MeetingParticipants.Select(mp => mp.UserId).ToList()
        }).ToList();
    }

    public async Task<List<DateTime>> SuggestAvailableSlotsAsync(ScheduleRequestDto request, int maxSuggestions = 3)
    {
        var meetings = await context.Meetings
            .Where(m => m.MeetingParticipants.Any(mp => request.ParticipantIds.Contains(mp.UserId)))
            .OrderBy(m => m.StartTime)
            .ToListAsync();

        var suggestions = new List<DateTime>();
        var currentSlot = request.EarliestStart;

        while (currentSlot.AddMinutes(request.DurationMinutes) <= request.LatestEnd && suggestions.Count < maxSuggestions)
        {
            var isOverlapping = meetings.Any(m =>
                currentSlot < m.EndTime &&
                currentSlot.AddMinutes(request.DurationMinutes) > m.StartTime);

            if (!isOverlapping)
            {
                suggestions.Add(currentSlot);
            }

            currentSlot = currentSlot.AddMinutes(request.DurationMinutes);
        }

        return suggestions;
    }

    private string BuildMeetingLink(int meetingId)
    {
        return $"https://localhost:7272/api/Meetings/{meetingId}";
    }
}
