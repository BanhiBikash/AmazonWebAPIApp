using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AmazonWeb.Infrastructure.DBContext
{
    public class ApplicationDBContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options) { }

        // DbSets for your aggregates
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Example: configure relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<OrderItem>(entity =>
            {
                // Composite primary key (OrderId + ProductId)
                entity.HasKey(oi => new { oi.OrderId, oi.ProductId });

                // Relationship: Order → OrderItems
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrderId);
            });

            // Optional: global query filter for soft delete
            modelBuilder.Entity<ApplicationUser>()
                .HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
