namespace AutoCare_Club_Api.Dtos.Schedules
{
    public class ScheduleAvailabilityDto
    {
        public string ServiceId { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}