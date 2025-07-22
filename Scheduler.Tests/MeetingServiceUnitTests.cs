using Microsoft.EntityFrameworkCore;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL;
using Scheduler.DAL.Entities;

namespace Scheduler.Tests;

public class MeetingServiceUnitTests
{
    private readonly IMeetingService _service;

    public MeetingServiceUnitTests()
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseInMemoryDatabase("UnitTestDb")
            .Options;

        var context = new SchedulerDbContext(options);
        _service = new MeetingService(context);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateAndReturnUser()
    {
        var user = await _service.CreateUserAsync("Alice");
        Assert.NotNull(user);
        Assert.Equal("Alice", user.Name);
        Assert.True(user.Id > 0);
    }

    [Fact]
    public async Task FindEarliestMeetingSlotAsync_ShouldReturnStartTime_WhenNoConflicts()
    {
        var request = new ScheduleRequestDto
        {
            ParticipantIds = [],
            DurationMinutes = 60,
            EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
        };

        var slot = await _service.FindEarliestMeetingSlotAsync(request);

        Assert.Equal(request.EarliestStart, slot);
    }

    [Fact]
    public async Task GetUserMeetingsAsync_ShouldReturnEmptyList_WhenNoMeetings()
    {
        var user = await _service.CreateUserAsync("TestUser");
        var meetings = await _service.GetMeetingsByUserIdAsync(user.Id);

        Assert.NotNull(meetings);
        Assert.Empty(meetings);
    }
}