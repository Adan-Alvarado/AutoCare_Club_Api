using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Dtos.Orders;
using AutoCare_Club.Api.Entities;
using AutoCare_Club.Api.Mappers;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Entities;
using Microsoft.EntityFrameworkCore;
using ApiStatusCode = AutoCare_Club.Api.Constants.HttpStatusCode;

namespace AutoCare_Club.Api.Services.Orders
{
    public class OrderService : IOrderService
    {
        private const int MaximumQuantityPerService = 10;
        private readonly AutoCareDbContext _context;

        public OrderService(AutoCareDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseDto<OrderDto>> GetCartAsync(
            string userId)
        {
            OrderEntity? order = await OrderQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(order =>
                    order.UserId == userId
                    && order.Status == OrderStatus.Draft);

            if (order is null)
            {
                return Error<OrderDto>(
                    ApiStatusCode.NOT_FOUND,
                    "No hay un carrito activo.");
            }

            return Success(
                OrderMapper.EntityToDto(order),
                "Carrito encontrado correctamente.");
        }

        public async Task<ResponseDto<OrderDto>> AddCartItemAsync(
            string userId,
            CartItemCreateDto dto)
        {
            if (!Guid.TryParse(dto.ServiceId, out _))
            {
                return Error<OrderDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador del servicio no es válido.");
            }

            ServiceEntity? service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(service =>
                    service.Id == dto.ServiceId
                    && service.IsActive);

            if (service is null)
            {
                return Error<OrderDto>(
                    ApiStatusCode.NOT_FOUND,
                    "El servicio no existe o está inactivo.");
            }

            OrderEntity? order = await OrderQuery()
                .FirstOrDefaultAsync(order =>
                    order.UserId == userId
                    && order.Status == OrderStatus.Draft);

            if (order is null)
            {
                order = new OrderEntity
                {
                    UserId = userId
                };

                await _context.Orders.AddAsync(order);
            }

            OrderItemEntity? existingItem = order.Items
                .FirstOrDefault(item => item.ServiceId == service.Id);

            if (existingItem is not null)
            {
                int newQuantity = existingItem.Quantity + dto.Quantity;

                if (newQuantity > MaximumQuantityPerService)
                {
                    return Error<OrderDto>(
                        ApiStatusCode.BAD_REQUEST,
                        "La cantidad acumulada no puede ser mayor que 10.");
                }

                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = service.Price;
                existingItem.Subtotal = service.Price * newQuantity;
            }
            else
            {
                order.Items.Add(new OrderItemEntity
                {
                    ServiceId = service.Id,
                    Quantity = dto.Quantity,
                    UnitPrice = service.Price,
                    Subtotal = service.Price * dto.Quantity
                });
            }

            RecalculateTotal(order);
            await _context.SaveChangesAsync();

            return Success(
                (await GetOrderForUserAsync(
                    order.Id,
                    userId,
                    includeDraft: true))!,
                "Servicio agregado al carrito.",
                ApiStatusCode.CREATED);
        }

        public async Task<ResponseDto<OrderDto>> EditCartItemAsync(
            string userId,
            string itemId,
            CartItemEditDto dto)
        {
            if (!Guid.TryParse(itemId, out _))
            {
                return Error<OrderDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador del elemento no es válido.");
            }

            OrderEntity? order = await OrderQuery()
                .FirstOrDefaultAsync(order =>
                    order.UserId == userId
                    && order.Status == OrderStatus.Draft
                    && order.Items.Any(item => item.Id == itemId));

            if (order is null)
            {
                return Error<OrderDto>(
                    ApiStatusCode.NOT_FOUND,
                    "El elemento no fue encontrado en el carrito.");
            }

            OrderItemEntity item = order.Items
                .First(item => item.Id == itemId);

            item.Quantity = dto.Quantity;
            item.Subtotal = item.UnitPrice * dto.Quantity;
            RecalculateTotal(order);

            await _context.SaveChangesAsync();

            return Success(
                OrderMapper.EntityToDto(order),
                "Cantidad actualizada correctamente.");
        }

        public async Task<ResponseDto<bool>> DeleteCartItemAsync(
            string userId,
            string itemId)
        {
            if (!Guid.TryParse(itemId, out _))
            {
                return Error<bool>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador del elemento no es válido.");
            }

            OrderEntity? order = await OrderQuery()
                .FirstOrDefaultAsync(order =>
                    order.UserId == userId
                    && order.Status == OrderStatus.Draft
                    && order.Items.Any(item => item.Id == itemId));

            if (order is null)
            {
                return Error<bool>(
                    ApiStatusCode.NOT_FOUND,
                    "El elemento no fue encontrado en el carrito.");
            }

            OrderItemEntity item = order.Items
                .First(item => item.Id == itemId);

            _context.OrderItems.Remove(item);
            order.Items.Remove(item);
            RecalculateTotal(order);

            await _context.SaveChangesAsync();

            return Success(
                true,
                "Servicio eliminado del carrito.",
                ApiStatusCode.NO_CONTENT);
        }

        public async Task<ResponseDto<OrderDto>> CheckoutAsync(
            string userId,
            CartCheckoutDto dto)
        {
            if (!Guid.TryParse(dto.VehicleId, out _))
            {
                return Error<OrderDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador del vehículo no es válido.");
            }

            if (!string.IsNullOrWhiteSpace(dto.AppointmentId)
                && !Guid.TryParse(dto.AppointmentId, out _))
            {
                return Error<OrderDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador de la cita no es válido.");
            }

            bool vehicleBelongsToUser = await _context.Vehicles
                .AsNoTracking()
                .AnyAsync(vehicle =>
                    vehicle.Id == dto.VehicleId
                    && vehicle.UserId == userId
                    && vehicle.IsActive);

            if (!vehicleBelongsToUser)
            {
                return Error<OrderDto>(
                    ApiStatusCode.NOT_FOUND,
                    "El vehículo no existe o no pertenece al usuario.");
            }

            AppointmentEntity? appointment = null;

            if (!string.IsNullOrWhiteSpace(dto.AppointmentId))
            {
                appointment = await _context.Appointments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(appointment =>
                        appointment.Id == dto.AppointmentId
                        && appointment.UserId == userId);

                if (appointment is null)
                {
                    return Error<OrderDto>(
                        ApiStatusCode.NOT_FOUND,
                        "La cita no existe o no pertenece al usuario.");
                }

                if (appointment.VehicleId != dto.VehicleId)
                {
                    return Error<OrderDto>(
                        ApiStatusCode.BAD_REQUEST,
                        "La cita no corresponde al vehículo seleccionado.");
                }

                if (appointment.Status == AppointmentStatus.Cancelled)
                {
                    return Error<OrderDto>(
                        ApiStatusCode.BAD_REQUEST,
                        "No se puede utilizar una cita cancelada.");
                }
            }

            OrderEntity? order = await OrderQuery()
                .FirstOrDefaultAsync(order =>
                    order.UserId == userId
                    && order.Status == OrderStatus.Draft);

            if (order is null)
            {
                return Error<OrderDto>(
                    ApiStatusCode.NOT_FOUND,
                    "No hay un carrito activo.");
            }

            if (order.Items.Count == 0)
            {
                return Error<OrderDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "No se puede confirmar un carrito vacío.");
            }

            if (order.Items.Any(item => !item.Service.IsActive))
            {
                return Error<OrderDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El carrito contiene servicios que ya no están activos.");
            }

            if (appointment is not null
                && !order.Items.Any(item =>
                    item.ServiceId == appointment.ServiceId))
            {
                return Error<OrderDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El servicio de la cita no está incluido en la orden.");
            }

            if (appointment is not null)
            {
                bool appointmentAlreadyUsed = await _context.Orders
                    .AsNoTracking()
                    .AnyAsync(existingOrder =>
                        existingOrder.Id != order.Id
                        && existingOrder.AppointmentId == appointment.Id);

                if (appointmentAlreadyUsed)
                {
                    return Error<OrderDto>(
                        ApiStatusCode.CONFLICT,
                        "La cita ya está relacionada con otra orden.");
                }
            }

            order.VehicleId = dto.VehicleId;
            order.AppointmentId = string.IsNullOrWhiteSpace(
                dto.AppointmentId)
                ? null
                : dto.AppointmentId;
            order.Status = OrderStatus.Pending;
            RecalculateTotal(order);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException) when (appointment is not null)
            {
                return Error<OrderDto>(
                    ApiStatusCode.CONFLICT,
                    "La cita ya no está disponible para esta orden.");
            }

