using AutoCare_Club.Api.Dtos.Orders;
using AutoCare_Club_Api.Dtos.Common;

namespace AutoCare_Club.Api.Services.Orders
{
    public interface IOrderService
    {
        Task<ResponseDto<OrderDto>> GetCartAsync(string userId);

        Task<ResponseDto<OrderDto>> AddCartItemAsync(
            string userId,
            CartItemCreateDto dto);

        Task<ResponseDto<OrderDto>> EditCartItemAsync(
            string userId,
            string itemId,
            CartItemEditDto dto);

        Task<ResponseDto<bool>> DeleteCartItemAsync(
            string userId,
            string itemId);

        Task<ResponseDto<OrderDto>> CheckoutAsync(
            string userId,
            CartCheckoutDto dto);

        Task<ResponseDto<OrderDto>> GetOrderAsync(
            string userId,
            string orderId);

        Task<ResponseDto<List<OrderDto>>> GetCustomerOrdersAsync(
            string userId);
    }
}
