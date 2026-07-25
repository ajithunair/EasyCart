using EasyCart.InventoryApi.Data;
using EasyCart.InventoryApi.Entities;
using EasyCart.InventoryApi.Interfaces;
using EasyCart.SharedLibrary.Logs;
using EasyCart.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EasyCart.InventoryApi.Repositories
{
    public class InventoryRepository(InventoryDbContext context) : IInventory
    {
        public async Task<Response> CreateAsync(Inventory entity)
        {
            try
            {
                var inventory = await context.Inventories.AddAsync(entity);
                await context.SaveChangesAsync();
                return new Response { Success = true, Message = $"Inventory created successfully." };
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "An error occurred while creating the Inventory." };
            }
        }

        public async Task<Response> DeleteAsync(Inventory entity)
        {
            try
            {
                var inventory = await context.Inventories.FindAsync(entity.Id);
                if (inventory == null)
                {
                    return new Response { Success = false, Message = "Inventory not found." };
                }
                else
                {
                    context.Inventories.Remove(inventory);
                    await context.SaveChangesAsync();
                    return new Response { Success = true, Message = $"Inventory deleted successfully." };
                }
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "An error occurred while deleting the product." };
            }
        }

        public async Task<Inventory?> FindByIdAsync(int id)
        {
            try
            {
                var inventory = await context.Inventories.FindAsync(id);
                return inventory;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the Inventory by ID.", ex);
            }
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            try
            {
                var inventories = await context.Inventories.AsNoTracking().ToListAsync();
                return inventories;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving all inventories.", ex);
            }
        }

        public async Task<Inventory?> GetByAsync(Expression<Func<Inventory, bool>> predicate)
        {
            try
            {
                var inventory = await context.Inventories.FirstOrDefaultAsync(predicate);
                return inventory;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the Inventory.", ex);
            }
        }

        public async Task<Response> UpdateAsync(Inventory entity)
        {
            try
            {
                var inventory = await FindByIdAsync(entity.Id);
                if (inventory == null)
                {
                    return new Response { Success = false, Message = "Inventory not found." };
                }

                // Update the product properties
                inventory.ProductId = entity.ProductId;
                inventory.AvailableQuantity = entity.AvailableQuantity;
                inventory.ReorderLevel = entity.ReorderLevel;
                inventory.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return new Response { Success = true, Message = $"Inventory updated successfully." };
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "An error occurred while updating the Inventory." };
            }
        }

        public async Task<Inventory?> GetInventoryByProductId(int productId)
        {
            try
            {
                var inventory = await context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
                return inventory;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the Inventory.", ex);
            }
        }
    }
}
