using AutoCare_Club.Api.Constants;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.User;
using AutoCare_Club_Api.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club_Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(
            IUserService userService
        )
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult<ResponseDto<PageDto<List<UserDto>>>>> GetPageList(
            string searchTerm = "", int page = 1, int pageSize = 0)
        {
            var response = await _userService.GetPageAsync(searchTerm, page, pageSize);

            return StatusCode(response.StatusCode, new ResponseDto<PageDto<List<UserDto>>>
            {
                Status = response.Status,
                Message = response.Message,
                Data = response.Data
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult<ResponseDto<UserDto>>> GetOne(string id)
        {
            var response = await _userService.GetOneAsync(id);

            return StatusCode(response.StatusCode, new ResponseDto<UserDto>
            {
                Status = response.Status,
                Message = response.Message,
                Data = response.Data
            });
        }

        [HttpPost]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult<ResponseDto<UserActionResponseDto>>> Create(
            [FromBody] UserCreateDto dto)
        {
            var response = await _userService.CreateAsync(dto);

            return StatusCode(response.StatusCode, new ResponseDto<UserActionResponseDto>
            {
                Status = response.Status,
                Message = response.Message,
                Data = response.Data
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult<ResponseDto<UserActionResponseDto>>> Edit(
            string id, [FromBody] UserEditDto dto)
        {
            var response = await _userService.EditAsync(id, dto);

            return StatusCode(response.StatusCode, new ResponseDto<UserActionResponseDto>
            {
                Status = response.Status,
                Message = response.Message,
                Data = response.Data
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult<ResponseDto<UserActionResponseDto>>> Delete(
            string id)
        {
            var response = await _userService.DeleteAsync(id);

            return StatusCode(response.StatusCode, new ResponseDto<UserActionResponseDto>
            {
                Status = response.Status,
                Message = response.Message,
                Data = response.Data
            });
        }
    }


}
