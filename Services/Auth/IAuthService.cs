using AutoCare_Club_Api.Dtos.Auth;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.User;

namespace AutoCare_Club_Api.Services.Auth
{
    public interface IAuthService
    {  
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ResponseDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
        Task<ResponseDto<UserActionResponseDto>> RegisterAsync(RegisterDto dto);
    }
}