using EasyCart.OrderApi.DTOs;
using EasyCart.OrderApi.DTOs.Conversions;
using EasyCart.OrderApi.Entities;
using EasyCart.OrderApi.Interfaces;
using Polly;
using Polly.Registry;

namespace EasyCart.OrderApi.Services
{
    public class OrderService(IOrder orderInterface, HttpClient httpClient, ResiliencePipelineProvider<string> resiliencePipeline) : IOrderService
    {
        public async Task<Order?> BuildOrderAsync(OrderCreateDto request, int clientId)
        {
            var retryPipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");
            var products = new List<ProductDto>();

            foreach (var item in request.Items)
            {
                var product = await retryPipeline.ExecuteAsync(async token => await GetProductByIdAsync(item.ProductId));
                if (product is null || product.Price < 0)
                    return null;

                products.Add(product);
            }

            // Prices are resolved server-side and copied onto OrderItems; clients cannot set purchase prices.
            return request.ToEntity(clientId, products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var httpResponse = await httpClient.GetAsync($"api/products/{productId}");
            if (!httpResponse.IsSuccessStatusCode) return null!;
            return await httpResponse.Content.ReadFromJsonAsync<ProductDto>() ?? null!;
        }

        public async Task<AppUserDto> GetUser(int userId)
        {
            var httpResponse = await httpClient.GetAsync($"api/authentication/{userId}");
            if (!httpResponse.IsSuccessStatusCode) return null!;
            return await httpResponse.Content.ReadFromJsonAsync<AppUserDto>() ?? null!;
        }

        public async Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId)
        {
            var order = await orderInterface.FindByIdAsync(orderId);
            if (order is null) return null!;

            var retryPipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");
            var appUserDto = await retryPipeline.ExecuteAsync(async token => await GetUser(order.ClientId));
            if (appUserDto is null) return null!;

            var itemDetails = new List<OrderItemDetailsDto>();
            foreach (var item in order.Items)
            {
                var product = await retryPipeline.ExecuteAsync(async token => await GetProductByIdAsync(item.ProductId));
                if (product is null) return null!;

                itemDetails.Add(new OrderItemDetailsDto(
                    item.ProductId,
                    product.Name,
                    item.Quantity,
                    item.UnitPrice,
                    item.UnitPrice * item.Quantity));
            }

            return new OrderDetailsDto(
                order.Id,
                appUserDto.Id,
                appUserDto.Email,
                appUserDto.Address,
                appUserDto.PhoneNumber,
                itemDetails,
                itemDetails.Sum(item => item.TotalPrice),
                order.OrderDate);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByClientIdAsync(int clientId)
        {
            var orders = await orderInterface.GetOrdersAsync(o => o.ClientId == clientId);
            return orders.ToDtos();
        }
    }
}
