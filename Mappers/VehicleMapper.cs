using AutoCare_Club.Api.Dtos.Vehicle;
using AutoCare_Club.Api.Entities;

namespace AutoCare_Club.Api.Mappers
{
    public static class VehicleMapper
    {
        public static VehicleEntity CreateDtoToEntity(
            VehicleCreateDto dto,
            string userId)
        {
            return new VehicleEntity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Brand = dto.Brand.Trim(),
                Year = dto.Year,
                LicensePlate = dto.LicensePlate.Trim(),
                VehicleType = dto.VehicleType.Trim(),
                IsActive = true
            };
        }

        public static VehicleDto EntityToDto(
            VehicleEntity entity)
        {
            return new VehicleDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Brand = entity.Brand,
                Year = entity.Year,
                LicensePlate = entity.LicensePlate,
                VehicleType = entity.VehicleType,
                IsActive = entity.IsActive
            };
        }

        public static VehicleEntity EditDtoToEntity(
            VehicleEntity entity,
            VehicleEditDto dto)
        {
            entity.Brand = dto.Brand.Trim();
            entity.Year = dto.Year;
            entity.LicensePlate = dto.LicensePlate.Trim();
            entity.VehicleType = dto.VehicleType.Trim();

            return entity;
        }

        public static List<VehicleDto> ListEntityToListDto(
            IEnumerable<VehicleEntity> entities)
        {
            return entities
                .Select(EntityToDto)
                .ToList();
        }
    }
}
