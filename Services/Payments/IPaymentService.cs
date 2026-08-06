using AutoCare_Club.Api.Dtos.Payments;
using AutoCare_Club_Api.Dtos.Common;
using Stripe;

namespace AutoCare_Club.Api.Services.Payments
{
    public interface IPaymentService
    {
        Task<ResponseDto<PaymentIntentDto>> CreatePaymentIntentAsync(
            string userId,
            string orderId);

        Task ProcessWebhookAsync(Event stripeEvent);
    }
}
