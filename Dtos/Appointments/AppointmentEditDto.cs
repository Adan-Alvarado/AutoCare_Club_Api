using AutoCare_Club_Api.Entities;

namespace AutoCare_Club_Api.Dtos.Appointments
{
    public class AppointmentEditDto : AppointmentCreateDto
    {
        public string? TechnicianId { get; set; }

        public AppointmentStatus Status { get; set; }
    }
}