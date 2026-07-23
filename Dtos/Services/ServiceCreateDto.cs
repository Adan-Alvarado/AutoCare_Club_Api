using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club.Api.Dtos.Services
{
    public class ServiceCreateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Descrpcion es requerida")]
        [StringLength(300, MinimumLength = 10, ErrorMessage = "La Descripcion debe tener entre 10 y 300 caracteres")]
        public string Description { get; set; } = string.Empty;
        
        [Range( 0.01, 999999.99, ErrorMessage = "El Precio debe ser mayor que cero" )]
        public decimal Price { get; set; }

        [Range( 10, 1440, ErrorMessage = "La duracion debe estar entre 10 min a 1440 min(1 dia)" )]
        public int DurationMinutes { get; set; }
        
        [Url(ErrorMessage = "La dirección de la imagen no es válida")]
        public string? ImageUrl { get; set; }
    }
}