using Microsoft.AspNetCore.Mvc;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;

namespace Scheduler.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController(IMeetingService meetingService) : ControllerBase
    {

        [HttpGet("GetAllMeetings")]
        [HttpGet]
        [ProducesResponseType(typeof(List<ScheduleResponseDto>), 200)]
        public async Task<IActionResult> GetAllMeetings()
        {
            var result = await meetingService.GetAllMeetingsAsync();
            return Ok(result);
        }

        [HttpGet("GetMeetingById/{id}")]
        public async Task<IActionResult> GetMeetingById(int id)
        {
            var meeting = await meetingService.GetMeetingByIdAsync(id);
            return meeting == null ? NotFound() : Ok(meeting);
        }

        [HttpPost("FindSlot")]
        public async Task<IActionResult> FindSlot([FromBody] ScheduleRequestDto request)
        {
            var slot = await meetingService.FindEarliestMeetingSlotAsync(request);
            return slot == null ? Conflict("No available slot.") : Ok(new { EarliestSlot = slot });
        }

        [HttpGet("GetMeetingsByUserId/{userId}")]
        public async Task<IActionResult> GetMeetingsByUser(int userId)
        {
            var meetings = await meetingService.GetMeetingsByUserIdAsync(userId);
            return Ok(meetings);
        }

        [HttpPost("CreateOrFindEarliestMeetingSlotWithSuggestions")]
        [HttpPost]
        [ProducesResponseType(typeof(ScheduleResponseDto), 200)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> ScheduleMeeting([FromBody] ScheduleRequestDto request)
        {
            var slot = await meetingService.FindEarliestMeetingSlotAsync(request);

            if (slot is not null)
            {
                var created = await meetingService.CreateMeetingAsync(request);
                return Ok(created);
            }

            var suggestions = await meetingService.SuggestAvailableSlotsAsync(request);
            return suggestions?.Any() == true
                ? Conflict(new { Message = "No available slot in preferred time range.", Suggestions = suggestions })
                : Conflict("No available time slot.");
        }
    }
}