using AutoCare_Club.Api.Dtos.Vehicle;
using AutoCare_Club.Api.Services.Vehicle;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club.Api.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
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
            List<VehicleDto> vehicles =
                await _vehicleService.GetAllAsync();

            return Ok(vehicles);
        }

[HttpGet("{id:guid}")]
        public async Task<ActionResult<VehicleDto>> GetById(string id)
        {
            if (!Guid.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    message = "El identificador debe ser un GUID válido."
                });
            }

           VehicleDto? vehicle =
                await _vehicleService.GetByIdAsync(id);

            if (vehicle is null)
            {
                return NotFound(new
                {
                    message = "El vehiculo no fue encontrado."
                });
            }

            return Ok(vehicle);
        }
    }
}