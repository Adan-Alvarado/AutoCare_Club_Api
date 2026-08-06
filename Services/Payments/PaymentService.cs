using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Dtos.Payments;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Stripe;
using Stripe.Checkout;
using ApiStatusCode = AutoCare_Club.Api.Constants.HttpStatusCode;

namespace AutoCare_Club.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private const string DefaultCurrency = "hnl";

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
            catch (StripeException exception)
            {
                _logger.LogError(
                    exception,
                    "Stripe no pudo preparar el pago para la orden {OrderId}.",
                    orderId);

                return Error<PaymentIntentDto>(
                    ApiStatusCode.BAD_GATEWAY,
                    "No fue posible comunicarse con Stripe");
            }
        }

        public async Task<ResponseDto<CheckoutSessionDto>>
            CreateCheckoutSessionAsync(string userId, string orderId)
        {
            if (!Guid.TryParse(orderId, out _))
            {
                return Error<CheckoutSessionDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador de la orden no es valido");
            }

            OrderEntity? order = await _context.Orders
                .FirstOrDefaultAsync(order =>
                    order.Id == orderId
                    && order.UserId == userId);

            if (order is null)
            {
                return Error<CheckoutSessionDto>(
                    ApiStatusCode.NOT_FOUND,
                    "La orden no existe o no pertenece al usuario");
            }

            if (order.Status == OrderStatus.Paid)
            {
                return Error<CheckoutSessionDto>(
                    ApiStatusCode.CONFLICT,
                    "La orden ya fue pagada");
            }

            if (order.Status != OrderStatus.Pending)
            {
                return Error<CheckoutSessionDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "Solo se pueden pagar ordenes pendientes");
            }

            if (order.Total <= 0)
            {
                return Error<CheckoutSessionDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "La orden no tiene un total valido");
            }

            string secretKey =
                _configuration["Stripe:SecretKey"] ?? string.Empty;
            string publishableKey =
                _configuration["Stripe:PublishableKey"] ?? string.Empty;
            string clientUrl =
                _configuration["Stripe:ClientUrl"]
                ?? _configuration["AllowURLS:0"]
                ?? "http://localhost:5173";

            if (string.IsNullOrWhiteSpace(secretKey)
                || string.IsNullOrWhiteSpace(publishableKey))
            {
                return Error<CheckoutSessionDto>(
                    ApiStatusCode.SERVICE_UNAVAILABLE,
                    "Stripe todavia no esta configurado");
            }

            string currency = GetCurrency();
            var stripeClient = new StripeClient(secretKey);
            var sessionService = new SessionService(stripeClient);

            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = currency,
                                UnitAmount = ToMinorUnits(order.Total),
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Orden AutoCare Club #{order.Id[..8]}",
                                    Description = $"Reserva y servicios en AutoCare Club"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"{clientUrl.TrimEnd('/')}/cart/confirmation?session_id={{CHECKOUT_SESSION_ID}}&order_id={order.Id}",
                    CancelUrl = $"{clientUrl.TrimEnd('/')}/cart/payment?canceled=true",
                    Metadata = new Dictionary<string, string>
                    {
                        ["orderId"] = order.Id,
                        ["userId"] = userId
                    },
                    ClientReferenceId = order.Id
                };

                Session session = await sessionService.CreateAsync(options);

                order.StripePaymentIntentId = session.PaymentIntentId ?? session.Id;
                order.PaymentStatus = session.PaymentStatus;
                await _context.SaveChangesAsync();

                var dto = new CheckoutSessionDto
                {
                    SessionId = session.Id,
                    Url = session.Url,
                    PublishableKey = publishableKey,
                    Amount = order.Total,
                    Currency = currency,
                    Status = session.PaymentStatus
                };

                return Success(dto, "Sesion de checkout creada correctamente");
            }
            catch (StripeException exception)
            {
                _logger.LogError(
                    exception,
                    "Stripe no pudo crear la sesion de checkout para la orden {OrderId}.",
                    orderId);

                return Error<CheckoutSessionDto>(
                    ApiStatusCode.BAD_GATEWAY,
                    "No fue posible comunicarse con Stripe");
            }
        }

        public async Task<ResponseDto<CheckoutSessionDto>>
            VerifyCheckoutSessionAsync(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Error<CheckoutSessionDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador de la sesion no es valido");
            }

            string secretKey =
                _configuration["Stripe:SecretKey"] ?? string.Empty;
            string publishableKey =
                _configuration["Stripe:PublishableKey"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return Error<CheckoutSessionDto>(
                    ApiStatusCode.SERVICE_UNAVAILABLE,
                    "Stripe todavia no esta configurado");
            }

            var stripeClient = new StripeClient(secretKey);
            var sessionService = new SessionService(stripeClient);

            try
            {
                Session session = await sessionService.GetAsync(sessionId);

                string? orderId = null;
                if (session.Metadata != null && session.Metadata.TryGetValue("orderId", out string? val))
                {
                    orderId = val;
                }
                orderId ??= session.ClientReferenceId;

                OrderEntity? order = null;
                if (!string.IsNullOrEmpty(orderId))
                {
                    order = await _context.Orders
                        .FirstOrDefaultAsync(o => o.Id == orderId);
                }

                order ??= await _context.Orders
                    .FirstOrDefaultAsync(o =>
                        o.StripePaymentIntentId == session.Id
                        || (session.PaymentIntentId != null && o.StripePaymentIntentId == session.PaymentIntentId));

                if (order == null)
                {
                    return Error<CheckoutSessionDto>(
                        ApiStatusCode.NOT_FOUND,
                        "No se encontro la orden asociada a la sesion");
                }

                order.PaymentStatus = session.PaymentStatus;
                if (session.PaymentStatus == "paid" || session.Status == "complete")
                {
                    order.Status = OrderStatus.Paid;
                    order.PaidAt ??= DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(session.PaymentIntentId))
                    {
                        order.StripePaymentIntentId = session.PaymentIntentId;
                    }
                }

                await _context.SaveChangesAsync();

                var dto = new CheckoutSessionDto
                {
                    SessionId = session.Id,
                    Url = session.Url ?? string.Empty,
                    PublishableKey = publishableKey,
                    Amount = order.Total,
                    Currency = GetCurrency(),
                    Status = session.PaymentStatus
                };

                return Success(dto, "Estado del pago verificado correctamente");
            }
            catch (StripeException exception)
            {
                _logger.LogError(
                    exception,
                    "Error al verificar la sesion de Stripe Checkout {SessionId}.",
                    sessionId);

                return Error<CheckoutSessionDto>(
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
            catch (StripeException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Stripe envio un webhook con una firma invalida.");

                return Error<bool>(
                    ApiStatusCode.BAD_REQUEST,
                    "La firma del webhook no es valida");
            }

            bool alreadyProcessed =
                await _context.StripeWebhookEvents
                    .AsNoTracking()
                    .AnyAsync(webhookEvent =>
                        webhookEvent.Id == stripeEvent.Id);

            if (alreadyProcessed)
            {
                return Success(true, "Evento procesado anteriormente");
            }

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted
                && stripeEvent.Data.Object is Session session)
            {
                string? orderId = null;
                if (session.Metadata != null && session.Metadata.TryGetValue("orderId", out string? val))
                {
                    orderId = val;
                }
                orderId ??= session.ClientReferenceId;

                OrderEntity? sessionOrder = null;
                if (!string.IsNullOrEmpty(orderId))
                {
                    sessionOrder = await _context.Orders
                        .FirstOrDefaultAsync(o => o.Id == orderId);
                }

                sessionOrder ??= await _context.Orders
                    .FirstOrDefaultAsync(o =>
                        o.StripePaymentIntentId == session.Id
                        || (session.PaymentIntentId != null && o.StripePaymentIntentId == session.PaymentIntentId));

                if (sessionOrder != null)
                {
                    sessionOrder.PaymentStatus = session.PaymentStatus;
                    sessionOrder.Status = OrderStatus.Paid;
                    sessionOrder.PaidAt ??= DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(session.PaymentIntentId))
                    {
                        sessionOrder.StripePaymentIntentId = session.PaymentIntentId;
                    }
                }
            }
            else if (stripeEvent.Data.Object is PaymentIntent paymentIntent)
            {
                if (stripeEvent.Type != EventTypes.PaymentIntentSucceeded
                    && stripeEvent.Type != EventTypes.PaymentIntentPaymentFailed
                    && stripeEvent.Type != EventTypes.PaymentIntentCanceled
                    && stripeEvent.Type != "payment_intent.processing")
                {
                    return Success(true, "Evento recibido");
                }

                OrderEntity? order = await _context.Orders
                    .FirstOrDefaultAsync(order =>
                        order.StripePaymentIntentId == paymentIntent.Id);

                if (order is null)
                {
                    _logger.LogWarning(
                        "No se encontro una orden para el PaymentIntent {PaymentIntentId}.",
                        paymentIntent.Id);

                    return Success(
                        true,
                        "El evento no corresponde a una orden registrada");
                }

                order.PaymentStatus = paymentIntent.Status;

                if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
                {
                    bool validAmount = paymentIntent.Amount == ToMinorUnits(order.Total);
                    bool validCurrency = string.Equals(
                        paymentIntent.Currency,
                        GetCurrency(),
                        StringComparison.OrdinalIgnoreCase);

                    if (!validAmount || !validCurrency)
                    {
                        _logger.LogWarning(
                            "El pago {PaymentIntentId} no coincide con el monto o moneda de la orden {OrderId}.",
                            paymentIntent.Id,
                            order.Id);

                        return Error<bool>(
                            ApiStatusCode.BAD_REQUEST,
                            "El monto o la moneda del pago no coincide con la orden");
                    }

                    order.Status = OrderStatus.Paid;
                    order.PaidAt ??= DateTime.UtcNow;
                }
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
                    "El evento Stripe {EventId} ya habia sido procesado.",
                    stripeEvent.Id);
            }

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
