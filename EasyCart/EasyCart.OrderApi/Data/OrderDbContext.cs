using EasyCart.OrderApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyCart.OrderApi.Data
{
    public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders { get; set; }
    }
}
