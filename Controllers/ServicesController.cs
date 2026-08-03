using AutoCare_Club.Api.Dtos.Services;
using AutoCare_Club.Api.Services.ServicesCatalog;
using AutoCare_Club.Api.Constants;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ServiceDto>> GetById(string id)
        {
            if (!Guid.TryParse(id, out _))
            {
                return BadRequest(new
                {
                    message = "El identificador debe ser un GUID válido."
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

        [HttpPost]
        [Authorize(
            AuthenticationSchemes = "Bearer",
            Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ServiceDto>> Create(
            ServiceCreateDto dto)
        {
            ServiceDto service =
                await _serviceCatalog.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = service.Id },
                service);
        }

        [HttpPut("{id:guid}")]
        [Authorize(
            AuthenticationSchemes = "Bearer",
            Roles = RolesConstant.Admin)]
        public async Task<ActionResult<ServiceDto>> Edit(
            string id,
            ServiceEditDto dto)
        {
            ServiceDto? service =
                await _serviceCatalog.EditAsync(id, dto);

            if (service is null)
            {
                return NotFound(new
                {
                    message = "El servicio no fue encontrado."
                });
            }

            return Ok(service);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(
            AuthenticationSchemes = "Bearer",
            Roles = RolesConstant.Admin)]
        public async Task<IActionResult> Delete(string id)
        {
            bool deleted =
                await _serviceCatalog.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "El servicio no fue encontrado."
                });
            }

            return NoContent();
        }
    }
}
