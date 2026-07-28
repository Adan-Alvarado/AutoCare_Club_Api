using AutoCare_Club_Api.Dtos.Auth;
using AutoCare_Club_Api.Dtos.Common;

namespace AutoCare_Club_Api.Services.Auth
{
    public interface IAuthService
    {  
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ResponseDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    }
}