using EasyCart.AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EasyCart.AuthApi.Data
{
    public class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RefreshToken>(entity => {
                entity.HasKey(rt => rt.Id);

                entity.HasIndex(rt => rt.TokenHash).IsUnique();

                entity.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(128);

                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
