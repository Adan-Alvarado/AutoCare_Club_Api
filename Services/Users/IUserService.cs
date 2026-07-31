using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.User;

namespace AutoCare_Club_Api.Services.Users
{
    public interface IUserService
    {
        Task<ResponseDto<PageDto<List<UserDto>>>> GetPageAsync(
            string searchTerm = "", int page = 1, int pageSize = 10);

        Task<ResponseDto<UserDto>> GetOneAsync(string id);
        Task<ResponseDto<UserActionResponseDto>> CreateAsync(UserCreateDto dto);
        Task<ResponseDto<UserActionResponseDto>> EditAsync(string id, UserEditDto dto);
        Task<ResponseDto<UserActionResponseDto>> DeleteAsync(string id);
        
    }
}