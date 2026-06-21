using EasyCart.OrderApi.DTOs;

namespace EasyCart.OrderApi.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersByClientIdAsync(int clientId);
        Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId);
    }
}
