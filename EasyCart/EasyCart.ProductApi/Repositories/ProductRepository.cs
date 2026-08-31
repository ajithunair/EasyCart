using EasyCart.ProductApi.Data;
using EasyCart.ProductApi.Entities;
using EasyCart.ProductApi.Interfaces;
using EasyCart.SharedLibrary.Logs;
using EasyCart.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using System.Linq.Expressions;
using System.Text.Json;

namespace EasyCart.ProductApi.Repositories
{
    public class ProductRepository(ProductDbContext context, IDistributedCache? cache = null) : IProduct
    {
        public async Task<Response> CreateAsync(Product entity)
        {
            try
            {
                // The database enforces uniqueness, but this fast pre-check gives a friendlier response in the common case.
                var existingProduct = await GetByAsync(p => p.Name == entity.Name);
                if (existingProduct is not null)
                {
                    return new Response
                    {
                        Success = false,
                        Message = "Product with the same name already exists."
                    };
                }

                await context.Products.AddAsync(entity);
                await context.SaveChangesAsync();

                return new Response { Success = true, Message = $"{entity.Name} created successfully." };
            }
            catch (DbUpdateException ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "A product with this name already exists." };
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
                if (product is null)
                {
                    return new Response { Success = false, Message = "Product not found." };
                }

                context.Products.Remove(product);
                await context.SaveChangesAsync();

                // A cache failure must not undo a successful database operation.
                await TryRemoveFromCacheAsync(GetCacheKey(entity.Id));

                return new Response { Success = true, Message = $"{product.Name} deleted successfully." };
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "An error occurred while deleting the product." };
            }
        }

        public async Task<Product?> FindByIdAsync(int id)
        {
            try
            {
                var cacheKey = GetCacheKey(id);
                var cachedProduct = await TryGetFromCacheAsync(cacheKey);
                if (!string.IsNullOrWhiteSpace(cachedProduct))
                {
                    return JsonSerializer.Deserialize<Product>(cachedProduct);
                }

                var product = await context.Products.FindAsync(id);
                if (product is not null)
                {
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        // Keep the cached entry short-lived so updates become visible quickly.
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    };

                    var serializedProduct = JsonSerializer.Serialize(product);
                    await TrySetInCacheAsync(cacheKey, serializedProduct, cacheOptions);
                }

                return product;
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
                return await context.Products.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                throw new Exception("An error occurred while retrieving all products.", ex);
            }
        }

        public async Task<Product?> GetByAsync(Expression<Func<Product, bool>> predicate)
        {
            try
            {
                return await context.Products.FirstOrDefaultAsync(predicate);
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
                if (product is null)
                {
                    return new Response { Success = false, Message = "Product not found." };
                }

                // Copy the incoming values onto the tracked entity so EF Core can generate a clean update statement.
                product.Name = entity.Name;
                product.Price = entity.Price;
                product.Quantity = entity.Quantity;

                await context.SaveChangesAsync();

                // A cache failure must not turn a successful update into an error response.
                await TryRemoveFromCacheAsync(GetCacheKey(entity.Id));

                return new Response { Success = true, Message = $"{product.Name} updated successfully." };
            }
            catch (DbUpdateException ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "A product with this name already exists." };
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);
                return new Response { Success = false, Message = "An error occurred while updating the product." };
            }
        }

        private static string GetCacheKey(int id) => $"Product:{id}";

        private async Task<string?> TryGetFromCacheAsync(string cacheKey)
        {
            if (cache is null) return null;

            try
            {
                return await cache.GetStringAsync(cacheKey);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Redis cache read failed for {CacheKey}; continuing with PostgreSQL.", cacheKey);
                return null;
            }
        }

        private async Task TrySetInCacheAsync(
            string cacheKey,
            string value,
            DistributedCacheEntryOptions options)
        {
            if (cache is null) return;

            try
            {
                await cache.SetStringAsync(cacheKey, value, options);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Redis cache write failed for {CacheKey}; continuing without caching.", cacheKey);
            }
        }

        private async Task TryRemoveFromCacheAsync(string cacheKey)
        {
            if (cache is null) return;

            try
            {
                await cache.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Redis cache invalidation failed for {CacheKey}; the database change was preserved.", cacheKey);
            }
        }
    }
}
