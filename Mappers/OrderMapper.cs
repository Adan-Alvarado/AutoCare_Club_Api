using AutoCare_Club.Api.Dtos.Orders;
using AutoCare_Club.Api.Entities;

namespace AutoCare_Club.Api.Mappers
{
    public static class OrderMapper
    {
        public static OrderDto EntityToDto(OrderEntity entity)
        {
            return new OrderDto
            {
                Id = entity.Id,
                VehicleId = entity.VehicleId,
                AppointmentId = entity.AppointmentId,
                Total = entity.Total,
                Status = entity.Status.ToString(),
                CreatedAt = entity.CreatedAt,
                Items = entity.Items
                    .OrderBy(item => item.Service.Name)
                    .Select(ItemEntityToDto)
                    .ToList()
            };
        }

        private static OrderItemDto ItemEntityToDto(
            OrderItemEntity entity)
        {
            return new OrderItemDto
            {
                Id = entity.Id,
                ServiceId = entity.ServiceId,
                ServiceName = entity.Service.Name,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                Subtotal = entity.Subtotal
            };
        }
    }
}
