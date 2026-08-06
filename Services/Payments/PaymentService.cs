using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Dtos.Payments;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Stripe;
using ApiStatusCode = AutoCare_Club.Api.Constants.HttpStatusCode;

namespace AutoCare_Club.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly AutoCareDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            AutoCareDbContext context,
            IConfiguration configuration,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ResponseDto<PaymentIntentDto>>
            CreatePaymentIntentAsync(
                string userId,
                string orderId)
        {
            if (!Guid.TryParse(orderId, out _))
            {
                return Error(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador de la orden no es válido.");
            }

            string? secretKey =
                _configuration["Stripe:SecretKey"];
            string? publishableKey =
                _configuration["Stripe:PublishableKey"];
            string currency =
                _configuration["Stripe:Currency"] ?? "hnl";

            if (string.IsNullOrWhiteSpace(secretKey)
                || string.IsNullOrWhiteSpace(publishableKey))
            {
                return Error(
                    ApiStatusCode.SERVICE_UNAVAILABLE,
                    "Stripe no está configurado en el servidor.");
            }

            OrderEntity? order = await _context.Orders
                .FirstOrDefaultAsync(order =>
                    order.Id == orderId
                    && order.UserId == userId);

            if (order is null)
            {
                return Error(
                    ApiStatusCode.NOT_FOUND,
                    "La orden no fue encontrada.");
            }

            if (order.Status == OrderStatus.Draft)
            {
                return Error(
                    ApiStatusCode.BAD_REQUEST,
                    "La orden debe confirmarse antes de pagar.");
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return Error(
                    ApiStatusCode.CONFLICT,
                    "La orden ya se encuentra pagada.");
            }

            if (order.Status == OrderStatus.Cancelled
                || order.Status == OrderStatus.Completed)
            {
                return Error(
                    ApiStatusCode.CONFLICT,
                    "La orden ya no admite pagos.");
            }

            try
            {
                StripeClient stripeClient = new(secretKey);
                PaymentIntentService intentService =
                    new(stripeClient);

                PaymentIntent paymentIntent;

                if (!string.IsNullOrWhiteSpace(
                    order.StripePaymentIntentId))
                {
                    paymentIntent = await intentService.GetAsync(
                        order.StripePaymentIntentId);
                }
                else
                {
                    long amount = ToMinorUnits(order.Total);

                    if (amount <= 0)
                    {
                        return Error(
                            ApiStatusCode.BAD_REQUEST,
                            "El total de la orden debe ser mayor que cero.");
                    }

                    PaymentIntentCreateOptions options = new()
                    {
                        Amount = amount,
                        Currency = currency.ToLowerInvariant(),
                        AutomaticPaymentMethods =
                            new PaymentIntentAutomaticPaymentMethodsOptions
                            {
                                Enabled = true
                            },
                        Metadata = new Dictionary<string, string>
                        {
                            ["orderId"] = order.Id,
                            ["userId"] = order.UserId
                        },
                        Description = $"Orden AutoCare Club {order.Id}"
                    };

                    paymentIntent = await intentService.CreateAsync(
                        options,
                        new RequestOptions
                        {
                            IdempotencyKey =
                                $"autocare-order-{order.Id}"
                        });

                    order.StripePaymentIntentId = paymentIntent.Id;
                    order.PaymentStatus = PaymentStatus.Pending;
                    await _context.SaveChangesAsync();
                }

                return Success(new PaymentIntentDto
                {
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    PublishableKey = publishableKey,
                    Amount = paymentIntent.Amount,
                    Currency = paymentIntent.Currency,
                    Status = paymentIntent.Status
                });
            }
            catch (StripeException exception)
            {
                _logger.LogError(
                    exception,
                    "Stripe no pudo preparar el pago para la orden {OrderId}.",
                    orderId);

                return Error(
                    ApiStatusCode.BAD_GATEWAY,
                    "Stripe no pudo preparar el pago. Intenta nuevamente.");
            }
        }

        public async Task ProcessWebhookAsync(Event stripeEvent)
        {
            bool alreadyProcessed =
                await _context.StripeWebhookEvents
                    .AsNoTracking()
                    .AnyAsync(webhookEvent =>
                        webhookEvent.Id == stripeEvent.Id);

            if (alreadyProcessed)
            {
                return;
            }

            if (stripeEvent.Data.Object is not PaymentIntent intent)
            {
                return;
            }

            if (!intent.Metadata.TryGetValue(
                "orderId",
                out string? orderId))
            {
                _logger.LogWarning(
                    "El evento Stripe {EventId} no contiene orderId.",
                    stripeEvent.Id);
                return;
            }

            OrderEntity? order = await _context.Orders
                .FirstOrDefaultAsync(order =>
                    order.Id == orderId
                    && order.StripePaymentIntentId == intent.Id);

            if (order is null)
            {
                _logger.LogWarning(
                    "No se encontró una orden para el PaymentIntent {PaymentIntentId}.",
                    intent.Id);
                return;
            }

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.Status = OrderStatus.Paid;
                    order.PaidAt ??= DateTime.UtcNow;
                    break;

                case "payment_intent.processing":
                    if (order.PaymentStatus != PaymentStatus.Paid)
                    {
                        order.PaymentStatus = PaymentStatus.Processing;
                    }
                    break;

                case "payment_intent.payment_failed":
                    if (order.PaymentStatus != PaymentStatus.Paid)
                    {
                        order.PaymentStatus = PaymentStatus.Failed;
                    }
                    break;

                case "payment_intent.canceled":
                    if (order.PaymentStatus != PaymentStatus.Paid)
                    {
                        order.PaymentStatus = PaymentStatus.Cancelled;
                    }
                    break;

                default:
                    return;
            }

            await _context.StripeWebhookEvents.AddAsync(
                new StripeWebhookEventEntity
                {
                    Id = stripeEvent.Id,
                    EventType = stripeEvent.Type
                });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (
                exception.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: "PK_StripeWebhookEvents"
                })
            {
                _logger.LogInformation(
                    "El evento Stripe {EventId} ya había sido procesado.",
                    stripeEvent.Id);
            }
        }

        private static long ToMinorUnits(decimal amount)
        {
            decimal minorUnits = decimal.Round(
                amount * 100m,
                0,
                MidpointRounding.AwayFromZero);

            return decimal.ToInt64(minorUnits);
        }

        private static ResponseDto<PaymentIntentDto> Success(
            PaymentIntentDto data)
        {
            return new ResponseDto<PaymentIntentDto>
            {
                StatusCode = ApiStatusCode.CREATED,
                Status = true,
                Message = "Pago preparado correctamente.",
                Data = data
            };
        }

        private static ResponseDto<PaymentIntentDto> Error(
            int statusCode,
            string message)
        {
            return new ResponseDto<PaymentIntentDto>
            {
                StatusCode = statusCode,
                Status = false,
                Message = message
            };
        }
    }
}
