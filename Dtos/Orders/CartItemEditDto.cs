using System.ComponentModel.DataAnnotations;

namespace AutoCare_Club.Api.Dtos.Orders
{
    public class CartItemEditDto
    {
        [Range(1, 10, ErrorMessage = "La cantidad debe estar entre 1 y 10")]
        public int Quantity { get; set; }
    }
}
