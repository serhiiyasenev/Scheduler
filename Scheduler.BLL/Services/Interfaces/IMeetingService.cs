using Scheduler.BLL.DTOs;

namespace Scheduler.BLL.Services.Interfaces;

public interface IMeetingService
{
    Task<List<ScheduleResponseDto>> GetAllMeetingsAsync();
    Task<DateTime?> FindEarliestMeetingSlotAsync(ScheduleRequestDto request);
    Task<List<ScheduleResponseDto>> GetMeetingsByUserIdAsync(int userId);
    Task<ScheduleResponseDto?> CreateMeetingAsync(ScheduleRequestDto dto);
    Task<ScheduleResponseDto?> GetMeetingByIdAsync(int id);
    Task<List<DateTime>> SuggestAvailableSlotsAsync(ScheduleRequestDto request, int maxSuggestions = 3);
}