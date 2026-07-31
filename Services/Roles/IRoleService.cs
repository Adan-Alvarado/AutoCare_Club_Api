using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.Roles;

namespace AutoCare_Club_Api.Services.Roles
{
    public interface IRoleService
    {
        Task<ResponseDto<PageDto<List<RoleDto>>>> GetPageAsync(
            string searchTerm = "", int page = 1, int pageSize = 10);

        Task<ResponseDto<RoleDto>> GetOneAsync(string id);
        Task<ResponseDto<RoleActionResponseDto>> CreateAsync(RoleCreateDto dto);
        Task<ResponseDto<RoleActionResponseDto>> EditAsync(string id, RoleEditDto dto);
        Task<ResponseDto<RoleActionResponseDto>> DeleteAsync(string id);
    }
}