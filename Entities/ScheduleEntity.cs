namespace AutoCare_Club_Api.Entities
{
    public class ScheduleEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}