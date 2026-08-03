using AutoCare_Club.Api.Constants;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.Technicians;
using AutoCare_Club_Api.Services.Technicians;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club_Api.Controllers
{
    [ApiController]
    [Route("api/technicians")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class TechnicianController : ControllerBase
    {
        private readonly ITechnicianService
            _technicianService;

        public TechnicianController(
            ITechnicianService technicianService)
        {
            _technicianService = technicianService;
        }

        [HttpGet]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<PageDto<List<TechnicianDto>>>>> GetPage(
                string searchTerm = "",
                int page = 1,
                int pageSize = 10,
                bool includeInactive = false)
        {
            var response =
                await _technicianService.GetPageAsync(
                    searchTerm,
                    page,
                    pageSize,
                    includeInactive);

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpGet("{userId:guid}")]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<TechnicianDto>>> GetOne(string userId)
        {
            var response =
                await _technicianService.GetOneAsync(
                    userId);

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpPost]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<TechnicianActionResponseDto>>> Create(TechnicianCreateDto dto)
        {
            var response =
                await _technicianService.CreateAsync(dto);

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpPut("{userId:guid}")]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<TechnicianActionResponseDto>>> Edit(
                string userId,
                TechnicianEditDto dto)
        {
            var response =
                await _technicianService.EditAsync(
                    userId,
                    dto);

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpDelete("{userId:guid}")]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<TechnicianActionResponseDto>>> Delete(string userId)
        {
            var response =
                await _technicianService.DeleteAsync(
                    userId);

            return StatusCode(
                response.StatusCode,
                response);
        }
    }
}