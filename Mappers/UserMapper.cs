using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Dtos.User;

namespace AutoCare_Club.Api.Mappers
{
    public static class UserMapper
    {
        public static UserEntity CreateDtoToEntity(
            UserCreateDto dto)
        {
            return new UserEntity
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.Trim(),
                UserName = dto.Email.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static UserEntity EditDtoToEntity(
            UserEntity entity,
            UserEditDto dto)
        {
            entity.FirstName = dto.FirstName.Trim();
            entity.LastName = dto.LastName.Trim();
            entity.Email = dto.Email.Trim();
            entity.UserName = dto.Email.Trim();

            return entity;
        }

        public static List<UserDto> ListEntityToListDto(
            IEnumerable<UserEntity> entities)
        {
            return entities
                .Select(EntityToDto)
                .ToList();
        }

        public static UserDto EntityToDto(
            UserEntity entity)
        {
            return new UserDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
