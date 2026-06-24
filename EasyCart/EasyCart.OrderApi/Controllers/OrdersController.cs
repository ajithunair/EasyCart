using EasyCart.OrderApi.DTOs;
using EasyCart.OrderApi.DTOs.Conversions;
using EasyCart.OrderApi.Interfaces;
using EasyCart.OrderApi.Services;
using EasyCart.SharedLibrary.RabbitMQ.Events;
using EasyCart.SharedLibrary.Responses;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EasyCart.OrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController(IOrder orderInterface, IOrderService orderService, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orders = await orderInterface.GetAllAsync();
            if (orders.Any())
            {
                return Ok(orders.ToDtos());  
            }
            return NotFound();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await orderInterface.FindByIdAsync(id);
            if (order == null)
                return NotFound(null);
            return Ok(order.ToDto());

        }

        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<OrderDto>> GetClientOrders(int clientId)
        {
            if (clientId <= 0) return BadRequest("Invalid client Id");
            var orders = await orderService.GetOrdersByClientIdAsync(clientId);
            return orders.Any() ? Ok(orders) : NotFound(null);
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(int orderId)
        {
            if (orderId <= 0) return BadRequest("Invalid order Id");
            var orders = await orderService.GetOrderDetailsAsync(orderId);
            return orders.OrderId > 0 ? Ok(orders) : NotFound(null);
        }

        [HttpPost]
        public async Task<ActionResult<SharedLibrary.Responses.Response>> CreateOrder(OrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Incomplete data submitted");
            }

            var response = await orderInterface.CreateAsync(orderDto.ToEntity());

            if (response.Success)
            {
                //Create the event payload
                var orderEvent = new OrderPlacedEvent
                {
                    OrderId = orderDto.Id,
                    OrderDate = DateTime.UtcNow,
                    Items =
                    [
                        new OrderItemMessage
                        {
                            ProductId = orderDto.ProductId,
                            Quantity = orderDto.PurchaseQuantity
                        }
                    ]
                };
                //Publish the event to RabbitMQ
                await publishEndpoint.Publish(orderEvent);
            }

            return response.Success ? Ok(response) : BadRequest(response);

        }

        [HttpPut]
        public async Task<ActionResult<SharedLibrary.Responses.Response>> UpdateOrder(OrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Incomplete data submitted");
            }

            var response = await orderInterface.UpdateAsync(orderDto.ToEntity());
            return response.Success? Ok(response) : BadRequest(response);
        }

        [HttpDelete]
        public async Task<ActionResult<SharedLibrary.Responses.Response>> DeleteOrder(OrderDto orderDto)
        {
            var response = await orderInterface.DeleteAsync(orderDto.ToEntity());
            return response.Success? Ok(response) : BadRequest(response);
        }
    }
}
