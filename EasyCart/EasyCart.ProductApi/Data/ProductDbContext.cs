using EasyCart.ProductApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyCart.ProductApi.Data
{
    public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product names act like a business key, so the database should enforce uniqueness.
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique();
        }
    }
}
