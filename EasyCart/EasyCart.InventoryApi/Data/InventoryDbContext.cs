using EasyCart.InventoryApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyCart.InventoryApi.Data
{
    public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
    {
        public DbSet<Inventory> Inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One inventory row per product keeps the stock ledger unambiguous.
            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.ProductId)
                .IsUnique();
        }
    }
}
