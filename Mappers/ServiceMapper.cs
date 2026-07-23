using AutoCare_Club.Api.Dtos.Services;
using AutoCare_Club.Api.Entities;

namespace AutoCare_Club.Api.Mappers
{
    public static class ServiceMapper
    {
        public static ServiceEntity CreateDtoToEntity(
            ServiceCreateDto dto)
        {
            return new ServiceEntity
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                DurationMinutes = dto.DurationMinutes,
                ImageUrl = dto.ImageUrl?.Trim(),
                IsActive = true
            };
        }

        public static ServiceDto EntityToDto(
            ServiceEntity entity)
        {
            return new ServiceDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                DurationMinutes = entity.DurationMinutes,
                ImageUrl = entity.ImageUrl,
                IsActive = entity.IsActive
            };
        }

        public static ServiceEntity EditDtoToEntity(
            ServiceEntity entity,
            ServiceEditDto dto)
        {
            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description.Trim();
            entity.Price = dto.Price;
            entity.DurationMinutes = dto.DurationMinutes;
            entity.ImageUrl = dto.ImageUrl?.Trim();
            entity.IsActive = dto.IsActive;

            return entity;
        }

        public static List<ServiceDto> ListEntityToListDto(
            IEnumerable<ServiceEntity> entities)
        {
            return entities
                .Select(EntityToDto)
                .ToList();
        }
    }
}