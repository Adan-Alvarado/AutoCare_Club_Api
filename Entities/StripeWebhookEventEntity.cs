namespace AutoCare_Club.Api.Entities
{
    public class StripeWebhookEventEntity
    {
        public string Id { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
