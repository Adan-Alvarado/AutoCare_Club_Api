namespace AutoCare_Club.Api.Entities
{
    public enum OrderStatus
    {
        Draft,
        Pending,
        Paid,
        Cancelled,
        Completed
    }

    public class OrderEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string? VehicleId { get; set; }
        public string? AppointmentId { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItemEntity> Items { get; set; } =
            new List<OrderItemEntity>();
    }
}
