using EasyCart.OrderApi.Data;
using EasyCart.OrderApi.DTOs;
using EasyCart.OrderApi.DTOs.Conversions;
using EasyCart.OrderApi.Entities;
using EasyCart.OrderApi.Services;
using EasyCart.SharedLibrary.RabbitMQ.Events;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EasyCart.OrderApi.Controllers
{
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartController(
        OrderDbContext context,
        IOrderService orderService,
        IPublishEndpoint publishEndpoint) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var clientId = GetAuthenticatedUserId();
            var cart = await context.Carts
                .Include(existing => existing.Items)
                .AsNoTracking()
                .SingleOrDefaultAsync(existing => existing.ClientId == clientId);

            return Ok(ToDto(cart ?? new Cart { ClientId = clientId }));
        }

        [HttpPost("items")]
        public async Task<ActionResult<CartDto>> AddItem(CartItemRequestDto request)
        {
            var product = await orderService.GetProductByIdAsync(request.ProductId);
            if (product is null)
                return NotFound("Product not found.");

            var cart = await GetOrCreateCartAsync();
            var item = cart.Items.SingleOrDefault(existing => existing.ProductId == request.ProductId);
            if (item is null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                });
            }
            else
            {
                // Adding an existing product accumulates quantity instead of creating duplicate cart lines.
                item.Quantity += request.Quantity;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok(ToDto(cart));
        }

        [HttpPut("items/{productId:int}")]
        public async Task<ActionResult<CartDto>> UpdateItem(int productId, CartItemRequestDto request)
        {
            if (productId != request.ProductId)
                return BadRequest("Route product id does not match the payload.");

            var cart = await GetCartForUpdateAsync();
            var item = cart?.Items.SingleOrDefault(existing => existing.ProductId == productId);
            if (item is null)
                return NotFound("Cart item not found.");

            item.Quantity = request.Quantity;
            cart!.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok(ToDto(cart));
        }

        [HttpDelete("items/{productId:int}")]
        public async Task<ActionResult<CartDto>> RemoveItem(int productId)
        {
            var cart = await GetCartForUpdateAsync();
            var item = cart?.Items.SingleOrDefault(existing => existing.ProductId == productId);
            if (item is null)
                return NotFound("Cart item not found.");

            context.CartItems.Remove(item);
            cart!.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok(ToDto(cart));
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var cart = await GetCartForUpdateAsync();
            if (cart is null)
                return NoContent();

            context.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<OrderDto>> Checkout(CheckoutDto checkoutDto)
        {
            if (!string.Equals(checkoutDto.PaymentMethod, "CashOnDelivery", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only CashOnDelivery is currently supported.");
            }

            var clientId = GetAuthenticatedUserId();
            var cart = await GetCartForUpdateAsync();
            if (cart is null || cart.Items.Count == 0)
                return BadRequest("Cannot checkout an empty cart.");

            var request = new OrderCreateDto(cart.Items
                .Select(item => new OrderItemCreateDto(item.ProductId, item.Quantity))
                .ToList());
            var order = await orderService.BuildOrderAsync(request, clientId);
            if (order is null)
                return BadRequest("One or more products could not be found.");

            // Payment remains pending for COD and the order remains pending until inventory confirms reservation.
            order.PaymentMethod = "CashOnDelivery";
            order.PaymentStatus = PaymentStatuses.Pending;
            order.ShippingAddress = checkoutDto.ShippingAddress;
            order.ShippingCity = checkoutDto.ShippingCity;
            order.ShippingPostalCode = checkoutDto.ShippingPostalCode;
            order.ShippingPhone = checkoutDto.ShippingPhone;

            // Npgsql uses a retrying execution strategy, so user transactions must run inside that strategy.
            var executionStrategy = context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                // Save the order and clear its source cart in one transaction so checkout cannot partially complete.
                context.Orders.Add(order);
                context.CartItems.RemoveRange(cart.Items);
                cart.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            });

            // Publish only after commit so inventory never receives an event for a rolled-back order.
            await publishEndpoint.Publish(new OrderPlacedEvent
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                Items = order.Items.Select(item => new OrderItemMessage
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            });

            return CreatedAtAction(nameof(OrdersController.GetOrder), "Orders", new { id = order.Id }, order.ToDto());
        }

        private async Task<Cart> GetOrCreateCartAsync()
        {
            var cart = await GetCartForUpdateAsync();
            if (cart is not null)
                return cart;

            cart = new Cart { ClientId = GetAuthenticatedUserId() };
            context.Carts.Add(cart);
            return cart;
        }

        private Task<Cart?> GetCartForUpdateAsync() => context.Carts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(cart => cart.ClientId == GetAuthenticatedUserId());

        private int GetAuthenticatedUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userId, out var parsedUserId) && parsedUserId > 0
                ? parsedUserId
                : throw new InvalidOperationException("Authenticated user id is missing from the token.");
        }

        private static CartDto ToDto(Cart cart) => new(
            cart.Id,
            cart.ClientId,
            cart.UpdatedAt,
            cart.Items.Select(item => new CartItemDto(item.ProductId, item.Quantity)).ToList());
    }
}
