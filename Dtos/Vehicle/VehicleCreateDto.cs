using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club.Api.Dtos.Vehicle
{
    public class VehicleCreateDto
    {
        [Required(ErrorMessage = "El identificador del usuario es requerido")]
        [StringLength(36, MinimumLength = 36, ErrorMessage = "El identificador del usuario debe ser un GUID válido")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca del vehículo es requerida")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 50 caracteres")]
        public string Brand { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "La matrícula es requerida")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "La matrícula debe tener entre 4 y 20 caracteres")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de vehículo es requerido")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "El tipo debe tener entre 4 y 20 caracteres")]
        public string VehicleType { get; set; } = string.Empty;
    }
}
