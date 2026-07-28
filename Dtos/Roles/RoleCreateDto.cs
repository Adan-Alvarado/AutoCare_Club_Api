using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club_Api.Dtos.Roles
{
    public class RoleCreateDto
    {
        [Display(Name = "Rol")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string Description { get; set; } = string.Empty;
    }
}