namespace AutoCare_Club.Api.Dtos.Orders
{
    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string? VehicleId { get; set; }
        public string? AppointmentId { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
