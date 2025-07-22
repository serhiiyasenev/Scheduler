using Scheduler.BLL.DTOs;
using Scheduler.DAL.Entities;
using Scheduler.Tests.Base;

namespace Scheduler.Tests.Integration;

public class MeetingServiceIntegrationTests : BaseTest
{
    [Fact]
    public async Task GetUserMeetingsAsync_ShouldReturnEmptyList_WhenNoMeetings()
    {
        var user = await UserService.CreateUserAsync("TestUser");
        var meetings = await MeetingService.GetMeetingsByUserIdAsync(user.Id);

        Assert.NotNull(meetings);
        Assert.Empty(meetings);
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

        var slot = await MeetingService.FindEarliestMeetingSlotAsync(request);

        Assert.Equal(request.EarliestStart, slot);
    }

    [Fact]
    public async Task GetUserMeetingsAsync_ShouldReturnScheduledMeetings()
    {
        var user = await UserService.CreateUserAsync("UserWithMeeting");

        var meeting = new Meeting
        {
            StartTime = new DateTime(2025, 6, 20, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 20, 15, 0, 0, DateTimeKind.Utc),
            MeetingParticipants = new List<MeetingParticipant>
            {
                new() { UserId = user.Id }
            }
        };
        Context.Meetings.Add(meeting);
        await Context.SaveChangesAsync();

        var meetings = await MeetingService.GetMeetingsByUserIdAsync(user.Id);

        Assert.Single(meetings);
        Assert.Equal(meeting.StartTime, meetings[0].Start);
    }

    [Fact]
    public async Task CreateUserAndScheduleMeeting_ShouldReturnValidSlot()
    {
        var user1 = await UserService.CreateUserAsync("User1");
        var user2 = await UserService.CreateUserAsync("User2");
        var user3 = await UserService.CreateUserAsync("User3");

        var request = new ScheduleRequestDto
        {
            ParticipantIds = [user1.Id, user2.Id, user3.Id],
            DurationMinutes = 60,
            EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
        };

        var slot = await MeetingService.FindEarliestMeetingSlotAsync(request);

        Assert.NotNull(slot);
        Assert.Equal(request.EarliestStart, slot);
    }

    [Fact]
    public async Task FindEarliestMeetingSlotAsync_ShouldReturnNextAvailableSlot_WhenOverlapping()
    {
        var user = await UserService.CreateUserAsync("BusyUser");

        Context.Meetings.Add(new Meeting
        {
            StartTime = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc),
            MeetingParticipants = new List<MeetingParticipant>
            {
                new() { UserId = user.Id }
            }
        });
        await Context.SaveChangesAsync();

        var request = new ScheduleRequestDto
        {
            ParticipantIds = [user.Id],
            DurationMinutes = 60,
            EarliestStart = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            LatestEnd = new DateTime(2025, 6, 20, 17, 0, 0, DateTimeKind.Utc)
        };

        var slot = await MeetingService.FindEarliestMeetingSlotAsync(request);

        Assert.Equal(new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc), slot);
    }

    [Fact]
    public async Task SuggestAvailableSlotsAsync_ShouldReturnEmpty_WhenNoAvailableSlot()
    {
        var user = await UserService.CreateUserAsync("Busy");

        Context.Meetings.Add(new Meeting
        {
            StartTime = DateTime.Parse("2025-06-20T09:00:00Z"),
            EndTime = DateTime.Parse("2025-06-20T17:00:00Z"),
            MeetingParticipants = new List<MeetingParticipant>
            {
                new() { UserId = user.Id }
            }
        });
        await Context.SaveChangesAsync();

        var request = new ScheduleRequestDto
        {
            ParticipantIds = [user.Id],
            DurationMinutes = 60,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T17:00:00Z")
        };

        var slots = await MeetingService.SuggestAvailableSlotsAsync(request);

        Assert.Empty(slots);
    }
}