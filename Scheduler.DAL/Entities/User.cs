namespace Scheduler.DAL.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<MeetingParticipant> MeetingParticipants { get; set; }
}
