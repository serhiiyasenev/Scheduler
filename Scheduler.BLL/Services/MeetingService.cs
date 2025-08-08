using Microsoft.Extensions.Options;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL.Entities;
using Scheduler.DAL.Repositories.Interfaces;

namespace Scheduler.BLL.Services;

public class MeetingService(IMeetingRepository meetingRepository, IOptions<MeetingSettings> settings) : IMeetingService
{
    private readonly MeetingSettings _settings = settings.Value;

    public async Task<List<ScheduleResponseDto>> GetAllMeetingsAsync()
    {
        var meetings = await meetingRepository.GetAllAsync();
        return meetings.Select(Map).ToList();
    }

    public async Task<DateTime?> FindEarliestMeetingSlotAsync(ScheduleRequestDto request)
    {
        var from = request.EarliestStart;
        var to = request.LatestEnd;
        var duration = TimeSpan.FromMinutes(request.DurationMinutes);

        var busy = await meetingRepository.GetMeetingsForParticipantsInRangeAsync(request.ParticipantIds, from, to);
        
        // Merge busy intervals
        var intervals = busy.Select(m => (m.StartTime, m.EndTime))
                            .OrderBy(x => x.StartTime)
                            .ToList();

        DateTime cursor = from;
        foreach (var (s, e) in intervals)
        {
            if (cursor + duration <= s)
                return cursor;

            if (e > cursor)
                cursor = e;

            if (cursor > to - duration)
                break;
        }

        if (cursor + duration <= to)
            return cursor;

        return null;
    }

    public async Task<List<ScheduleResponseDto>> GetMeetingsByUserIdAsync(int userId)
    {
        var meetings = await meetingRepository.GetByUserIdAsync(userId);
        return meetings.Select(Map).ToList();
    }

    public async Task<ScheduleResponseDto?> CreateMeetingAsync(ScheduleRequestDto dto)
    {
        var start = await FindEarliestMeetingSlotAsync(dto);
        if (start is null) return null;

        var meeting = new Meeting
        {
            StartTime = start.Value,
            EndTime = start.Value.AddMinutes(dto.DurationMinutes)
        };

        var created = await meetingRepository.AddWithParticipantsAsync(meeting, dto.ParticipantIds);
        return Map(created);
    }

    public async Task<ScheduleResponseDto?> GetMeetingByIdAsync(int id)
    {
        var meeting = await meetingRepository.GetByIdAsync(id);
        return meeting is null ? null : Map(meeting);
    }

    public async Task<List<DateTime>> SuggestAvailableSlotsAsync(ScheduleRequestDto request, int maxSuggestions = 3)
    {
        var from = request.EarliestStart;
        var to = request.LatestEnd;
        var duration = TimeSpan.FromMinutes(request.DurationMinutes);
        var suggestions = new List<DateTime>();

        var busy = await meetingRepository.GetMeetingsForParticipantsInRangeAsync(request.ParticipantIds, from, to);
        var intervals = busy.Select(m => (m.StartTime, m.EndTime))
                            .OrderBy(x => x.StartTime)
                            .ToList();

        DateTime cursor = from;
        while (cursor + duration <= to && suggestions.Count < maxSuggestions)
        {
            // find next busy interval that starts after cursor
            var nextBusy = intervals.FirstOrDefault(iv => iv.Item1 < cursor + duration && iv.Item2 > cursor);
            if (nextBusy != default)
            {
                cursor = nextBusy.Item2 > cursor ? nextBusy.Item2 : cursor;
                cursor = cursor.AddMinutes(0); // no-op; clarity
            }
            else
            {
                suggestions.Add(cursor);
                cursor = cursor.AddMinutes(request.DurationMinutes);
            }
        }

        return suggestions;
    }

    private ScheduleResponseDto Map(Meeting meeting)
    {
        return new ScheduleResponseDto
        {
            MeetingId = meeting.Id,
            Link = BuildMeetingLink(meeting.Id),
            Start = meeting.StartTime,
            End = meeting.EndTime,
            DurationMinutes = (int)(meeting.EndTime - meeting.StartTime).TotalMinutes,
            ParticipantIds = meeting.MeetingParticipants?.Select(mp => mp.UserId).ToList() ?? []
        };
    }

    private string BuildMeetingLink(int meetingId) => $"{_settings.BaseUrl}/api/Meetings/{meetingId}";
}
