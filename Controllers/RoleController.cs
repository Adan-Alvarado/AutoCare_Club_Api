using AutoCare_Club.Api.Constants;
using AutoCare_Club_Api.Dtos.Roles;
using AutoCare_Club_Api.Services.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club_Api.Controllers
{
    [ApiController]
    [Route("api/role")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class RoleController : ControllerBase
    {
      private readonly IRoleService _roleService;

        public RoleController(
            IRoleService roleService
        )
        {
            _roleService = roleService;
        }

        [HttpGet]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult> GetPage(
            string searchTerm = "", int page = 1, int pageSize = 10 
        )
        {
            var response = await _roleService.GetPageAsync(searchTerm, page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult> GetOne(string id)
        {
            var result = await _roleService.GetOneAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult> Create(RoleCreateDto dto)
        {
            var result = await _roleService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult> Update(string id, RoleEditDto dto)
        {
            var result = await _roleService.EditAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{RolesConstant.Admin}")]

        public async Task<ActionResult> Delete(string id)
        {
            var result = await _roleService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }   
    }
}