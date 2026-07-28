using System.Security.Claims;
using AutoCare_Club.Api.Dtos.Vehicle;
using AutoCare_Club.Api.Services.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club.Api.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(
            IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<VehicleDto>>> GetAll()
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return Unauthorized(new
                {
                    message = "El token no contiene un usuario que sea valido."
                });
            }

            List<VehicleDto> vehicles =
                await _vehicleService.GetAllAsync(userId);

            return Ok(vehicles);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<VehicleDto>> GetById(string id)
        {
            if (!Guid.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    message = "El identificador debe ser un GUID valido."
                });
            }

            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return Unauthorized(new
                {
                    message = "El token no contiene un usuario valido."
                });
            }

            VehicleDto? vehicle =
                await _vehicleService.GetByIdAsync(id, userId);

            if (vehicle is null)
            {
                return NotFound(new
                {
                    message = "El vehiculo no fue encontrado."
                });
            }

            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<ActionResult<VehicleDto>> Create(
            VehicleCreateDto dto)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return Unauthorized(new
                {
                    message = "El token no contiene un usuario valido."
                });
            }

            VehicleDto vehicle =
                await _vehicleService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = vehicle.Id },
                vehicle);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<VehicleDto>> Edit(
            string id,
            VehicleEditDto dto)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return Unauthorized(new
                {
                    message = "El token no contiene un usuario valido."
                });
            }

            VehicleDto? vehicle =
                await _vehicleService.EditAsync(id, userId, dto);

            if (vehicle is null)
            {
                return NotFound(new
                {
                    message = "El vehiculo no fue encontrado."
                });
            }

            return Ok(vehicle);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(string id)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return Unauthorized(new
                {
                    message = "El token no contiene un usuario valido."
                });
            }

            bool wasDeleted =
                await _vehicleService.DeleteAsync(id, userId);

            if (!wasDeleted)
            {
                return NotFound(new
                {
                    message = "El vehiculo no fue encontrado."
                });
            }

            return NoContent();
        }

        private string? GetAuthenticatedUserId()
        {
            string? userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("UserId");

            return Guid.TryParse(userId, out _)
                ? userId
                : null;
        }
    }
}
