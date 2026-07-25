using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club_Api.Dtos.User
{
    public class UserCreateDto
    {
        [Display(Name = "Nombres")]
        [Required(ErrorMessage = "Los {0} son requeridos")]
        [StringLength(50, ErrorMessage = "Los {0} no pueden tener más de {1} caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "Los {0} son requeridos")]
        [StringLength(50, ErrorMessage = "Los {0} no pueden tener más de {1} caracteres")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Fecha de nacimiento")]
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Los {0} son requeridos")]
        [EmailAddress(ErrorMessage = "El {0} no tiene un formato de correo válido")]
        [StringLength(256, ErrorMessage = "Los {0} no pueden tener más de {1} caracteres")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Roles")]
        public List<string>? Roles { get; set; }  // ['ÁDMIN', 'NORMAL USER']
        
        [Display(Name = "Contraseñá")]
        [Required(ErrorMessage = "Los {0} son requeridos")]
        [StringLength(100, ErrorMessage = "Los {0} no pueden tener más de {1} caracteres")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirmar contraseña")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no son iguales")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}