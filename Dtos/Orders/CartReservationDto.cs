using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club.Api.Dtos.Orders
{
    // Reúne los datos mínimos para crear la cita y confirmar el carrito en una sola operación.
    public class CartReservationDto
    {
        [Required(ErrorMessage = "El identificador del vehículo es requerido")]
        [StringLength(36, MinimumLength = 36,
            ErrorMessage = "El identificador del vehículo debe ser un GUID válido")]
        public string VehicleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El identificador del servicio es requerido")]
        [StringLength(36, MinimumLength = 36,
            ErrorMessage = "El identificador del servicio debe ser un GUID válido")]
        public string ServiceId { get; set; } = string.Empty;

        [Display(Name = "Fecha de la reserva")]
        public DateOnly AppointmentDate { get; set; }

        [Display(Name = "Hora de inicio")]
        public TimeOnly StartTime { get; set; }
    }
}
