using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club_Api.Dtos.Auth
{
    public class LoginDto
    {
        [Display(Name = "Correo Electrónico")]
        [Required(ErrorMessage = "EL {0} es requerido")]
        [EmailAddress(ErrorMessage = "El {0} no tiene un formato válido")]
        public string Email { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La {0} es requerida")]
        public string Password { get; set; }
    }
}