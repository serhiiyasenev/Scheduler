using Microsoft.AspNetCore.Mvc;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL.Entities;

namespace Scheduler.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController(IMeetingService meetingService) : ControllerBase
    {
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await meetingService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllMeetings()
        {
            var result = await meetingService.GetAllMeetingsAsync();
            return Ok(result);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] string name)
        {
            var user = await meetingService.CreateUserAsync(name);
            return user == null ? Conflict("User with the same name already exists.") : Ok(user);
        }

        [HttpGet("meetings/{id}")]
        public async Task<IActionResult> GetMeetingById(int id)
        {
            var meeting = await meetingService.GetMeetingByIdAsync(id);
            return meeting == null ? NotFound() : Ok(meeting);
        }

        [HttpPost("meetings/slot")]
        public async Task<IActionResult> FindSlot([FromBody] ScheduleRequestDto request)
        {
            var slot = await meetingService.FindEarliestMeetingSlotAsync(request);
            return slot == null ? Conflict("No available slot.") : Ok(new { EarliestSlot = slot });
        }

        [HttpGet("users/{userId}/meetings")]
        public async Task<IActionResult> GetMeetingsByUser(int userId)
        {
            var meetings = await meetingService.GetMeetingsByUserIdAsync(userId);
            return Ok(meetings);
        }

        [HttpPost("meetings")]
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