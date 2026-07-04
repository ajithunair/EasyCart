using EasyCart.InventoryApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyCart.InventoryApi.Data
{
    public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
    {
        public DbSet<Inventory> Inventories { get; set; }
    }
}
