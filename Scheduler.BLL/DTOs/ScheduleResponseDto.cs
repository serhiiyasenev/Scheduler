namespace Scheduler.BLL.DTOs;

public class ScheduleResponseDto
{
    public int MeetingId { get; set; }
    public string Link { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int DurationMinutes { get; set; }
    public List<int> ParticipantIds { get; set; } = new();
}