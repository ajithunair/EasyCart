using EasyCart.OrderApi.Data;
using EasyCart.OrderApi.Entities;
using EasyCart.OrderApi.Interfaces;
using EasyCart.SharedLibrary.Logs;
using EasyCart.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EasyCart.OrderApi.Repositories
{
    public class OrderRepository(OrderDbContext context) : IOrder
    {
        public async Task<Response> CreateAsync(Order entity)
        {
            try
            {
                var result = context.Orders.Add(entity).Entity;
                await context.SaveChangesAsync();
                return result.Id > 0
                    ? new Response(true, "Order created successfully")
                    : new Response(false, "Failed to create order");
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response(false, "An error occurred while creating the order.");
            }
        }

        public async Task<Response> DeleteAsync(Order entity)
        {
            try
            {
                var order = await context.Orders.FindAsync(entity.Id);
                if (order is null) return new Response(false, "Order not found.");

                context.Orders.Remove(order);
                await context.SaveChangesAsync();
                return new Response(true, "Order deleted successfully.");
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response(false, "An error occurred while deleting the order.");
            }
        }

        public async Task<Order?> FindByIdAsync(int id)
        {
            try
            {
                return await context.Orders
                    .Include(order => order.Items)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(order => order.Id == id);
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the order.", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            try
            {
                return await context.Orders
                    .Include(order => order.Items)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving all orders.", ex);
            }
        }

        public async Task<Response> UpdateAsync(Order entity)
        {
            try
            {
                var order = await context.Orders
                    .Include(existing => existing.Items)
                    .FirstOrDefaultAsync(existing => existing.Id == entity.Id);
                if (order is null) return new Response(false, "Order not found.");

                order.ClientId = entity.ClientId;
                order.OrderDate = entity.OrderDate;

                // Replace the child collection as one aggregate update so removed items cannot remain orphaned.
                context.OrderItems.RemoveRange(order.Items);
                order.Items = entity.Items;
                await context.SaveChangesAsync();
                return new Response(true, "Order updated successfully");
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response(false, "An error occurred while updating the order.");
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(Expression<Func<Order, bool>> predicate)
        {
            try
            {
                return await context.Orders
                    .Include(order => order.Items)
                    .AsNoTracking()
                    .Where(predicate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the order.", ex);
            }
        }

        public async Task<Order?> GetByAsync(Expression<Func<Order, bool>> predicate)
        {
            return await context.Orders
                .Include(order => order.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }
    }
}
