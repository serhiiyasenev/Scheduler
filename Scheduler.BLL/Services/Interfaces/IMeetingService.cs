using Scheduler.BLL.DTOs;
using Scheduler.DAL.Entities;

namespace Scheduler.BLL.Services.Interfaces;

public interface IMeetingService
{
    Task<List<User>> GetAllUsersAsync();
    Task<List<ScheduleResponseDto>> GetAllMeetingsAsync();
    Task<User?> CreateUserAsync(string name);
    Task<DateTime?> FindEarliestMeetingSlotAsync(ScheduleRequestDto request);
    Task<List<ScheduleResponseDto>> GetMeetingsByUserIdAsync(int userId);
    Task<ScheduleResponseDto?> CreateMeetingAsync(ScheduleRequestDto dto);
    Task<ScheduleResponseDto?> GetMeetingByIdAsync(int id);
    Task<List<DateTime>> SuggestAvailableSlotsAsync(ScheduleRequestDto request, int maxSuggestions = 3);
}