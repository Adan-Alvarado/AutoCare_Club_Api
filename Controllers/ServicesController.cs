using AutoCare_Club.Api.Dtos.Services;
using AutoCare_Club.Api.Services.ServicesCatalog;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club.Api.Controllers
{
    [ApiController]
    [Route("api/services")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceCatalogService _serviceCatalog;

        public ServicesController(
            IServiceCatalogService serviceCatalog)
        {
            _serviceCatalog = serviceCatalog;
        }

        [HttpGet]
        public async Task<ActionResult<List<ServiceDto>>> GetAll()
        {
            List<ServiceDto> services =
                await _serviceCatalog.GetAllAsync();

            return Ok(services);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "El identificador debe ser mayor que cero."
                });
            }

            ServiceDto? service =
                await _serviceCatalog.GetByIdAsync(id);

            if (service is null)
            {
                return NotFound(new
                {
                    message = "El servicio no fue encontrado."
                });
            }

            return Ok(service);
        }
    }
}