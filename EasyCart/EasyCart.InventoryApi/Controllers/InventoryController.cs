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
        [HttpGet("{productId}")]
        public async Task<Inventory> GetInventory(int productId)
        {
            var inventory = await inventoryInterface.GetInventoryByProductId(productId);
            return inventory;
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateInventory(InventoryDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await inventoryInterface.CreateAsync(request.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }


        [HttpPost("{productId}/add-stock")]
        public async Task<ActionResult<Response>> AddStock(int productId, StockRequest request)
        {
            var inventory = await GetInventory(productId);
            if (inventory == null)
            {
                return new Response { Success = false, Message = "Inventory not found." };
            }

            inventory.AvailableQuantity += request.Quantity;
            await context.SaveChangesAsync();
            return new Response { Success = true, Message = "Stock added successfully." };
        }

        [HttpPost("{productId}/reduce-stock")]
        public async Task<ActionResult<Response>> ReduceStock(int productId,StockRequest request)
        {
            var inventory = await GetInventory(productId);
            if (inventory == null)
            {
                return new Response { Success = false, Message = "Inventory not found." };
            }
            if (inventory.AvailableQuantity < request.Quantity)
            {
                return BadRequest("Insufficient stock");
            }
            inventory.AvailableQuantity -= request.Quantity;
            await context.SaveChangesAsync();
            return new Response { Success = true, Message = "Stock updated successfully." };
        }

        //[HttpGet("low-stock")]
        //public async Task<ActionResult<Response>> GetLowStock()
        //{
        //    return null;
        //}
    }
}

