namespace Scheduler.DAL.Entities;

public class MeetingParticipant
{
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}