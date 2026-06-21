using EasyCart.ProductApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyCart.ProductApi.Data
{
    public class ProductDbContext(DbContextOptions<ProductDbContext> options): DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
    }
}
