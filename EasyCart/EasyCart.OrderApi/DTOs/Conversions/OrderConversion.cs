using EasyCart.OrderApi.DTOs;
using EasyCart.OrderApi.Entities;

namespace EasyCart.OrderApi.DTOs.Conversions
{
    public static class OrderConversion
    {
        public static Order ToEntity(this OrderDto orderDto) {
            return new Order
            {
                Id = orderDto.Id,
                ProductId = orderDto.ProductId,
                ClientId = orderDto.ClientId,
                PurchaseQuantity = orderDto.PurchaseQuantity,
                OrderDate = orderDto.OrderDate
            };
        }

        public static IEnumerable<OrderDto> ToDtos(this IEnumerable<Order> orders)
        {
            return orders.Select(ToDto);
        }

        public static OrderDto ToDto(this Order order)
        {
            return new OrderDto
            (
                Id: order.Id,
                ProductId: order.ProductId,
                PurchaseQuantity: order.PurchaseQuantity,
                ClientId: order.ClientId,
                OrderDate: order.OrderDate
            );
        }
    }
}
