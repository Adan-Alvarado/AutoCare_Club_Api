using AutoCare_Club_Api.Dtos.Auth;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.User;
using AutoCare_Club_Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club_Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize(AuthenticationSchemes = "Bearer")]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService
        )
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<LoginResponseDto>>> Login(LoginDto dto)
        {
            var response = await _authService.LoginAsync(dto);

            return StatusCode(response.StatusCode, new ResponseDto<LoginResponseDto>
            {
                Status = response.Status,
                Message = response.Message,
                Data = response.Data
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<UserActionResponseDto>>> Register(RegisterDto dto)
        {
            var response = await _authService.RegisterAsync(dto);

            return StatusCode(response.StatusCode, response);
        }
    }
}