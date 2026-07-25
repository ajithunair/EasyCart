using EasyCart.OrderApi.DTOs;
using EasyCart.OrderApi.Entities;

namespace EasyCart.OrderApi.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetOrdersByClientIdAsync(int clientId);
        Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId);
        Task<Order?> BuildOrderAsync(OrderCreateDto request, int clientId);
    }
}
