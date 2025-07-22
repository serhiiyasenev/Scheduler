using Microsoft.AspNetCore.Mvc;
using Moq;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL.Entities;
using Scheduler.WebApi.Controllers;

namespace Scheduler.Tests;

public class MeetingsControllerTests
{
    private readonly Mock<IMeetingService> _mockService;
    private readonly MeetingsController _controller;

    public MeetingsControllerTests()
    {
        _mockService = new Mock<IMeetingService>();
        _controller = new MeetingsController(_mockService.Object);
    }

    [Fact]
    public async Task CreateUser_ShouldCallServiceAndReturnOk()
    {
        var expectedUser = new User { Id = 1, Name = "Alice" };
        _mockService.Setup(s => s.CreateUserAsync("Alice"))
                    .ReturnsAsync(expectedUser);

        var result = await _controller.CreateUser("Alice");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(expectedUser.Id, actualUser.Id);
    }

    [Fact]
    public async Task ScheduleMeeting_ShouldReturnSlot_WhenAvailable()
    {
        var request = new ScheduleRequestDto
        {
            ParticipantIds = [1, 2],
            DurationMinutes = 30,
            EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
        };

        var expectedSlot = new DateTime(2025, 6, 20, 9, 30, 0, DateTimeKind.Utc);

        var expectedResponse = new ScheduleResponseDto
        {
            Start = expectedSlot,
            End = expectedSlot.AddMinutes(request.DurationMinutes),
            DurationMinutes = request.DurationMinutes,
            ParticipantIds = request.ParticipantIds
        };

        _mockService.Setup(s => s.FindEarliestMeetingSlotAsync(request))
            .ReturnsAsync(expectedSlot);

        _mockService.Setup(s => s.CreateMeetingAsync(request))
                    .ReturnsAsync(expectedResponse);

        var result = await _controller.ScheduleMeeting(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ScheduleResponseDto>(okResult.Value);
        Assert.Equal(expectedSlot, response.Start);
        Assert.Equal(expectedSlot.AddMinutes(request.DurationMinutes), response.End);
        Assert.Equal(request.DurationMinutes, response.DurationMinutes);
        Assert.Equal(request.ParticipantIds, response.ParticipantIds);
    }

    [Fact]
    public async Task FindSlot_ShouldReturnSlot_WhenAvailable()
    {
        var request = new ScheduleRequestDto
        {
            ParticipantIds = [1, 2],
            DurationMinutes = 30,
            EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
        };

        var expectedSlot = new DateTime(2025, 6, 20, 9, 30, 0, DateTimeKind.Utc);

        _mockService.Setup(s => s.FindEarliestMeetingSlotAsync(request))
            .ReturnsAsync(expectedSlot);

        var result = await _controller.FindSlot(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("EarliestSlot", okResult.Value!.ToString());
    }

    [Fact]
    public async Task ScheduleMeeting_ShouldReturnConflict_WhenNoSlot()
    {
        var request = new ScheduleRequestDto
        {
            ParticipantIds = [1, 2],
            DurationMinutes = 30,
            EarliestStart = DateTime.UtcNow,
            LatestEnd = DateTime.UtcNow.AddHours(1)
        };

        _mockService.Setup(s => s.FindEarliestMeetingSlotAsync(request))
                    .ReturnsAsync((DateTime?)null);

        var result = await _controller.ScheduleMeeting(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task GetUserMeetings_ShouldReturnUserMeetings()
    {
        var userId = 1;
        var meetings = new List<ScheduleResponseDto>
        {
            new() { MeetingId = 1, Start = DateTime.UtcNow, End = DateTime.UtcNow.AddMinutes(60) }
        };

        _mockService.Setup(s => s.GetMeetingsByUserIdAsync(userId))
                    .ReturnsAsync(meetings);

        var result = await _controller.GetMeetingsByUser(userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedMeetings = Assert.IsAssignableFrom<List<ScheduleResponseDto>>(okResult.Value);
        Assert.Single(returnedMeetings);
    }

    [Fact]
    public async Task ScheduleMeeting_ShouldReturnConflictOrProposeNewTime_WhenSlotIsBusy()
    {
        var request = new ScheduleRequestDto
        {
            ParticipantIds = [1, 2, 3],
            DurationMinutes = 60,
            EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
        };

        var nextAvailableSlot = new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc);

        _mockService.Setup(s => s.FindEarliestMeetingSlotAsync(request))
            .ReturnsAsync(nextAvailableSlot);

        _mockService.Setup(s => s.CreateMeetingAsync(It.IsAny<ScheduleRequestDto>()))
            .ReturnsAsync(new ScheduleResponseDto
            {
                Start = nextAvailableSlot,
                End = nextAvailableSlot.AddMinutes(request.DurationMinutes),
                DurationMinutes = request.DurationMinutes,
                ParticipantIds = request.ParticipantIds
            });

        var result = await _controller.ScheduleMeeting(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<ScheduleResponseDto>(okResult.Value);
        Assert.Equal(nextAvailableSlot, dto.Start);
        Assert.Equal(request.DurationMinutes, dto.DurationMinutes);
        Assert.Equal(request.ParticipantIds, dto.ParticipantIds);
    }
}
