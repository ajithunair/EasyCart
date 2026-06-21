using EasyCart.OrderApi.DTOs;
using EasyCart.OrderApi.DTOs.Conversions;
using EasyCart.OrderApi.Interfaces;
using Polly;
using Polly.Registry;

namespace EasyCart.OrderApi.Services
{
    public class OrderService(IOrder orderInterface, HttpClient httpClient, ResiliencePipelineProvider<string> resiliencePipeline) : IOrderService
    {
        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var httpResponse = await httpClient.GetAsync($"api/products/{productId}");
            if (!httpResponse.IsSuccessStatusCode)
            {
                return null!;
            }
            var product = await httpResponse.Content.ReadFromJsonAsync<ProductDto>();
            return product!;
        }

        public async Task<AppUserDto> GetUser(int userId)
        {
            var httpResponse = await httpClient.GetAsync($"api/authentication/{userId}");
            if (!httpResponse.IsSuccessStatusCode)
            {
                return null!;
            }
            var user = await httpResponse.Content.ReadFromJsonAsync<AppUserDto>();
            return user!;
        }
        public async Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId)
        {
            //Prepare order
            var order = await orderInterface.FindByIdAsync(orderId);
            if (order == null)
            {
                return null!;
            }

            //Get retry pipeline
            var retryPipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");

            //prepare product and user tasks
            var productDto = await retryPipeline.ExecuteAsync(async token => await GetProductByIdAsync(order.ProductId));
            var appUserDto = await retryPipeline.ExecuteAsync(async token => await GetUser(order.ClientId));

            if(productDto is null)
                return null!;

            if (appUserDto is null)
                return null!;


            //populate order details
            var orderDetails = new OrderDetailsDto(
            order.Id,
            productDto.Id,
            order.PurchaseQuantity,
            appUserDto.Id,
            appUserDto.Email,
            appUserDto.Address,
            appUserDto.PhoneNumber,
            productDto.Name,
            order.PurchaseQuantity,
            productDto.Price,
            productDto.Price * order.PurchaseQuantity,
            order.OrderDate
            );

            return orderDetails;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByClientIdAsync(int clientId)
        {
            var orders = await orderInterface.GetOrdersAsync(o => o.ClientId == clientId);
            if(orders == null || !orders.Any())
            {
                return Enumerable.Empty<OrderDto>();
            }

            return orders.ToDtos();
        }
    }
}

