using System.Security.Claims;
using AutoCare_Club.Api.Dtos.Payments;
using AutoCare_Club.Api.Services.Payments;
using AutoCare_Club_Api.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("orders/{orderId:guid}/intent")]
        public async Task<ActionResult<ResponseDto<PaymentIntentDto>>>
            CreatePaymentIntent(string orderId)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return Unauthorized(new ResponseDto<PaymentIntentDto>
                {
                    Status = false,
                    Message = "El token no contiene un usuario valido"
                });
            }

            ResponseDto<PaymentIntentDto> response =
                await _paymentService.CreatePaymentIntentAsync(
                    userId,
                    orderId);

            return StatusCode(response.StatusCode, response);
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
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

        private string? GetAuthenticatedUserId()
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("UserId");

            return Guid.TryParse(userId, out _)
                ? userId
                : null;
        }
    }
}
