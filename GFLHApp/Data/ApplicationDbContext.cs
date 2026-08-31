using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GFLHApp.Models;
using System.Linq;

namespace GFLHApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Basket> Basket { get; set; } = default!;
        public DbSet<BasketProducts> BasketProducts { get; set; } = default!;
        public DbSet<OrderProducts> OrderProducts { get; set; } = default!;
        public DbSet<Orders> Orders { get; set; } = default!;
        public DbSet<ProducerOrders> ProducerOrders { get; set; } = default!;
        public DbSet<Producers> Producers { get; set; } = default!;
        public DbSet<Products> Products { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Enforce uniform currency precision (10 digits, 2 decimal places) across all decimal fields
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(10,2)");
            }

            // Producers.UserId must be unique to allow referencing it as a principal key from ProducerOrders
            builder.Entity<Producers>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            // Link ProducerOrders slice to Producers entity using the Producer's Identity UserId
            builder.Entity<ProducerOrders>()
                .HasOne(po => po.Producers)
                .WithMany()
                .HasForeignKey(po => po.ProducerId)
                .HasPrincipalKey(p => p.UserId);
        }
    }
}