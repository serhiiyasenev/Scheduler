namespace Scheduler.BLL.DTOs;

public class ScheduleRequestDto
{
    public required List<int> ParticipantIds { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime EarliestStart { get; set; }
    public DateTime LatestEnd { get; set; }
}
