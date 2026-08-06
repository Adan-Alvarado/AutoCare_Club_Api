using AutoCare_Club.Api.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace AutoCare_Club.Api.Controllers
{
    [ApiController]
    [Route("api/stripe/webhook")]
    [AllowAnonymous]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(
            IPaymentService paymentService,
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger)
        {
            _paymentService = paymentService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            string? webhookSecret =
                _configuration["Stripe:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                _logger.LogError(
                    "El secreto del webhook de Stripe no está configurado.");
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable);
            }

            string json = await new StreamReader(
                HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                Event stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret);

                await _paymentService.ProcessWebhookAsync(
                    stripeEvent);

                return Ok();
            }
            catch (StripeException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Stripe envió un webhook con una firma inválida.");
                return BadRequest();
            }
        }
    }
}
