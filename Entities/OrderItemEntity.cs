namespace AutoCare_Club.Api.Entities
{
    public class OrderItemEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OrderId { get; set; } = string.Empty;
        public string ServiceId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        public OrderEntity Order { get; set; } = null!;
        public ServiceEntity Service { get; set; } = null!;
    }
}
