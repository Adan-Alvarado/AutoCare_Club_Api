using AutoCare_Club.Api.Constants;
using AutoCare_Club.Api.Database;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.Roles;
using AutoCare_Club_Api.Entities;
using AutoCare_Club_Api.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoCare_Club_Api.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<RoleEntity> _roleManager;
        private readonly AutoCareDbContext _context;
        private readonly int _defaultPageSize;
        private readonly int _pageSizeLimit;

        public RoleService(
            RoleManager<RoleEntity> roleManager,
            AutoCareDbContext context,
            IConfiguration configuration)
        {
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

        public async Task<ResponseDto<PageDto<List<RoleDto>>>>
            GetPageAsync(
                string searchTerm = "",
                int page = 1,
                int pageSize = 10)
        {
            page = page > 0 ? page : 1;
            pageSize = pageSize > 0
                ? pageSize
                : _defaultPageSize;
            pageSize = Math.Min(pageSize, _pageSizeLimit);

            var startIndex = (page - 1) * pageSize;
            var rolesQuery = _context.Roles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();

                rolesQuery = rolesQuery.Where(role =>
                    (role.Name != null &&
                        role.Name.Contains(term)) ||
                    role.Descripcion.Contains(term));
            }

            var totalRows = await rolesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(
                (double)totalRows / pageSize);

            var roleEntities = await rolesQuery
                .OrderBy(role => role.Name)
                .Skip(startIndex)
                .Take(pageSize)
                .ToListAsync();

            return new ResponseDto<PageDto<List<RoleDto>>>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = HttpMessageResponse.REGISTERS_FOUND,
                Data = new PageDto<List<RoleDto>>
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalRows,
                    TotalPages = totalPages,
                    Items = RoleMapper.ListEntityToListDto(
                        roleEntities),
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                }
            };
        }

        public async Task<ResponseDto<RoleDto>> GetOneAsync(
            string id)
        {
            var roleEntity = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(role => role.Id == id);

            if (roleEntity is null)
            {
                return new ResponseDto<RoleDto>
                {
                    StatusCode = HttpStatusCode.NOT_FOUND,
                    Status = false,
                    Message =
                        HttpMessageResponse.REGISTER_NOT_FOUND
                };
            }

            return new ResponseDto<RoleDto>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = HttpMessageResponse.REGISTER_FOUND,
                Data = RoleMapper.EntityToDto(roleEntity)
            };
        }

        public async Task<ResponseDto<RoleActionResponseDto>>
            CreateAsync(RoleCreateDto dto)
        {
            var roleEntity =
                RoleMapper.CreateDtoToEntity(dto);

            var result =
                await _roleManager.CreateAsync(roleEntity);

            if (!result.Succeeded)
            {
                return IdentityErrorResponse(result);
            }

            return new ResponseDto<RoleActionResponseDto>
            {
                StatusCode = HttpStatusCode.CREATED,
                Status = true,
                Message = HttpMessageResponse.REGISTER_CREATED,
                Data = RoleMapper.EntityToActionResponseDto(
                    roleEntity)
            };
        }

        public async Task<ResponseDto<RoleActionResponseDto>>
            EditAsync(
                string id,
                RoleEditDto dto)
        {
            var roleEntity =
                await _roleManager.FindByIdAsync(id);

            if (roleEntity is null)
            {
                return NotFoundResponse();
            }

            RoleMapper.EditDtoToEntity(roleEntity, dto);

            var result =
                await _roleManager.UpdateAsync(roleEntity);

            if (!result.Succeeded)
            {
                return IdentityErrorResponse(result);
            }

            return new ResponseDto<RoleActionResponseDto>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = HttpMessageResponse.REGISTER_UPDATED,
                Data = RoleMapper.EntityToActionResponseDto(
                    roleEntity)
            };
        }

        public async Task<ResponseDto<RoleActionResponseDto>>
            DeleteAsync(string id)
        {
            var roleEntity =
                await _roleManager.FindByIdAsync(id);

            if (roleEntity is null)
            {
                return NotFoundResponse();
            }

            var result =
                await _roleManager.DeleteAsync(roleEntity);

            if (!result.Succeeded)
            {
                return IdentityErrorResponse(result);
            }

            return new ResponseDto<RoleActionResponseDto>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = HttpMessageResponse.REGISTER_DELETED,
                Data = RoleMapper.EntityToActionResponseDto(
                    roleEntity)
            };
        }

        private static ResponseDto<RoleActionResponseDto>
            NotFoundResponse()
        {
            return new ResponseDto<RoleActionResponseDto>
            {
                StatusCode = HttpStatusCode.NOT_FOUND,
                Status = false,
                Message = HttpMessageResponse.REGISTER_NOT_FOUND
            };
        }

        private static ResponseDto<RoleActionResponseDto>
            IdentityErrorResponse(IdentityResult result)
        {
            return new ResponseDto<RoleActionResponseDto>
            {
                StatusCode = HttpStatusCode.BAD_REQUEST,
                Status = false,
                Message = string.Join(
                    ", ",
                    result.Errors.Select(
                        error => error.Description))
            };
        }
    }
}
