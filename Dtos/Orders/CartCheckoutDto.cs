using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club.Api.Dtos.Orders
{
    public class CartCheckoutDto
    {
        [Required(ErrorMessage = "El identificador del vehículo es requerido")]
        [StringLength(36, MinimumLength = 36,
            ErrorMessage = "El identificador del vehículo debe ser un GUID válido")]
        public string VehicleId { get; set; } = string.Empty;

        [StringLength(36, MinimumLength = 36,
            ErrorMessage = "El identificador de la cita debe ser un GUID válido")]
        public string? AppointmentId { get; set; }
    }
}
