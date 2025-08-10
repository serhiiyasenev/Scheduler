using Microsoft.AspNetCore.Mvc;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;

namespace Scheduler.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeetingsController(IMeetingService meetingService) : ControllerBase
{
    [HttpGet("GetAllMeetings")]
    [ProducesResponseType(typeof(List<ScheduleResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllMeetings()
    {
        var result = await meetingService.GetAllMeetingsAsync();
        return Ok(result);
    }

    [HttpGet("GetMeetingById/{id}")]
    [ProducesResponseType(typeof(ScheduleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMeetingById(int id)
    {
        var meeting = await meetingService.GetMeetingByIdAsync(id);
        return meeting == null ? NotFound() : Ok(meeting);
    }

    [HttpPost("FindSlot")]
    [ProducesResponseType(typeof(FindSlotResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> FindSlot([FromBody] ScheduleRequestDto request)
    {
        var slot = await meetingService.FindEarliestMeetingSlotAsync(request);
        return slot == null ? Conflict("No available slot.") : Ok(new FindSlotResponseDto(slot.Value));
    }

    [HttpGet("GetMeetingsByUserId/{userId}")]
    [ProducesResponseType(typeof(List<ScheduleResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMeetingsByUser(int userId)
    {
        var meetings = await meetingService.GetMeetingsByUserIdAsync(userId);
        return Ok(meetings);
    }

    [HttpPost("CreateOrFindEarliestMeetingSlotWithSuggestions")]
    [ProducesResponseType(typeof(ScheduleResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ScheduleMeeting([FromBody] ScheduleRequestDto request)
    {
        var slot = await meetingService.FindEarliestMeetingSlotAsync(request);

        if (slot is not null)
        {
            var created = await meetingService.CreateMeetingAsync(request);
            return CreatedAtAction(nameof(GetMeetingById), new { id = created!.MeetingId }, created);
        }

        var suggestions = await meetingService.SuggestAvailableSlotsAsync(request);
        return suggestions?.Any() == true
            ? Conflict(new { Message = "No available slot in preferred time range.", Suggestions = suggestions })
            : Conflict("No available time slot.");
    }
}