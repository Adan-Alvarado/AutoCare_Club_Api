using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club_Api.Dtos.Technicians
{
    public class TechnicianCreateDto
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        [StringLength(
            36, MinimumLength = 36, ErrorMessage = "El identificador debe ser un GUID válido")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especialidad es requerida")]
        [StringLength(
            100, MinimumLength = 2, ErrorMessage = "La especialidad debe tener entre 2 y 100 caracteres")]
        public string Specialty { get; set; } = string.Empty;
    }
}