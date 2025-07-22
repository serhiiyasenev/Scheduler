using Microsoft.EntityFrameworkCore;
using Scheduler.BLL.DTOs;
using Scheduler.BLL.Services;
using Scheduler.BLL.Services.Interfaces;
using Scheduler.DAL;
using Scheduler.DAL.Entities;

namespace Scheduler.Tests;

public class MeetingServiceIntegrationTests : IDisposable
{
    private readonly SchedulerDbContext _context;
    private readonly IMeetingService _service;

    public MeetingServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new SchedulerDbContext(options);
        _service = new MeetingService(_context);
    }

    [Fact]
    public async Task CreateUserAndScheduleMeeting_ShouldReturnValidSlot()
    {
        var user1 = await _service.CreateUserAsync("User1");
        var user2 = await _service.CreateUserAsync("User2");
        var user3 = await _service.CreateUserAsync("User3");

        var request = new ScheduleRequestDto
        {
            ParticipantIds = [user1.Id, user2.Id, user3.Id],
            DurationMinutes = 60,
            EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
        };

        var slot = await _service.FindEarliestMeetingSlotAsync(request);

        Assert.NotNull(slot);
        Assert.Equal(request.EarliestStart, slot);
    }

    [Fact]
    public async Task FindEarliestMeetingSlotAsync_ShouldReturnNextAvailableSlot_WhenOverlapping()
    {
        var user = await _service.CreateUserAsync("BusyUser");

        _context.Meetings.Add(new Meeting
        {
            StartTime = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc),
            MeetingParticipants = new List<MeetingParticipant>
            {
                new() { UserId = user.Id }
            }
        });
        await _context.SaveChangesAsync();

        var request = new ScheduleRequestDto
        {
            ParticipantIds = [user.Id],
            DurationMinutes = 60,
            EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
        };

        var slot = await _service.FindEarliestMeetingSlotAsync(request);

        Assert.Equal(new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc), slot);
    }

    [Fact]
    public async Task GetUserMeetingsAsync_ShouldReturnScheduledMeetings()
    {
        var user = await _service.CreateUserAsync("UserWithMeeting");

        var meeting = new Meeting
        {
            StartTime = new DateTime(2025, 6, 20, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 20, 15, 0, 0, DateTimeKind.Utc),
            MeetingParticipants = new List<MeetingParticipant>
            {
                new() { UserId = user.Id }
            }
        };
        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();

        var meetings = await _service.GetMeetingsByUserIdAsync(user.Id);

        Assert.Single(meetings);
        Assert.Equal(meeting.StartTime, meetings[0].Start);
    }

    public void Dispose() => _context.Dispose();
}
