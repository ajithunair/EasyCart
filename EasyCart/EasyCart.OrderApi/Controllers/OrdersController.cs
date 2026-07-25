using EasyCart.OrderApi.DTOs;
using EasyCart.OrderApi.DTOs.Conversions;
using EasyCart.OrderApi.Entities;
using EasyCart.OrderApi.Interfaces;
using EasyCart.OrderApi.Services;
using EasyCart.SharedLibrary.RabbitMQ.Events;
using EasyCart.SharedLibrary.Responses;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            var orders = User.IsInRole("Admin")
                ? await orderInterface.GetAllAsync()
                : await orderInterface.GetOrdersAsync(o => o.ClientId == GetAuthenticatedUserId());
            return orders.Any() ? Ok(orders.ToDtos()) : NotFound("No orders found.");
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await orderInterface.FindByIdAsync(id);
            if (order is not null && !CanAccess(order.ClientId))
            {
                return Forbid();
            }

            return order is null ? NotFound("Order not found.") : Ok(order.ToDto());
        }

        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<OrderDto>> GetClientOrders(int clientId)
        {
            if (clientId <= 0)
            {
                return BadRequest("Client id must be greater than zero.");
            }

            if (!User.IsInRole("Admin") && clientId != GetAuthenticatedUserId())
            {
                return Forbid();
            }

            var orders = await orderService.GetOrdersByClientIdAsync(clientId);
            return orders.Any() ? Ok(orders) : NotFound("No orders found for the requested client.");
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(int orderId)
        {
            if (orderId <= 0)
            {
                return BadRequest("Order id must be greater than zero.");
            }

            var order = await orderInterface.FindByIdAsync(orderId);
            if (order is null)
            {
                return NotFound("Order details not found.");
            }

            if (!CanAccess(order.ClientId))
            {
                return Forbid();
            }

            var details = await orderService.GetOrderDetailsAsync(orderId);
            return details.OrderId > 0 ? Ok(details) : NotFound("Order details not found.");
        }

        [HttpPost]
        public async Task<ActionResult<EasyCart.SharedLibrary.Responses.Response>> CreateOrder([FromBody] OrderCreateDto orderDto)
        {
            var orderEntity = await orderService.BuildOrderAsync(orderDto, GetAuthenticatedUserId());
            if (orderEntity is null)
            {
                return BadRequest("One or more products could not be found.");
            }

            var response = await orderInterface.CreateAsync(orderEntity);

            if (response.Success)
            {
                // Publish only after the order is saved so downstream consumers receive a real persisted id.
                var orderEvent = new OrderPlacedEvent
                {
                    OrderId = orderEntity.Id,
                    OrderDate = orderEntity.OrderDate,
                    Items =
                    [
                        .. orderEntity.Items.Select(item => new OrderItemMessage
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        })
                    ]
                };

                var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

                // Keep the trace chain intact across the message bus.
                await publishEndpoint.Publish(orderEvent, context =>
                {
                    context.Headers.Set("CorrelationId", correlationId);
                });
            }

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EasyCart.SharedLibrary.Responses.Response>> UpdateOrder(int id, [FromBody] OrderUpdateDto orderDto)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (id != orderDto.Id)
            {
                return BadRequest("Route id does not match the payload id.");
            }

            var response = await orderInterface.UpdateAsync(orderDto.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<EasyCart.SharedLibrary.Responses.Response>> DeleteOrder(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Deletion only needs the identifier, so callers do not have to submit the full order payload.
            var response = await orderInterface.DeleteAsync(new Order { Id = id });
            return response.Success ? Ok(response) : BadRequest(response);
        }

        private int GetAuthenticatedUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userId, out var parsedUserId) && parsedUserId > 0
                ? parsedUserId
                : throw new InvalidOperationException("Authenticated user id is missing from the token.");
        }

        private bool CanAccess(int clientId) => User.IsInRole("Admin") || clientId == GetAuthenticatedUserId();
    }
}
