using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

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
        public DbSet<CartItem> CartItems { get; set; } // 🛒 Registered CartItem Aggregate Dataset
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Order Relationships
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

                // Relationship: Order → OrderItems
                entity.HasMany(o => o.Items);
            });
                
            // Configure OrderItem Relationships
            modelBuilder.Entity<OrderItem>(entity =>
            {
                // Composite primary key (OrderId + ProductId)
                entity.HasKey(oi => new { oi.OrderId, oi.ProductId });

            });

            // 🛒 Configure CartItem Table Constraints & Indexes
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);

                // 🎯 UNIQUE COMPOSITE INDEX: Prevents redundant item rows for the same user.
                // If a product already exists in a user's cart, your API code should just increment its quantity.
                entity.HasIndex(ci => new { ci.UserId, ci.ProductId }).IsUnique();

                // Relationship: User → CartItems
                entity.HasOne(ci => ci.User)
                      .WithMany(u => u.CartItems) // Make sure 'public List<CartItem> CartItems { get; set; }' exists in ApplicationUser!
                      .HasForeignKey(ci => ci.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Wipes out cart data if the profile is deleted

                // Relationship: Product → CartItems
                entity.HasOne(ci => ci.Product)
                      .WithMany()
                      .HasForeignKey(ci => ci.ProductId);
            });

            // Global query filter for soft delete
            modelBuilder.Entity<ApplicationUser>()
                .HasQueryFilter(u => !u.IsDeleted);

            //Transaction table configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId);

                // 1. Configure Relationship: User → Transactions
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Transactions)
                      .HasForeignKey(t => t.UserId)             // 🎯 Point to your clean UserId column
                      .OnDelete(DeleteBehavior.Restrict);       // Safe tracking audit trail behavior

                // 2. Configure Relationship: Order → Transactions (If you want EF to know OrderId is a Foreign Key)
                // Assuming you don't have a virtual "Order" navigation property inside Transaction, you can set it up anonymously:
                entity.HasOne<Order>()
                      .WithMany()
                      .HasForeignKey(t => t.OrderId)            // 🎯 Point to your clean OrderId column
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}