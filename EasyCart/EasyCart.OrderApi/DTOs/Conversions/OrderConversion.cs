using EasyCart.OrderApi.DTOs;
using EasyCart.OrderApi.Entities;

namespace EasyCart.OrderApi.DTOs.Conversions
{
    public static class OrderConversion
    {
        public static Order ToEntity(this OrderCreateDto orderDto, int clientId, IEnumerable<ProductDto> products)
        {
            var productPrices = products.ToDictionary(product => product.Id, product => product.Price);

            return new Order
            {
                ClientId = clientId,
                OrderDate = DateTime.UtcNow,
                Items = orderDto.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = productPrices[item.ProductId]
                }).ToList()
            };
        }

        public static Order ToEntity(this OrderUpdateDto orderDto)
        {
            return new Order
            {
                Id = orderDto.Id,
                ClientId = orderDto.ClientId,
                OrderDate = orderDto.OrderDate,
                Items = orderDto.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };
        }

        public static IEnumerable<OrderDto> ToDtos(this IEnumerable<Order> orders) => orders.Select(ToDto);

        public static OrderDto ToDto(this Order order) => new(
            order.Id,
            order.ClientId,
            order.OrderDate,
            order.Status,
            order.PaymentMethod,
            order.PaymentStatus,
            order.ShippingStatus,
            order.ShippingAddress,
            order.ShippingCity,
            order.ShippingPostalCode,
            order.ShippingPhone,
            order.TrackingNumber,
            order.Items.Select(item => new OrderItemDto(
                item.Id,
                item.ProductId,
                item.Quantity,
                item.UnitPrice)).ToList());
    }
}
