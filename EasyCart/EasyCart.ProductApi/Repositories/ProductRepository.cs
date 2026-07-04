using EasyCart.ProductApi.Data;
using EasyCart.ProductApi.Entities;
using EasyCart.ProductApi.Interfaces;
using EasyCart.SharedLibrary.Logs;
using EasyCart.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Expressions;
using System.Text.Json;

namespace EasyCart.ProductApi.Repositories
{
    public class ProductRepository(ProductDbContext context, IDistributedCache cache) : IProduct
    {
        public async Task<Response> CreateAsync(Product entity)
        {
            try
            {
                var result = await GetByAsync(p => p.Name == entity.Name);
                if (result != null)
                {
                    return new Response
                    {
                        Success = false,
                        Message = "Product with the same name already exists."
                    };
                }
                var product = await context.Products.AddAsync(entity);
                await context.SaveChangesAsync();
                return new Response { Success = true, Message = $"{entity.Name} created successfully." };
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "An error occurred while creating the product." };
            }
        }

        public async Task<Response> DeleteAsync(Product entity)
        {
            try
            {
                var product = await context.Products.FindAsync(entity.Id);
                if (product == null)
                {
                    return new Response { Success = false, Message = "Product not found." };
                }
                else
                {
                    context.Products.Remove(product);
                    await context.SaveChangesAsync();
                    // Invalidate the Cache
                    var cacheKey = $"Product:{entity.Id}";
                    await cache.RemoveAsync(cacheKey);

                    return new Response { Success = true, Message = $"{product.Name} deleted successfully." };
                }
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "An error occurred while deleting the product." };
            }
        }

        public async Task<Product> FindByIdAsync(int id)
        {
            string cacheKey = $"Product:{id}";
            try
            {
                // Try to fetch data from Redis Cache
                string? cachedProduct = await cache.GetStringAsync(cacheKey);
                if(!string.IsNullOrEmpty(cachedProduct))
                {
                    // CACHE HIT: Deserialize the JSON string back into the Product object
                    var productFromCache = JsonSerializer.Deserialize<Product>(cachedProduct);
                    return productFromCache!;
                }

                // CACHE MISS: Fetch from PostgreSQL database
                var product = await context.Products.FindAsync(id);

                if(product != null)
                {
                    // Configure Cache Expiration Options
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        // Absolute Expiration: The cache expires exactly 10 minutes from now no matter what
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                        // Sliding Expiration: Keeps the cache alive for an extra 2 mins if someone accesses it
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    };

                    // Serialize object to JSON string and save to Redis
                    string serializedProduct = JsonSerializer.Serialize(product);
                    await cache.SetStringAsync(cacheKey, serializedProduct, cacheOptions);

                }
                return product!;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the product by ID.", ex);
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {

            try
            {
                var products = await context.Products.AsNoTracking().ToListAsync();
                return products;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving all products.", ex);
            }
        }

        public async Task<Product> GetByAsync(Expression<Func<Product, bool>> predicate)
        {
            try
            {
                var product = await context.Products.FirstOrDefaultAsync(predicate);
                return product!;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving the product.", ex);
            }
        }

        public async Task<Response> UpdateAsync(Product entity)
        {
            try
            {
                var product = await context.Products.FindAsync(entity.Id);
                if (product == null)
                {
                    return new Response { Success = false, Message = "Product not found." };
                }

                // Update the product properties
                product.Name = entity.Name;
                product.Price = entity.Price;
                product.Quantity = entity.Quantity;

                await context.SaveChangesAsync();
                // Invalidate the Cache
                var cacheKey = $"Product:{entity.Id}";
                await cache.RemoveAsync(cacheKey);

                return new Response { Success = true, Message = $"{product.Name} updated successfully." };
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "An error occurred while updating the product." };
            }
        }
    }
}
