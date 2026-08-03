using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club_Api.Dtos.Technicians
{
    public class TechnicianEditDto
    {
         [Required(ErrorMessage = "La especialidad es requerida")]
        [StringLength(100, MinimumLength = 2, ErrorMessage =
                "La especialidad debe tener entre 2 y 100 caracteres")]
        public string Specialty { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}