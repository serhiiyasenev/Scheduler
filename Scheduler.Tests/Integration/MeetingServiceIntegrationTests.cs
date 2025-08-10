using Scheduler.BLL.DTOs;
using Scheduler.BLL.Models;
using Scheduler.DAL.Entities;
using Scheduler.Tests.Base;

namespace Scheduler.Tests.Integration;

public class MeetingServiceIntegrationTests : BaseTest
{
    [Fact]
    public async Task GetUserMeetingsAsync_ShouldReturnEmptyList_WhenNoMeetings()
    {
        var user = await UserService.CreateUserAsync(new CreateUserRequest("TestUser"));
        var meetings = await MeetingService.GetMeetingsByUserIdAsync(user!.Id);

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
        var user = await UserService.CreateUserAsync(new CreateUserRequest("UserWithMeeting"));

        var meeting = new Meeting
        {
            StartTime = new DateTime(2025, 6, 20, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 20, 15, 0, 0, DateTimeKind.Utc),
            MeetingParticipants = new List<MeetingParticipant>
            {
                new() { UserId = user!.Id }
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
        var user1 = await UserService.CreateUserAsync(new CreateUserRequest("User1"));
        var user2 = await UserService.CreateUserAsync(new CreateUserRequest("User2"));
        var user3 = await UserService.CreateUserAsync(new CreateUserRequest("User3"));

        var request = new ScheduleRequestDto
        {
            ParticipantIds = [user1!.Id, user2!.Id, user3!.Id],
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
        var user = await UserService.CreateUserAsync(new CreateUserRequest("BusyUser"));

        Context.Meetings.Add(new Meeting
        {
            StartTime = new DateTime(2025, 6, 20, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 20, 10, 0, 0, DateTimeKind.Utc),
            MeetingParticipants = new List<MeetingParticipant>
            {
                new() { UserId = user!.Id }
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
        var user = await UserService.CreateUserAsync(new CreateUserRequest("Busy"));

        Context.Meetings.Add(new Meeting
        {
            StartTime = DateTime.Parse("2025-06-20T09:00:00Z"),
            EndTime = DateTime.Parse("2025-06-20T17:00:00Z"),
            MeetingParticipants = new List<MeetingParticipant>
            {
                new() { UserId = user!.Id }
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

    [Fact]
    public async Task CreateMeetingAsync_ShouldThrow_WhenAnyParticipantDoesNotExist()
    {
        var user = await UserService.CreateUserAsync(new CreateUserRequest("Exists"));
        var request = new ScheduleRequestDto
        {
            ParticipantIds = [user!.Id, 9999],
            DurationMinutes = 30,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T17:00:00Z")
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => MeetingService.CreateMeetingAsync(request));
    }

    [Fact]
    public async Task FindEarliestMeetingSlotAsync_ShouldThrow_WhenAnyParticipantDoesNotExist()
    {
        var request = new ScheduleRequestDto
        {
            ParticipantIds = [12345],
            DurationMinutes = 30,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T17:00:00Z")
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => MeetingService.FindEarliestMeetingSlotAsync(request));
    }

    [Fact]
    public async Task SuggestAvailableSlotsAsync_ShouldThrow_WhenAnyParticipantDoesNotExist()
    {
        var request = new ScheduleRequestDto
        {
            ParticipantIds = [42],
            DurationMinutes = 30,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T12:00:00Z")
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => MeetingService.SuggestAvailableSlotsAsync(request));
    }

    [Fact]
    public async Task GetMeetingsByUserIdAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() => MeetingService.GetMeetingsByUserIdAsync(123));
    }

    [Fact]
    public async Task CreateMeetingAsync_ShouldReturnLink_UsingSettingsBaseUrl()
    {
        var user1 = await UserService.CreateUserAsync(new CreateUserRequest("A"));
        var user2 = await UserService.CreateUserAsync(new CreateUserRequest("B"));

        var req = new ScheduleRequestDto
        {
            ParticipantIds = [user1!.Id, user2!.Id],
            DurationMinutes = 30,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T17:00:00Z")
        };

        var created = await MeetingService.CreateMeetingAsync(req);

        Assert.NotNull(created);
        Assert.Contains("/api/meetings/", created.Link);
        Assert.True(created.MeetingId > 0);
    }

    [Fact]
    public async Task CreateMeetingAsync_ShouldDeduplicateParticipants()
    {
        var user1 = await UserService.CreateUserAsync(new CreateUserRequest("Dup1"));
        var user2 = await UserService.CreateUserAsync(new CreateUserRequest("Dup2"));

        var req = new ScheduleRequestDto
        {
            ParticipantIds = [user1!.Id, user1.Id, user2!.Id, user2.Id],
            DurationMinutes = 30,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T17:00:00Z")
        };

        var created = await MeetingService.CreateMeetingAsync(req);

        Assert.NotNull(created);
        Assert.Equal(2, created.ParticipantIds.Distinct().Count());
    }

    [Fact]
    public async Task SuggestAvailableSlotsAsync_ShouldRespectMaxSuggestions_AndBeOrdered()
    {
        var user = await UserService.CreateUserAsync(new CreateUserRequest("SlotsUser"));

        Context.Meetings.Add(new Meeting
        {
            StartTime = DateTime.Parse("2025-06-20T09:00:00Z"),
            EndTime = DateTime.Parse("2025-06-20T10:00:00Z"),
            MeetingParticipants = new List<MeetingParticipant> { new() { UserId = user!.Id } }
        });

        await Context.SaveChangesAsync();

        var req = new ScheduleRequestDto
        {
            ParticipantIds = [user.Id],
            DurationMinutes = 60,
            EarliestStart = DateTime.Parse("2025-06-20T09:00:00Z"),
            LatestEnd = DateTime.Parse("2025-06-20T12:00:00Z")
        };

        var slots = await MeetingService.SuggestAvailableSlotsAsync(req, maxSuggestions: 2);

        Assert.Equal(2, slots.Count);
        Assert.True(slots[0] < slots[1]);
        Assert.Equal(DateTime.Parse("2025-06-20T10:00:00Z"), slots[0]);
    }
}