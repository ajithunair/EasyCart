using EasyCart.AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EasyCart.AuthApi.Data
{
    public class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }
    }
}
