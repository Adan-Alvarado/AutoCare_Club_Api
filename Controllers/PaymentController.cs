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

        [HttpPost("orders/{orderId:guid}/checkout-session")]
        public async Task<ActionResult<ResponseDto<CheckoutSessionDto>>>
            CreateCheckoutSession(string orderId)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return Unauthorized(new ResponseDto<CheckoutSessionDto>
                {
                    Status = false,
                    Message = "El token no contiene un usuario valido"
                });
            }

            ResponseDto<CheckoutSessionDto> response =
                await _paymentService.CreateCheckoutSessionAsync(
                    userId,
                    orderId);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("sessions/{sessionId}/verify")]
        public async Task<ActionResult<ResponseDto<CheckoutSessionDto>>>
            VerifyCheckoutSession(string sessionId)
        {
            ResponseDto<CheckoutSessionDto> response =
                await _paymentService.VerifyCheckoutSessionAsync(sessionId);

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
