namespace Scheduler.DAL.Entities
{
    public class Meeting
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ICollection<MeetingParticipant> MeetingParticipants { get; set; }
    }
}