            return Success(
                OrderMapper.EntityToDto(order),
                "Orden confirmada correctamente.");
        }

        public async Task<ResponseDto<OrderDto>> GetOrderAsync(
            string userId,
            string orderId)
        {
            if (!Guid.TryParse(orderId, out _))
            {
                return Error<OrderDto>(
                    ApiStatusCode.BAD_REQUEST,
                    "El identificador de la orden no es válido.");
            }

            OrderDto? order = await GetOrderForUserAsync(
                orderId,
                userId,
                includeDraft: false);

            if (order is null)
            {
                return Error<OrderDto>(
                    ApiStatusCode.NOT_FOUND,
                    "La orden no fue encontrada.");
            }

            return Success(
                order,
                "Orden encontrada correctamente.");
        }

        public async Task<ResponseDto<List<OrderDto>>>
            GetCustomerOrdersAsync(string userId)
        {
            List<OrderEntity> orders = await OrderQuery()
                .AsNoTracking()
                .Where(order =>
                    order.UserId == userId
                    && order.Status != OrderStatus.Draft)
                .OrderByDescending(order => order.CreatedAt)
                .ToListAsync();

            return Success(
                orders.Select(OrderMapper.EntityToDto).ToList(),
                "Órdenes encontradas correctamente.");
        }

        private IQueryable<OrderEntity> OrderQuery()
        {
            return _context.Orders
                .Include(order => order.Items)
                .ThenInclude(item => item.Service);
        }

        private async Task<OrderDto?> GetOrderForUserAsync(
            string orderId,
            string userId,
            bool includeDraft)
        {
            IQueryable<OrderEntity> query = OrderQuery()
                .AsNoTracking()
                .Where(order =>
                    order.Id == orderId
                    && order.UserId == userId);

            if (!includeDraft)
            {
                query = query.Where(order =>
                    order.Status != OrderStatus.Draft);
            }

            OrderEntity? order = await query.FirstOrDefaultAsync();

            return order is null
                ? null
                : OrderMapper.EntityToDto(order);
        }

        private static void RecalculateTotal(OrderEntity order)
        {
            order.Total = order.Items.Sum(item => item.Subtotal);
        }

        private static ResponseDto<T> Success<T>(
            T data,
            string message,
            int statusCode = ApiStatusCode.OK)
        {
            return new ResponseDto<T>
            {
                StatusCode = statusCode,
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
