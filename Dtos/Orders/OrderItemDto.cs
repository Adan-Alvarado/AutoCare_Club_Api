namespace AutoCare_Club.Api.Dtos.Orders
{
    public class OrderItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ServiceId { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}
