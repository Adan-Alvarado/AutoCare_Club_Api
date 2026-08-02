using AutoCare_Club.Api.Constants;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Dtos.Schedules;
using AutoCare_Club_Api.Services.Schedules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club_Api.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(
            IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<List<ScheduleDto>>>> GetAll()
        {
            var response =
                await _scheduleService.GetAllAsync();

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<ScheduleDto>>> GetOne(
                string id)
        {
            var response =
                await _scheduleService.GetOneAsync(id);

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpPost]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<ScheduleActionResponseDto>>> Create(ScheduleCreateDto dto)
        {
            var response =
                await _scheduleService.CreateAsync(dto);

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = RolesConstant.Admin)]

        public async Task<ActionResult<ResponseDto<ScheduleActionResponseDto>>>Edit(string id, ScheduleEditDto dto)
        {
            var response =
                await _scheduleService.EditAsync(
                    id,
                    dto);

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ResponseDto<ScheduleActionResponseDto>>> Delete(string id)
        {
            var response =
                await _scheduleService.DeleteAsync(id);

            return StatusCode(
                response.StatusCode,
                response);
        }

        [HttpGet("available")]
        [Authorize(
            Roles =
                $"{RolesConstant.Customer}," +
                $"{RolesConstant.Admin}")]

        public async Task<ActionResult<ResponseDto<List<ScheduleAvailabilityDto>>>> GetAvailable(
                [FromQuery] string serviceId,
                [FromQuery] DateOnly date)
        {
            var response =
                await _scheduleService.GetAvailableAsync(
                    serviceId,
                    date);

            return StatusCode(
                response.StatusCode,
                response);
        }
    }
}
