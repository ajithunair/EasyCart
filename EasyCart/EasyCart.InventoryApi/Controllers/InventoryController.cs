using EasyCart.InventoryApi.Data;
using EasyCart.InventoryApi.DTOs;
using EasyCart.InventoryApi.DTOs.Conversions;
using EasyCart.InventoryApi.Entities;
using EasyCart.InventoryApi.Interfaces;
using EasyCart.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyCart.InventoryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class InventoryController(IInventory inventoryInterface, InventoryDbContext context) : Controller
    {
        [HttpGet("{productId:int}")]
        public async Task<ActionResult<InventoryDto>> GetInventory(int productId)
        {
            if (productId <= 0)
            {
                return BadRequest("Product id must be greater than zero.");
            }

            var inventory = await inventoryInterface.GetInventoryByProductId(productId);
            return inventory is null ? NotFound("Inventory not found.") : Ok(inventory.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateInventory([FromBody] InventoryCreateDto request)
        {
            var response = await inventoryInterface.CreateAsync(request.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Response>> UpdateInventory(int id, [FromBody] InventoryUpdateDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("Route id does not match the payload id.");
            }

            var response = await inventoryInterface.UpdateAsync(request.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Response>> DeleteInventory(int id)
        {
            // Inventory deletion only requires the identifier, so we keep the endpoint small and explicit.
            var response = await inventoryInterface.DeleteAsync(new Inventory { Id = id });
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("{productId:int}/increase-stock")]
        public async Task<ActionResult<Response>> IncreaseStock(int productId, [FromBody] StockIncreaseRequest request)
        {
            if (productId <= 0)
            {
                return BadRequest("Product id must be greater than zero.");
            }

            var inventory = await inventoryInterface.GetInventoryByProductId(productId);
            if (inventory is null)
            {
                return NotFound("Inventory not found.");
            }

            var adjustment = request.ToAdjustment();

            // Increase stock means we are explicitly adding units back into the available pool.
            inventory.AvailableQuantity += adjustment.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return Ok(new Response { Success = true, Message = "Stock increased successfully." });
        }

        [HttpPost("{productId:int}/decrease-stock")]
        public async Task<ActionResult<Response>> DecreaseStock(int productId, [FromBody] StockDecreaseRequest request)
        {
            if (productId <= 0)
            {
                return BadRequest("Product id must be greater than zero.");
            }

            var inventory = await inventoryInterface.GetInventoryByProductId(productId);
            if (inventory is null)
            {
                return NotFound("Inventory not found.");
            }

            var adjustment = request.ToAdjustment();

            if (inventory.AvailableQuantity < adjustment.Quantity)
            {
                return BadRequest("Insufficient stock.");
            }

            // Decrease stock is intentionally guarded so we never drive the available quantity below zero.
            inventory.AvailableQuantity -= adjustment.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return Ok(new Response { Success = true, Message = "Stock decreased successfully." });
        }
    }
}
