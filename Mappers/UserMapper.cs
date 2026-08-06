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
            IEnumerable<UserEntity> entities,
            IReadOnlyDictionary<string, List<string>>? rolesByUserId = null)
        {
            return entities
                .Select(entity => EntityToDto(entity, rolesByUserId?.TryGetValue(entity.Id, out var roles) == true ? roles : null))
                .ToList();
        }

        public static UserDto EntityToDto(
            UserEntity entity,
            IEnumerable<string>? roles = null)
        {
            return new UserDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email ?? string.Empty,
                Roles = roles?
                    .Where(role => !string.IsNullOrWhiteSpace(role))
                    .Select(role => role.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>(),
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
