using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club_Api.Dtos.Appointments
{
    public class AppointmentCreateDto
    {
        [Required(ErrorMessage = "El vehículo es requerido")]
        public string VehicleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El servicio es requerido")]
        public string ServiceId { get; set; } = string.Empty;

        [Display(Name = "Fecha de la cita")]
        public DateOnly AppointmentDate { get; set; }

        [Display(Name = "Hora de inicio")]
        public TimeOnly StartTime { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden superar los 500 caracteres")]
        public string? Notes { get; set; }
    }
}