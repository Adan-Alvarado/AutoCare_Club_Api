using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club.Api.Dtos.Orders
{
    public class CartItemCreateDto
    {
        [Required(ErrorMessage = "El identificador del servicio es requerido")]
        [StringLength(36, MinimumLength = 36,
            ErrorMessage = "El identificador del servicio debe ser un GUID válido")]
        public string ServiceId { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "La cantidad debe estar entre 1 y 10")]
        public int Quantity { get; set; } = 1;
    }
}
