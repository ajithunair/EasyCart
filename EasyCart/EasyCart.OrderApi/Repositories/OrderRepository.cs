using EasyCart.OrderApi.Data;
using EasyCart.OrderApi.Entities;
using EasyCart.OrderApi.Interfaces;
using EasyCart.SharedLibrary.Interfacess;
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

                if (result.Id > 0)
                {
                    return new Response
                    {
                        Success = true,
                        Message = "Order created successfully",
                    };

                }
                else
                {
                    return new Response
                    {
                        Success = false,
                        Message = "Failed to create order"
                    };

                }
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response
                {
                    Success = false,
                    Message = "An error occurred while creating the order."
                };
            }
        }

        public async Task<Response> DeleteAsync(Order entity)
        {
            try
            {
                var order = await context.Orders.FindAsync(entity.Id);
                if (order is null)
                {
                    return new Response
                    {
                        Success = false,
                        Message = "Order not found."
                    };
                }

                context.Orders.Remove(order);
                await context.SaveChangesAsync();

                return new Response
                {
                    Success = true,
                    Message = "Order deleted successfully."
                };  
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response
                {
                    Success = false,
                    Message = "An error occurred while deleting the order."
                };
            }
        }

        public async Task<Order> FindByIdAsync(int id)
        {
            try
            {
                var order = await context.Orders.FindAsync(id);
                if (order is null)
                {
                    return null!;
                }
                return order;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the order.");
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            try
            {
                var orders = await context.Orders.AsNoTracking().ToListAsync();
                if (orders is null || !orders.Any())
                {
                    return Enumerable.Empty<Order>();
                }
                return orders;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving all orders.");
            }
        }

        public async Task<Response> UpdateAsync(Order entity)
        {
            try
            {
                var order = await context.Orders.FindAsync(entity.Id);
                if (order is null)
                {
                    return new Response
                    {
                        Success = false,
                        Message = "Order not found."
                    };
                }
                context.Entry(order).State = EntityState.Detached;
                context.Orders.Update(entity);
                await context.SaveChangesAsync();

                return new Response
                {
                    Success = true,
                    Message = "Order Updated successfully"
                };

            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response
                {
                    Success = false,
                    Message = "An error occurred while updating the order."
                };
            }
        }
        
        public async Task<IEnumerable<Order>> GetOrdersAsync(Expression<Func<Order, bool>> predicate)
        {
            try
            {
                var orders = await context.Orders.AsNoTracking().Where(predicate).ToListAsync();
                if (orders is null || !orders.Any())
                {
                    return Enumerable.Empty<Order>();
                }
                return orders;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the order.");
            }
        }

        public async Task<Order> GetByAsync(Expression<Func<Order, bool>> predicate)
        {
            try
            {
                var order = await context.Orders.AsNoTracking().Where(predicate).FirstOrDefaultAsync();
                if (order is null)
                {
                    return null!;
                }
                return order;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the order.");
            }
        }
    }
}
