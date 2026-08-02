namespace AutoCare_Club_Api.Dtos.Appointments
{
    public class AppointmentDto
    {
       public string Id { get; set; } =
            string.Empty;

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

        public string Status { get; set; } =
            string.Empty;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } 
    }
}