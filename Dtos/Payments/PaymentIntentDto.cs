namespace AutoCare_Club.Api.Dtos.Payments
{
    public class PaymentIntentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
