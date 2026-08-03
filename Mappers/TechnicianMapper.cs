using AutoCare_Club_Api.Dtos.Technicians;
using AutoCare_Club_Api.Entities;

namespace AutoCare_Club_Api.Mappers
{
    public class TechnicianMapper
    {
        public static TechnicianEntity CreateDtoToEntity(
            TechnicianCreateDto dto)
        {
            return new TechnicianEntity
            {
                UserId = dto.UserId,
                Specialty = dto.Specialty.Trim(),
                IsActive = true
            };
        }

        public static TechnicianEntity EditDtoToEntity(
            TechnicianEntity entity,
            TechnicianEditDto dto)
        {
            entity.Specialty = dto.Specialty.Trim();
            entity.IsActive = dto.IsActive;

            return entity;
        }

        public static TechnicianDto EntityToDto(
            TechnicianEntity entity)
        {
            return new TechnicianDto
            {
                UserId = entity.UserId,
                FirstName = entity.User.FirstName,
                LastName = entity.User.LastName,
                Email = entity.User.Email ??
                    string.Empty,
                Specialty = entity.Specialty,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
        }

        public static List<TechnicianDto>
            ListEntityToListDto(
                IEnumerable<TechnicianEntity> entities)
        {
            return entities
                .Select(EntityToDto)
                .ToList();
        }
    }
}