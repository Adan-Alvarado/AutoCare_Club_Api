namespace AutoCare_Club_Api.Entities
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        InProgress,
        Completed,
        Cancelled
    }

    public class AppointmentEntity
    {
        public string Id { get; set; } =
            Guid.NewGuid().ToString();

        public string UserId { get; set; } =
            string.Empty;

        public string VehicleId { get; set; } =
            string.Empty;

        public string ServiceId { get; set; } =
            string.Empty;

        public string? TechnicianId { get; set; }

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public AppointmentStatus Status { get; set; } =
            AppointmentStatus.Pending;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;
    }
}