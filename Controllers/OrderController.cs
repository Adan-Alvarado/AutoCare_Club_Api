using System.Security.Claims;
using AutoCare_Club.Api.Dtos.Orders;
using AutoCare_Club.Api.Services.Orders;
using AutoCare_Club_Api.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDto<OrderDto>>> Create(
            CartCheckoutDto dto)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<OrderDto>();
            }

            return ToActionResult(
                await _orderService.CheckoutAsync(userId, dto));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseDto<OrderDto>>> GetById(
            string id)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<OrderDto>();
            }

            return ToActionResult(
                await _orderService.GetOrderAsync(userId, id));
        }

        [HttpGet("~/api/customer/orders")]
        public async Task<ActionResult<ResponseDto<List<OrderDto>>>>
            GetCustomerOrders()
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<List<OrderDto>>();
            }

            return ToActionResult(
                await _orderService.GetCustomerOrdersAsync(userId));
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

        private ActionResult<ResponseDto<T>> ToActionResult<T>(
            ResponseDto<T> response)
        {
            return StatusCode(response.StatusCode, new ResponseDto<T>
            {
                Status = response.Status,
                Message = response.Message,
                Data = response.Data
            });
        }

        private UnauthorizedObjectResult UnauthorizedResponse<T>()
        {
            return Unauthorized(new ResponseDto<T>
            {
                Status = false,
                Message = "El token no contiene un usuario válido."
            });
        }
    }
}
