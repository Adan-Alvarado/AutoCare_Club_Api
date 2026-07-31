using System.Security.Claims;
using AutoCare_Club.Api.Dtos.Orders;
using AutoCare_Club.Api.Services.Orders;
using AutoCare_Club_Api.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoCare_Club.Api.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CartController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public CartController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDto<OrderDto>>> Get()
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<OrderDto>();
            }

            return ToActionResult(
                await _orderService.GetCartAsync(userId));
        }

        [HttpPost("items")]
        public async Task<ActionResult<ResponseDto<OrderDto>>> AddItem(
            CartItemCreateDto dto)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<OrderDto>();
            }

            return ToActionResult(
                await _orderService.AddCartItemAsync(userId, dto));
        }

        [HttpPut("items/{id:guid}")]
        public async Task<ActionResult<ResponseDto<OrderDto>>> EditItem(
            string id,
            CartItemEditDto dto)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return UnauthorizedResponse<OrderDto>();
            }

            return ToActionResult(
                await _orderService.EditCartItemAsync(
                    userId,
                    id,
                    dto));
        }

        [HttpDelete("items/{id:guid}")]
        public async Task<IActionResult> DeleteItem(string id)
        {
            string? userId = GetAuthenticatedUserId();

            if (userId is null)
            {
                return Unauthorized(new
                {
                    message = "El token no contiene un usuario válido."
                });
            }

            ResponseDto<bool> response =
                await _orderService.DeleteCartItemAsync(userId, id);

            if (response.Status)
            {
                return NoContent();
            }

            return StatusCode(response.StatusCode, new ResponseDto<bool>
            {
                Status = false,
                Message = response.Message
            });
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<ResponseDto<OrderDto>>> Checkout(
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
