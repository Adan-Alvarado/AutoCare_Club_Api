using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Dtos.Payments;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Stripe;
using ApiStatusCode = AutoCare_Club.Api.Constants.HttpStatusCode;

namespace AutoCare_Club.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private const string DefaultCurrency = "hnl";

        private readonly AutoCareDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentService(
            AutoCareDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ResponseDto<PaymentIntentDto>>
            CreatePaymentIntentAsync(string userId, string orderId)
        {
            if (!Guid.TryParse(orderId, out _))
            {
                return Error<PaymentIntentDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador de la orden no es valido");
            }

            OrderEntity? order = await _context.Orders
                .FirstOrDefaultAsync(order =>
                    order.Id == orderId
                    && order.UserId == userId);

            if (order is null)
            {
                return Error<PaymentIntentDto>(
                    ApiStatusCode.NOT_FOUND,
                    "La orden no existe o no pertenece al usuario");
            }

            if (order.Status == OrderStatus.Paid)
            {
                return Error<PaymentIntentDto>(
                    ApiStatusCode.CONFLICT,
                    "La orden ya fue pagada");
            }

            if (order.Status != OrderStatus.Pending)
            {
                return Error<PaymentIntentDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "Solo se pueden pagar ordenes pendientes");
            }

            if (order.Total <= 0)
            {
                return Error<PaymentIntentDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "La orden no tiene un total valido");
            }

            string secretKey =
                _configuration["Stripe:SecretKey"] ?? string.Empty;
            string publishableKey =
                _configuration["Stripe:PublishableKey"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(secretKey)
                || string.IsNullOrWhiteSpace(publishableKey))
            {
                return Error<PaymentIntentDto>(
                    ApiStatusCode.SERVICE_UNAVAILABLE,
                    "Stripe todavia no esta configurado");
            }

            string currency = GetCurrency();
            var stripeClient = new StripeClient(secretKey);
            var intentService = new PaymentIntentService(stripeClient);

            try
            {
                PaymentIntent paymentIntent;

                if (string.IsNullOrWhiteSpace(
                    order.StripePaymentIntentId))
                {
                    var options = new PaymentIntentCreateOptions
                    {
                        Amount = ToMinorUnits(order.Total),
                        Currency = currency,
                        Description = $"Orden AutoCare Club {order.Id}",
                        AutomaticPaymentMethods =
                            new PaymentIntentAutomaticPaymentMethodsOptions
                            {
                                Enabled = true
                            },
                        Metadata = new Dictionary<string, string>
                        {
                            ["orderId"] = order.Id,
                            ["userId"] = userId
                        }
                    };

                    var requestOptions = new RequestOptions
                    {
                        IdempotencyKey = $"autocare-order-{order.Id}"
                    };

                    paymentIntent = await intentService.CreateAsync(
                        options,
                        requestOptions);

                    order.StripePaymentIntentId = paymentIntent.Id;
                }
                else
                {
                    paymentIntent = await intentService.GetAsync(
                        order.StripePaymentIntentId);
                }

                order.PaymentStatus = paymentIntent.Status;
                await _context.SaveChangesAsync();

                return Success(
                    ToDto(
                        paymentIntent,
                        publishableKey,
                        order.Total,
                        currency),
                    "Pago preparado correctamente");
            }
            catch (StripeException)
            {
                return Error<PaymentIntentDto>(
                    ApiStatusCode.BAD_GATEWAY,
                    "No fue posible comunicarse con Stripe");
            }
        }

        public async Task<ResponseDto<bool>> ProcessWebhookAsync(
            string payload,
            string signature)
        {
            string webhookSecret =
                _configuration["Stripe:WebhookSecret"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                return Error<bool>(
                    ApiStatusCode.SERVICE_UNAVAILABLE,
                    "El webhook de Stripe todavia no esta configurado");
            }

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    signature,
                    webhookSecret);
            }
            catch (StripeException)
            {
                return Error<bool>(
                    ApiStatusCode.BAD_REQUEST,
                    "La firma del webhook no es valida");
            }

            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
            {
                return Success(true, "Evento recibido");
            }

            if (stripeEvent.Type != EventTypes.PaymentIntentSucceeded
                && stripeEvent.Type
                    != EventTypes.PaymentIntentPaymentFailed
                && stripeEvent.Type
                    != EventTypes.PaymentIntentCanceled)
            {
                return Success(true, "Evento recibido");
            }

            OrderEntity? order = await _context.Orders
                .FirstOrDefaultAsync(order =>
                    order.StripePaymentIntentId == paymentIntent.Id);

            if (order is null)
            {
                return Success(
                    true,
                    "El evento no corresponde a una orden registrada");
            }

            order.PaymentStatus = paymentIntent.Status;

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                bool validAmount = paymentIntent.Amount
                    == ToMinorUnits(order.Total);
                bool validCurrency = string.Equals(
                    paymentIntent.Currency,
                    GetCurrency(),
                    StringComparison.OrdinalIgnoreCase);

                if (!validAmount || !validCurrency)
                {
                    return Error<bool>(
                        ApiStatusCode.BAD_REQUEST,
                        "El monto o la moneda del pago no coincide con la orden");
                }

                order.Status = OrderStatus.Paid;
                order.PaidAt ??= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Success(true, "Evento procesado correctamente");
        }

        private string GetCurrency()
        {
            return (_configuration["Stripe:Currency"]
                ?? DefaultCurrency).ToLowerInvariant();
        }

        private static long ToMinorUnits(decimal amount)
        {
            return decimal.ToInt64(decimal.Round(
                amount * 100,
                0,
                MidpointRounding.AwayFromZero));
        }

        private static PaymentIntentDto ToDto(
            PaymentIntent paymentIntent,
            string publishableKey,
            decimal amount,
            string currency)
        {
            return new PaymentIntentDto
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                PublishableKey = publishableKey,
                Amount = amount,
                Currency = currency,
                Status = paymentIntent.Status
            };
        }

        private static ResponseDto<T> Success<T>(
            T data,
            string message)
        {
            return new ResponseDto<T>
            {
                StatusCode = ApiStatusCode.OK,
                Status = true,
                Message = message,
                Data = data
            };
        }

        private static ResponseDto<T> Error<T>(
            int statusCode,
            string message)
        {
            return new ResponseDto<T>
            {
                StatusCode = statusCode,
                Status = false,
                Message = message
            };
        }
    }
}
