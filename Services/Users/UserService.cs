using AutoCare_Club.Api.Constants;
using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Entities;
using AutoCare_Club.Api.Mappers;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.User;
using AutoCare_Club_Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club_Api.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly AutoCareDbContext _context;
        private readonly int _defaultPageSize;
        private readonly int _pageSizeLimit;

        public UserService(
            UserManager<UserEntity> userManager,
            RoleManager<RoleEntity> roleManager,
            AutoCareDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;

            var configuredPageSize =
                configuration.GetValue<int>("PageSize");
            var configuredPageSizeLimit =
                configuration.GetValue<int>("PageSizeLimit");

            _defaultPageSize = configuredPageSize > 0
                ? configuredPageSize
                : 10;
            _pageSizeLimit = configuredPageSizeLimit > 0
                ? configuredPageSizeLimit
                : 100;
        }

        public async Task<ResponseDto<PageDto<List<UserDto>>>>
            GetPageAsync(
                string searchTerm = "",
                int page = 1,
                int pageSize = 10)
        {
            page = page > 0 ? page : 1;
            pageSize = pageSize > 0 ? pageSize : _defaultPageSize;
            pageSize = Math.Min(pageSize, _pageSizeLimit);

            var startIndex = (page - 1) * pageSize;
            var usersQuery = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();

                usersQuery = usersQuery.Where(user =>
                    user.FirstName.Contains(term) ||
                    user.LastName.Contains(term) ||
                    (user.Email != null &&
                        user.Email.Contains(term)));
            }

            var totalRows = await usersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(
                (double)totalRows / pageSize);

            var userEntities = await usersQuery
                .OrderBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                .Skip(startIndex)
                .Take(pageSize)
                .ToListAsync();

            return new ResponseDto<PageDto<List<UserDto>>>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = "Usuarios encontrados correctamente.",
                Data = new PageDto<List<UserDto>>
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalRows,
                    TotalPages = totalPages,
                    Items = UserMapper.ListEntityToListDto(userEntities),
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                }
            };
        }

        public async Task<ResponseDto<UserDto>> GetOneAsync(string id)
        {
            var userEntity = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == id);

            if (userEntity is null)
            {
                return new ResponseDto<UserDto>
                {
                    StatusCode = HttpStatusCode.NOT_FOUND,
                    Message = "No se encontró el usuario.",
                    Status = false
                };
            }

            return new ResponseDto<UserDto>
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Usuario encontrado correctamente.",
                Status = true,
                Data = UserMapper.EntityToDto(userEntity)
            };
        }

        public async Task<ResponseDto<UserActionResponseDto>>
            CreateAsync(UserCreateDto dto)
        {
            var rolesValidation = await ValidateRolesAsync(dto.Roles);

            if (rolesValidation is not null)
            {
                return rolesValidation;
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var user = UserMapper.CreateDtoToEntity(dto);
                var createResult = await _userManager.CreateAsync(
                    user,
                    dto.Password);

                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return IdentityErrorResponse(createResult);
                }

                if (dto.Roles is not null && dto.Roles.Count > 0)
                {
                    var addRolesResult = await _userManager
                        .AddToRolesAsync(user, dto.Roles);

                    if (!addRolesResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return IdentityErrorResponse(addRolesResult);
                    }
                }

                await transaction.CommitAsync();

                return new ResponseDto<UserActionResponseDto>
                {
                    StatusCode = HttpStatusCode.CREATED,
                    Status = true,
                    Message = "Usuario creado correctamente.",
                    Data = new UserActionResponseDto
                    {
                        Id = user.Id
                    }
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                return InternalServerErrorResponse();
            }
        }

        public async Task<ResponseDto<UserActionResponseDto>>
            EditAsync(
                string id,
                UserEditDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return new ResponseDto<UserActionResponseDto>
                {
                    StatusCode = HttpStatusCode.NOT_FOUND,
                    Status = false,
                    Message = "No se encontró el usuario."
                };
            }

            var rolesValidation = await ValidateRolesAsync(dto.Roles);

            if (rolesValidation is not null)
            {
                return rolesValidation;
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                UserMapper.EditDtoToEntity(user, dto);

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return IdentityErrorResponse(updateResult);
                }

                if (dto.Roles is not null)
                {
                    var rolesResult = await UpdateRolesAsync(
                        user,
                        dto.Roles);

                    if (rolesResult is not null)
                    {
                        await transaction.RollbackAsync();
                        return rolesResult;
                    }
                }

                await transaction.CommitAsync();

                return new ResponseDto<UserActionResponseDto>
                {
                    StatusCode = HttpStatusCode.OK,
                    Status = true,
                    Message = "Usuario actualizado correctamente.",
                    Data = new UserActionResponseDto
                    {
                        Id = id
                    }
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                return InternalServerErrorResponse();
            }
        }

        public async Task<ResponseDto<UserActionResponseDto>>
            DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
            {
                return new ResponseDto<UserActionResponseDto>
                {
                    StatusCode = HttpStatusCode.NOT_FOUND,
                    Status = false,
                    Message = "No se encontró el usuario."
                };
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var currentRoles =
                    await _userManager.GetRolesAsync(user);

                if (currentRoles.Count > 0)
                {
                    var removeRolesResult = await _userManager
                        .RemoveFromRolesAsync(user, currentRoles);

                    if (!removeRolesResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return IdentityErrorResponse(removeRolesResult);
                    }
                }

                var deleteResult =
                    await _userManager.DeleteAsync(user);

                if (!deleteResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return IdentityErrorResponse(deleteResult);
                }

                await transaction.CommitAsync();

                return new ResponseDto<UserActionResponseDto>
                {
                    StatusCode = HttpStatusCode.OK,
                    Status = true,
                    Message = "Usuario eliminado correctamente.",
                    Data = new UserActionResponseDto
                    {
                        Id = id
                    }
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                return InternalServerErrorResponse();
            }
        }

        private async Task<ResponseDto<UserActionResponseDto>>
            ValidateRolesAsync(IEnumerable<string> roles)
        {
            if (roles is null)
            {
                return null;
            }

            var requestedRoles = roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requestedRoles.Count == 0)
            {
                return null;
            }

            var existingRoles = await _roleManager.Roles
                .Where(role => role.Name != null)
                .Select(role => role.Name)
                .ToListAsync();

            var invalidRoles = requestedRoles
                .Except(
                    existingRoles,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (invalidRoles.Count == 0)
            {
                return null;
            }

            return new ResponseDto<UserActionResponseDto>
            {
                StatusCode = HttpStatusCode.BAD_REQUEST,
                Status = false,
                Message = $"Roles no permitidos: " +
                    string.Join(", ", invalidRoles)
            };
        }

        private async Task<ResponseDto<UserActionResponseDto>>
            UpdateRolesAsync(
                UserEntity user,
                IEnumerable<string> requestedRoles)
        {
            var normalizedRoles = requestedRoles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = normalizedRoles
                .Except(
                    currentRoles,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
            var rolesToRemove = currentRoles
                .Except(
                    normalizedRoles,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (rolesToAdd.Count > 0)
            {
                var addRolesResult = await _userManager
                    .AddToRolesAsync(user, rolesToAdd);

                if (!addRolesResult.Succeeded)
                {
                    return IdentityErrorResponse(addRolesResult);
                }
            }

            if (rolesToRemove.Count > 0)
            {
                var removeRolesResult = await _userManager
                    .RemoveFromRolesAsync(user, rolesToRemove);

                if (!removeRolesResult.Succeeded)
                {
                    return IdentityErrorResponse(removeRolesResult);
                }
            }

            return null;
        }

        private static ResponseDto<UserActionResponseDto>
            IdentityErrorResponse(IdentityResult result)
        {
            return new ResponseDto<UserActionResponseDto>
            {
                StatusCode = HttpStatusCode.BAD_REQUEST,
                Status = false,
                Message = string.Join(
                    ", ",
                    result.Errors.Select(error => error.Description))
            };
        }

        private static ResponseDto<UserActionResponseDto>
            InternalServerErrorResponse()
        {
            return new ResponseDto<UserActionResponseDto>
            {
                StatusCode = HttpStatusCode.INTERNAL_SERVER_ERROR,
                Status = false,
                Message = "Ocurrió un error interno en el servidor."
            };
        }
    }
}
