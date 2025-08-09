using Microsoft.Extensions.Options;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Models;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL.Entities;
using Scheduler.DAL.Repositories.Interfaces;

namespace Scheduler.BLL.Services;

public class MeetingService(IMeetingRepository meetingRepository, IUserRepository userRepository, IOptions<MeetingSettings> settings) : IMeetingService
{
    private readonly MeetingSettings _settings = settings.Value;

    public async Task<List<ScheduleResponseDto>> GetAllMeetingsAsync()
    {
        var meetings = await meetingRepository.GetAllAsync();
        return meetings.Select(Map).ToList();
    }

    public async Task<DateTime?> FindEarliestMeetingSlotAsync(ScheduleRequestDto request)
    {
        await EnsureParticipantsExistAsync(request.ParticipantIds);

        var from = request.EarliestStart;
        var to = request.LatestEnd;
        var duration = TimeSpan.FromMinutes(request.DurationMinutes);

        var intervals = await GetBusyIntervals(request.ParticipantIds, from, to);

        var cursor = from;
        foreach (var (s, e) in intervals)
        {
            if (cursor + duration <= s)
                return cursor;

            if (e > cursor)
                cursor = e;

            if (cursor > to - duration)
                break;
        }

        return cursor + duration <= to ? cursor : null;
    }

    public async Task<List<ScheduleResponseDto>> GetMeetingsByUserIdAsync(int userId)
    {
        var existing = await userRepository.GetExistingUserIdsAsync([userId]);
        if (existing.Count == 0)
            throw new EntityNotFoundException($"User not found: {userId}");

        var meetings = await meetingRepository.GetByUserIdAsync(userId);
        return meetings.Select(Map).ToList();
    }

    public async Task<ScheduleResponseDto?> CreateMeetingAsync(ScheduleRequestDto dto)
    {
        await EnsureParticipantsExistAsync(dto.ParticipantIds);

        var start = await FindEarliestMeetingSlotAsync(dto);
        if (start is null) return null;

        var meeting = new Meeting
        {
            StartTime = start.Value,
            EndTime = start.Value.AddMinutes(dto.DurationMinutes)
        };

        var created = await meetingRepository.AddWithParticipantsAsync(meeting, dto.ParticipantIds);
        await meetingRepository.SaveChangesAsync();
        return Map(created);
    }

    public async Task<ScheduleResponseDto?> GetMeetingByIdAsync(int id)
    {
        var meeting = await meetingRepository.GetByIdAsync(id);
        return meeting is null ? null : Map(meeting);
    }

    public async Task<List<DateTime>> SuggestAvailableSlotsAsync(ScheduleRequestDto request, int maxSuggestions = 3)
    {
        await EnsureParticipantsExistAsync(request.ParticipantIds);

        var from = request.EarliestStart;
        var to = request.LatestEnd;
        var duration = TimeSpan.FromMinutes(request.DurationMinutes);
        var suggestions = new List<DateTime>();

        var intervals = await GetBusyIntervals(request.ParticipantIds, from, to);

        var cursor = from;
        while (cursor + duration <= to && suggestions.Count < maxSuggestions)
        {
            var nextBusy = intervals.FirstOrDefault(iv => iv.Start < cursor + duration && iv.End > cursor);
            if (nextBusy != default)
            {
                cursor = nextBusy.End > cursor ? nextBusy.End : cursor;
            }
            else
            {
                suggestions.Add(cursor);
                cursor = cursor.AddMinutes(request.DurationMinutes);
            }
        }

        return suggestions;
    }

    private async Task<List<(DateTime Start, DateTime End)>> GetBusyIntervals(IEnumerable<int> participantIds, DateTime from, DateTime to)
    {
        var busyMeetings = await meetingRepository.GetMeetingsForParticipantsInRangeAsync(participantIds, from, to);
        return busyMeetings
            .Select(m => (m.StartTime, m.EndTime))
            .OrderBy(x => x.StartTime)
            .ToList();
    }

    private async Task EnsureParticipantsExistAsync(List<int> participantIds)
    {
        var existing = await userRepository.GetExistingUserIdsAsync(participantIds);
        var missing = participantIds.Except(existing).ToList();
        if (missing.Count > 0)
            throw new EntityNotFoundException($"Users not found: {string.Join(", ", missing)}");
    }

    private ScheduleResponseDto Map(Meeting meeting) => new()
    {
        MeetingId = meeting.Id,
        Link = BuildMeetingLink(meeting.Id),
        Start = meeting.StartTime,
        End = meeting.EndTime,
        DurationMinutes = (int)(meeting.EndTime - meeting.StartTime).TotalMinutes,
        ParticipantIds = meeting.MeetingParticipants?.Select(mp => mp.UserId).ToList() ?? []
    };

    private string BuildMeetingLink(int meetingId) => $"{_settings.BaseUrl}/api/Meetings/{meetingId}";
}
