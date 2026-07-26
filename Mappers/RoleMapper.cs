using AutoCare_Club_Api.Dtos.Roles;
using AutoCare_Club_Api.Entities;

namespace AutoCare_Club_Api.Mappers
{
    public static class RoleMapper
    {
        public static RoleEntity CreateDtoToEntity(
            RoleCreateDto dto)
        {
            return new RoleEntity
            {
                Name = dto.Name.Trim(),
                Descripcion = dto.Description.Trim()
            };
        }

        public static RoleEntity EditDtoToEntity(
            RoleEntity entity,
            RoleEditDto dto)
        {
            entity.Name = dto.Name.Trim();
            entity.Descripcion = dto.Description.Trim();

            return entity;
        }

        public static List<RoleDto> ListEntityToListDto(
            IEnumerable<RoleEntity> entities)
        {
            return entities
                .Select(EntityToDto)
                .ToList();
        }

        public static RoleDto EntityToDto(RoleEntity entity)
        {
            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name ?? string.Empty,
                Description = entity.Descripcion
            };
        }

        public static RoleActionResponseDto
            EntityToActionResponseDto(RoleEntity entity)
        {
            return new RoleActionResponseDto
            {
                Id = entity.Id,
                Name = entity.Name ?? string.Empty
            };
        }
    }
}
