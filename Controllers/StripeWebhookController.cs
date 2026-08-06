using AutoCare_Club.Api.Services.Payments;
using AutoCare_Club_Api.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club.Api.Controllers
{
    [ApiController]
    [Route("api/stripe/webhook")]
    [AllowAnonymous]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public StripeWebhookController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            using var reader = new StreamReader(Request.Body);
            string payload = await reader.ReadToEndAsync();
            string signature = Request.Headers["Stripe-Signature"]
                .ToString();

            ResponseDto<bool> response =
                await _paymentService.ProcessWebhookAsync(
                    payload,
                    signature);

            return StatusCode(response.StatusCode, response);
        }
    }
}
