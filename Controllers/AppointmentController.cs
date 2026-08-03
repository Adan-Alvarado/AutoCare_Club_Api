using System.Security.Claims;
using AutoCare_Club.Api.Constants;
using AutoCare_Club_Api.Dtos.Appointments;
using AutoCare_Club_Api.Dtos.Common;
using AutoCare_Club_Api.Services.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club_Api.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService
                _appointmentService;

        public AppointmentController(
            IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<
            ResponseDto<List<AppointmentDto>>>>
            GetAll()
        {
            return ToActionResult(
                await _appointmentService.GetAllAsync());
        }

        [HttpGet("me")]
        public async Task<ActionResult<
            ResponseDto<List<AppointmentDto>>>>
            GetMine()
        {
            var userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<
                    List<AppointmentDto>>();
            }

            return ToActionResult(
                await _appointmentService.GetMineAsync(
                    userId));
        }

        [HttpGet("technician/me")]
        [Authorize(Roles = RolesConstant.Technician)]
        public async Task<ActionResult<
            ResponseDto<List<AppointmentDto>>>>
            GetTechnicianAppointments()
        {
            var technicianId = GetAuthenticatedUserId();

            if (technicianId is null)
            {
                return UnauthorizedResponse<
                    List<AppointmentDto>>();
            }

            return ToActionResult(
                await _appointmentService
                    .GetTechnicianAppointmentsAsync(
                        technicianId));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<
            ResponseDto<AppointmentDto>>>
            GetOne(string id)
        {
            var userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<AppointmentDto>();
            }

            var canManage =
                User.IsInRole(RolesConstant.Admin);

            return ToActionResult(
                await _appointmentService.GetOneAsync(
                    id,
                    userId,
                    canManage));
        }

        [HttpPost]
        [Authorize(
            Roles = $"{RolesConstant.Customer}," +
                    $"{RolesConstant.Admin}")]
        public async Task<ActionResult<
            ResponseDto<AppointmentActionResponseDto>>>
            Create(AppointmentCreateDto dto)
        {
            var userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<
                    AppointmentActionResponseDto>();
            }

            return ToActionResult(
                await _appointmentService.CreateAsync(
                    userId,
                    dto));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<
            ResponseDto<AppointmentActionResponseDto>>>
            Edit(
                string id,
                AppointmentEditDto dto)
        {
            return ToActionResult(
                await _appointmentService.EditAsync(
                    id,
                    dto));
        }

        [HttpPatch("{id:guid}/cancel")]
        [Authorize(
            Roles = $"{RolesConstant.Customer}," +
                    $"{RolesConstant.Admin}")]
        public async Task<ActionResult<
            ResponseDto<AppointmentActionResponseDto>>>
            Cancel(string id)
        {
            var userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<
                    AppointmentActionResponseDto>();
            }

            var canManage =
                User.IsInRole(RolesConstant.Admin);

            return ToActionResult(
                await _appointmentService.CancelAsync(
                    id,
                    userId,
                    canManage));
        }

        [HttpPatch("{id:guid}/technician-status")]
        [Authorize(Roles = RolesConstant.Technician)]
        public async Task<ActionResult<
            ResponseDto<AppointmentActionResponseDto>>>
            UpdateTechnicianStatus(
                string id,
                AppointmentStatusEditDto dto)
        {
            var technicianId = GetAuthenticatedUserId();

            if (technicianId is null)
            {
                return UnauthorizedResponse<
                    AppointmentActionResponseDto>();
            }

            return ToActionResult(
                await _appointmentService
                    .UpdateStatusByTechnicianAsync(
                        id,
                        technicianId,
                        dto));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = RolesConstant.Admin)]
        public async Task<ActionResult<
            ResponseDto<AppointmentActionResponseDto>>>
            Delete(string id)
        {
            return ToActionResult(
                await _appointmentService.DeleteAsync(id));
        }

        private string? GetAuthenticatedUserId()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("UserId");

            return Guid.TryParse(userId, out _)
                ? userId
                : null;
        }

        private ActionResult<ResponseDto<T>>
            ToActionResult<T>(
                ResponseDto<T> response)
        {
            return StatusCode(
                response.StatusCode,
                response);
        }

        private ActionResult<ResponseDto<T>>
            UnauthorizedResponse<T>()
        {
            var response = new ResponseDto<T>
            {
                StatusCode = HttpStatusCode.UNAUTHORIZED,
                Status = false,
                Message =
                    "El token no contiene un usuario válido."
            };

            return StatusCode(
                response.StatusCode,
                response);
        }
    }
}
