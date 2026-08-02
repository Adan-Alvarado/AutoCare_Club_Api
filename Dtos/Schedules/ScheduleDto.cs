namespace AutoCare_Club_Api.Dtos.Schedules
{
    public class ScheduleDto
    {
        public string Id { get; set; } = string.Empty;

        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public bool IsAvailable { get; set; }
    }
}